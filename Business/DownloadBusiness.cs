using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using YoutubeExtractor;
using DataAccess;
using System.Net;

namespace Business {
    public class DownloadBusiness {
        private ObservableCollection<DownloadItem> downloadsList = new ObservableCollection<DownloadItem>();

        public ObservableCollection<DownloadItem> DownloadsList {
            get { return downloadsList; }
        }

        public event EventHandler DownloadAdded;

        /// <summary>
        /// Downloads specified video from YouTube.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="queuePos">The position in the queue to auto-play, or -1.</param>
        /// <param name="callback">The method to call once download is completed.</param>
        public async Task DownloadVideoAsync(Media video, int queuePos, EventHandler<DownloadCompletedEventArgs> callback) {
            if (video == null || string.IsNullOrEmpty(video.DownloadUrl))
                throw new ArgumentException("Video object is null or doesn't contain a valid YouTube URL.");

            if (IsDownloadDuplicate(video))
                return;

            // Store in the Downloads folder.
            DefaultMediaPath PathCalc = new DefaultMediaPath(Settings.TempFilesPath.Substring(Settings.NaturalGroundingFolder.Length));
            string Destination = Settings.NaturalGroundingFolder + PathCalc.GetDefaultFileName(video.Artist, video.Title, null, (MediaType)video.MediaTypeId);
            string DownloadDesc = Path.GetFileName(Destination);
            Directory.CreateDirectory(Settings.TempFilesPath);

            // Add DownloadItem right away before doing any async work.
            DownloadItem DownloadInfo = new DownloadItem(video, Destination, DownloadDesc, queuePos, callback);
            System.Windows.Application.Current.Dispatcher.Invoke(() => downloadsList.Insert(0, DownloadInfo));

            // Notify UI of new download to show window.
            if (DownloadAdded != null)
                DownloadAdded(this, new EventArgs());

            if (downloadsList.Where(d => d.Status == DownloadStatus.Downloading || d.Status == DownloadStatus.Initializing).Count() < Settings.SimultaneousDownloads)
                await StartDownloadAsync(DownloadInfo).ConfigureAwait(false);
        }

        private async Task StartDownloadAsync(DownloadItem downloadInfo) {
            downloadInfo.Status = DownloadStatus.Initializing;

            // Query the download URL for the right file.
            var VTask = GetDownloadUrlsAsync(downloadInfo.Request.DownloadUrl);
            var VideoList = await VTask.ConfigureAwait(false);
            if (VideoList == null) {
                downloadInfo.Status = DownloadStatus.Failed;
                RaiseCallback(downloadInfo);
                return;
            }

            // Get the highest resolution format.
            int MaxResolution = (from v in VideoList
                                 where (Settings.SavedFile.MaxDownloadQuality == 0 || v.Resolution <= Settings.SavedFile.MaxDownloadQuality) &&
                                    (v.AdaptiveType == AdaptiveType.Video || (v.VideoType != VideoType.Mp4 && v.VideoType != VideoType.WebM))
                                 orderby v.Resolution descending
                                 select v.Resolution).FirstOrDefault();
            if (MaxResolution == 0)
                return;

            // Select format in this order: WebM, Mp4, or Flash.
            VideoInfo VideoFile = await SelectBestFormat(VideoList.Where(v => v.Resolution == MaxResolution));

            if (VideoFile.AdaptiveType == AdaptiveType.None) {
                // Download single file.
                downloadInfo.Files.Add(new DownloadItem.FileProgress() {
                    Source = VideoFile,
                    Destination = downloadInfo.Destination + VideoFile.VideoExtension
                });
                await DownloadFilesAsync(downloadInfo, downloadInfo.Callback).ConfigureAwait(false);
            } else {
                // Download audio and video separately.
                VideoInfo AudioFile = SelectBestAudio(from v in VideoList
                                       where (v.CanExtractAudio || v.AdaptiveType == AdaptiveType.Audio)
                                       orderby v.AudioBitrate descending
                                       select v);
                downloadInfo.Files.Add(new DownloadItem.FileProgress() {
                    Source = VideoFile,
                    Destination = downloadInfo.Destination + GetCodecExtension(VideoFile.VideoType)
                });
                if (AudioFile != null) {
                    downloadInfo.Files.Add(new DownloadItem.FileProgress() {
                        Source = AudioFile,
                        Destination = downloadInfo.Destination + GetAudioExtension(AudioFile.AudioType)
                    });
                }
                await DownloadFilesAsync(downloadInfo, downloadInfo.Callback).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Returns the best format from the list in this order of availability: WebM, Mp4 or Flash.
        /// Mp4 will be chosen if WebM is over 30% smaller.
        /// </summary>
        /// <param name="list">The list of videos to chose from.</param>
        /// <returns>The best format available.</returns>
        public static async Task<VideoInfo> SelectBestFormat(IEnumerable<VideoInfo> list) {
            BestFormatInfo Result = await SelectBestFormatWithStatus(list);
            if (Result != null)
                return Result.BestFile;
            else
                return null;
        }

        /// <summary>
        /// Returns the best format from the list in this order of availability: WebM, Mp4 or Flash.
        /// Mp4 will be chosen if WebM is over 35% smaller.
        /// </summary>
        /// <param name="list">The list of videos to chose from.</param>
        /// <returns>The best format available.</returns>
        public static async Task<BestFormatInfo> SelectBestFormatWithStatus(IEnumerable<VideoInfo> list) {
            BestFormatInfo Result = new BestFormatInfo();
            VideoInfo WebmFile = list.FirstOrDefault(v => v.VideoType == VideoType.WebM && v.AdaptiveType == AdaptiveType.Video);
            VideoInfo Mp4File = list.FirstOrDefault(v => v.VideoType == VideoType.Mp4 && v.AdaptiveType == AdaptiveType.Video);
            VideoInfo FlvFile = list.FirstOrDefault(v => v.VideoType == VideoType.Flash);
            long WebmFileSize = 0;
            long Mp4FileSize = 0;
            if (WebmFile != null)
                WebmFileSize = await GetDownloadSize(WebmFile);
            if (Mp4File != null)
                Mp4FileSize = await GetDownloadSize(Mp4File);

            // If both WebM and MP4 are available, select WebM unless it is over 35% smaller than MP4.
            if (WebmFile != null && Mp4File != null) {
                // Only return WebM if it is no more than 35% smaller than MP4.
                float WebmSmallerRatio = 1 - ((float)WebmFileSize / Mp4FileSize);
                if (WebmSmallerRatio < .36) {
                    Result.BestFile = WebmFile;
                    Result.Size = WebmFileSize;
                } else {
                    Result.BestFile = Mp4File;
                    Result.Size = Mp4FileSize;
                    Result.StatusText = string.Format("WebM {0:p0} smaller", WebmSmallerRatio);
                }
            }

            if (Result.BestFile == null) {
                // Return video in order of preference.
                if (WebmFile != null) {
                    Result.BestFile = WebmFile;
                    Result.Size = WebmFileSize;
                } else if (Mp4File != null) {
                    Result.BestFile = Mp4File;
                    Result.Size = Mp4FileSize;
                } else if (FlvFile != null)
                    Result.BestFile = FlvFile;
                else
                    Result.BestFile = list.FirstOrDefault();

                if (Result.BestFile != null && Result.Size == 0)
                    Result.Size = await GetDownloadSize(Result.BestFile);
            }

            if (Result.BestFile != null) {
                // When download H264 video, see whether OGG (Vorbis) audio is available.
                if (WebmFile != null && (Result.BestFile.VideoType == VideoType.Mp4 || Result.BestFile.VideoType == VideoType.WebM))
                    Result.HasVorbisAudio = true;
                return Result;
            }  else
                return null;
        }

        /// <summary>
        /// Selects OGG (Vorbis) audio if available, otherwise AAC.
        /// </summary>
        /// <param name="list">The list of available audios.</param>
        /// <returns>The audio to download.</returns>
        public static VideoInfo SelectBestAudio(IEnumerable<VideoInfo> list) {
            VideoInfo Result = list.Where(v => v.AudioType == AudioType.Vorbis).FirstOrDefault();
            if (Result == null)
                Result = list.Where(v => v.AudioType == AudioType.Aac).FirstOrDefault();
            if (Result == null)
                Result = list.FirstOrDefault();
            return Result;
        }

        /// <summary>
        /// Queries YouTube for the size of the video stream.
        /// </summary>
        /// <param name="file">The video stream to get the size of.</param>
        /// <returns>The video stream size in bytes, excluding the audio.</returns>
        public static async Task<long> GetDownloadSize(VideoInfo file) {
            long Result = 0;
            Exception QueryEx = null;
            for (int i = 0; i < 3; i++) {
                try {
                    Result = await Task.Run(() => {
                        if (file.RequiresDecryption)
                            DownloadUrlResolver.DecryptDownloadUrl(file);
                        var request = (HttpWebRequest)WebRequest.Create(file.DownloadUrl);
                        using (WebResponse response = request.GetResponse()) {
                            return response.ContentLength;
                        }
                    });
                    break;
                } catch (Exception ex) {
                    QueryEx = ex;
                }
                await Task.Delay(1000);
            }
            if (Result == 0 && QueryEx != null)
                throw QueryEx;
            return Result;
        }

        /// <summary>
        /// Returns the file extension for specified video codec type.
        /// To avoid conflicting file names, the codec extension must be different than the final extension.
        /// </summary>
        /// <param name="video">The video type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        public string GetCodecExtension(VideoType video) {
            if (video == VideoType.WebM)
                return ".vp9";
            else if (video == VideoType.Mp4)
                return ".h264";
            else if (video == VideoType.Flash)
                return ".flv";
            else if (video == VideoType.Mobile)
                return "";
            else
                return ".avi";
        }

        /// <summary>
        /// Returns the file extension for specified video type.
        /// To avoid conflicting file names, the codec extension must be different than the final extension.
        /// </summary>
        /// <param name="video">The video type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        public string GetFinalExtension(VideoType video, AudioType audio) {
            if (video == VideoType.WebM)
                return ".webm";
            else if (video == VideoType.Mp4 && audio == AudioType.Aac)
                return ".mp4";
            else if (video == VideoType.Mp4 && audio == AudioType.Vorbis)
                return ".mkv";
            else
                return GetCodecExtension(video);
        }

        /// <summary>
        /// Returns the file extension for specified audio type.
        /// </summary>
        /// <param name="audio">The audio type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        public string GetAudioExtension(AudioType audio) {
            if (audio == AudioType.Vorbis)
                return ".ogg";
            else if (audio == AudioType.Aac)
                return ".aac";
            else if (audio == AudioType.Mp3)
                return ".mp3";
            else
                return ".mp3";
        }

        /// <summary>
        /// Returns the list of download URLs from YouTube.
        /// </summary>
        /// <param name="url">The YouTube video URL to query.</param>
        /// <returns>A list of VideoInfo objects.</returns>
        public static Task<IEnumerable<VideoInfo>> GetDownloadUrlsAsync(string url) {
            return Task.Run<IEnumerable<VideoInfo>>(() => {
                try {
                    return YoutubeExtractor.DownloadUrlResolver.GetDownloadUrls(url, false);
                } catch {
                    return null;
                }
            });
        }

        /// <summary>
        /// Downloads the specified list of files.
        /// </summary>
        /// <param name="downloadInfo">The information about the files to download.</param>
        /// <param name="callback">The function to call when download is completed.</param>
        private async Task DownloadFilesAsync(DownloadItem downloadInfo, EventHandler<DownloadCompletedEventArgs> callback) {
            // Decrypt all URLs at the same time without waiting.
            List<Task> DecryptTasks = new List<Task>();
            foreach (var item in downloadInfo.Files) {
                DecryptTasks.Add(Task.Run(() => DownloadUrlResolver.DecryptDownloadUrl(item.Source)));
            }
            await Task.WhenAll(DecryptTasks.ToArray()).ConfigureAwait(false);

            if (!downloadInfo.IsCanceled) {
                // Download all files.
                List<Task> DownloadTasks = new List<Task>();
                foreach (var item in downloadInfo.Files) {
                    DownloadTasks.Add(DownloadVideoAsync(downloadInfo, item, callback));
                }
                await Task.WhenAll(DownloadTasks.ToArray()).ConfigureAwait(false);
            } else
                RaiseCallback(downloadInfo);
        }

        private async Task DownloadVideoAsync(DownloadItem downloadInfo, DownloadItem.FileProgress fileInfo, EventHandler<DownloadCompletedEventArgs> callback) {
            downloadInfo.Status = DownloadStatus.Downloading;
            VideoDownloader YTD = new VideoDownloader(fileInfo.Source, fileInfo.Destination);
            YTD.DownloadProgressChanged += (sender, e) => {
                if (downloadInfo.IsCanceled)
                    e.Cancel = true;
                else {
                    fileInfo.BytesTotal = YTD.DownloadSize;
                    fileInfo.BytesDownloaded = e.ProgressBytes;
                    downloadInfo.UpdateProgress();
                }
            };

            // Run downloader task.
            await Task.Run(() => {
                try {
                    YTD.Execute();
                } catch {
                    downloadInfo.Status = DownloadStatus.Failed;
                }
            }).ConfigureAwait(false);

            // Detect whether this is the last file.
            fileInfo.Done = true;
            if (downloadInfo.Files.Any(d => !d.Done) == false) {
                // Raise events for the last file part only.
                if (downloadInfo.IsCompleted)
                    await DownloadCompletedAsync(downloadInfo).ConfigureAwait(false);
                else if (downloadInfo.IsCanceled)
                    await DownloadCanceledAsync(downloadInfo).ConfigureAwait(false);
                RaiseCallback(downloadInfo);

                await StartNextDownloadAsync().ConfigureAwait(false);
            }
        }

        private async Task DownloadCompletedAsync(DownloadItem downloadInfo) {
            downloadInfo.Status = DownloadStatus.Done;
            string Destination = downloadInfo.Files[0].Destination;
            Media video = downloadInfo.Request;

            if (downloadInfo.Files.Count > 1) {
                // Remove extension.
                Destination = Destination.Substring(0, Destination.Length - Path.GetExtension(Destination).Length);

                VideoType File1Type = downloadInfo.Files[0].Source.VideoType;
                AudioType File2Type = downloadInfo.Files[1].Source.AudioType;
                string File1Ext = GetCodecExtension(File1Type);
                string File2Ext = GetAudioExtension(File2Type);
                string OutputExt = GetFinalExtension(File1Type, File2Type);

                // Make sure output file doesn't already exist.
                File.Delete(Destination + OutputExt);

                // Merge audio and video files.
                FfmpegBusiness.JoinAudioVideo(Destination + File1Ext, Destination + File2Ext, Destination + OutputExt, true);

                // Delete source files
                File.Delete(Destination + File1Ext);
                File.Delete(Destination + File2Ext);

                // Move file
                DefaultMediaPath PathCalc = new DefaultMediaPath();
                string NewFileName = PathCalc.GetDefaultFileName(video.Artist, video.Title, video.MediaCategoryId, (MediaType)video.MediaTypeId);
                Directory.CreateDirectory(Path.GetDirectoryName(Settings.NaturalGroundingFolder + NewFileName));
                video.FileName = NewFileName + OutputExt;
                File.Move(Destination + OutputExt, Settings.NaturalGroundingFolder + video.FileName);
            } else {
                // Move single file
                DefaultMediaPath PathCalc = new DefaultMediaPath();
                string NewFileName = PathCalc.GetDefaultFileName(video.Artist, video.Title, video.MediaCategoryId, (MediaType)video.MediaTypeId);
                Directory.CreateDirectory(Path.GetDirectoryName(Settings.NaturalGroundingFolder + NewFileName));
                video.FileName = NewFileName + Path.GetExtension(Destination);
                File.Move(Destination, Settings.NaturalGroundingFolder + video.FileName);
            }

            // Add to database
            EditVideoBusiness Business = new EditVideoBusiness();
            Media ExistingData = Business.GetVideoById(video.MediaId);
            if (ExistingData != null) {
                // Edit video info.
                ExistingData.FileName = video.FileName;
                Business.Save();
            } else {
                // Add new video info.
                Business.AddVideo(video);
                Business.Save();
            }
            downloadInfo.Request.FileName = video.FileName;
        }

        private async Task DownloadCanceledAsync(DownloadItem downloadInfo) {
            if (downloadInfo.Status == DownloadStatus.Canceled)
                downloadInfo.Status = DownloadStatus.Canceled;
            else if (downloadInfo.Status == DownloadStatus.Failed)
                downloadInfo.Status = DownloadStatus.Failed;
            Thread.Sleep(100);

            // Delete partially-downloaded files.
            foreach (var item in downloadInfo.Files) {
                File.Delete(item.Destination);
            }
        }

        private async Task StartNextDownloadAsync() {
            // Start next download.
            DownloadItem NextDownload = downloadsList.Where(d => d.Status == DownloadStatus.Waiting).LastOrDefault();
            if (NextDownload != null)
                await StartDownloadAsync(NextDownload).ConfigureAwait(false);
        }

        public bool IsDownloadDuplicate(Media request) {
            bool Result = (from d in this.downloadsList
                           where (d.Status == DownloadStatus.Downloading || d.Status == DownloadStatus.Initializing || d.Status == DownloadStatus.Waiting) &&
                             (string.Compare(d.Request.DownloadUrl, request.DownloadUrl, true) == 0 || (string.Compare(d.Request.Artist, request.Artist, true) == 0 && string.Compare(d.Request.Title, request.Title, true) == 0))
                           select d).Any();
            return Result;
        }

        private void RaiseCallback(DownloadItem downloadInfo) {
            if (downloadInfo.Callback != null)
                Application.Current.Dispatcher.Invoke(() => downloadInfo.Callback(this, new DownloadCompletedEventArgs(downloadInfo)));
        }
    }

    public class DownloadCompletedEventArgs {
        public DownloadCompletedEventArgs() {
        }

        public DownloadCompletedEventArgs(DownloadItem downloadInfo) {
            this.DownloadInfo = downloadInfo;
        }

        public DownloadItem DownloadInfo { get; set; }
    }

    public class BestFormatInfo {
        public VideoInfo BestFile { get; set; }
        public long Size { get; set; }
        public bool HasVorbisAudio { get; set; }
        public string StatusText { get; set; }
    }
}
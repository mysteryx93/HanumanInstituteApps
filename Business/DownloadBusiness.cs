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
        public static string[] DownloadedExtensions = new string[] { ".mp4", ".webm", ".mkv", ".flv" };

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
            await DownloadVideoAsync(video, queuePos, false, callback);
        }

        /// <summary>
        /// Downloads specified video from YouTube.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="queuePos">The position in the queue to auto-play, or -1.</param>
        /// <param name="upgradeAudio">If true, only the audio will be downloaded and it will be merged with the local video file.</param>
        /// <param name="callback">The method to call once download is completed.</param>
        public async Task DownloadVideoAsync(Media video, int queuePos, bool upgradeAudio, EventHandler<DownloadCompletedEventArgs> callback) {
            if (video == null || string.IsNullOrEmpty(video.DownloadUrl))
                throw new ArgumentException("Video object is null or doesn't contain a valid YouTube URL.");

            if (IsDownloadDuplicate(video))
                return;

            // Store in the Temp folder.
            DefaultMediaPath PathCalc = new DefaultMediaPath(Settings.TempFilesPath.Substring(Settings.NaturalGroundingFolder.Length));
            string Destination = Settings.NaturalGroundingFolder + PathCalc.GetDefaultFileName(video.Artist, video.Title, null, (MediaType)video.MediaTypeId);
            string DownloadDesc = Path.GetFileName(Destination);
            Directory.CreateDirectory(Settings.TempFilesPath);

            // Add DownloadItem right away before doing any async work.
            DownloadItem DownloadInfo = new DownloadItem(video, Destination, DownloadDesc, queuePos, upgradeAudio, callback);
            Application.Current.Dispatcher.Invoke(() => downloadsList.Insert(0, DownloadInfo));

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

            if (!downloadInfo.UpgradeAudio || downloadInfo.Request.FileName == null || !File.Exists(Settings.NaturalGroundingFolder + downloadInfo.Request.FileName)) {
                downloadInfo.UpgradeAudio = false;
                // Get the highest resolution format.

                BestFormatInfo BestFile = SelectBestFormat(VideoList);

                if (BestFile.BestVideo.AdaptiveType == AdaptiveType.None) {
                    // Download single file.
                    downloadInfo.Files.Add(new DownloadItem.FileProgress() {
                        Source = BestFile.BestVideo,
                        Destination = downloadInfo.Destination + BestFile.BestVideo.VideoExtension
                    });
                } else {
                    // Download audio and video separately.
                    downloadInfo.Files.Add(new DownloadItem.FileProgress() {
                        Source = BestFile.BestVideo,
                        Destination = downloadInfo.Destination + GetCodecExtension(BestFile.BestVideo.VideoType)
                    });
                    if (BestFile.BestAudio != null) {
                        downloadInfo.Files.Add(new DownloadItem.FileProgress() {
                            Source = BestFile.BestAudio,
                            Destination = downloadInfo.Destination + GetAudioExtension(BestFile.BestAudio.AudioType)
                        });
                    }
                }
            } else if (File.Exists(Settings.NaturalGroundingFolder + downloadInfo.Request.FileName)) {
                // Keep local video and upgrade audio.
                VideoInfo AudioFile = SelectBestAudio(from v in VideoList
                                                      where (v.CanExtractAudio || v.AdaptiveType == AdaptiveType.Audio)
                                                      orderby v.AudioBitrate descending
                                                      select v);
                downloadInfo.Files.Add(new DownloadItem.FileProgress() {
                    Source = AudioFile,
                    Destination = downloadInfo.Destination + GetAudioExtension(AudioFile.AudioType)
                });
            }

            await DownloadFilesAsync(downloadInfo, downloadInfo.Callback).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the best format from the list in this order of availability: WebM, Mp4 or Flash.
        /// Mp4 will be chosen if WebM is over 35% smaller.
        /// </summary>
        /// <param name="list">The list of videos to chose from.</param>
        /// <returns>The best format available.</returns>
        public static BestFormatInfo SelectBestFormat(IEnumerable<VideoInfo> list) {
            var MaxResolutionList = (from v in list
                                     where (Settings.SavedFile.MaxDownloadQuality == 0 || v.Resolution <= Settings.SavedFile.MaxDownloadQuality)
                                        && v.AdaptiveType != AdaptiveType.Audio
                                     orderby v.Resolution descending
                                     select v).ToList();
            MaxResolutionList = MaxResolutionList.Where(v => v.Resolution == MaxResolutionList.First().Resolution).ToList();

            // If for the maximum resolution, only some formats have a file size, the other ones must be queried.
            if (MaxResolutionList.Any(v => v.FileSize == 0) && MaxResolutionList.Any(v => v.FileSize > 0)) {
                foreach (VideoInfo item in MaxResolutionList.Where(v => v.FileSize == 0)) {
                    DownloadUrlResolver.QueryStreamSize(item);
                }
            }

            VideoInfo BestVideo = (from v in MaxResolutionList
                                   // WebM VP9 encodes ~35% better. non-DASH is VP8 and isn't better than MP4.
                                   let Preference = (int)((v.VideoType == VideoType.WebM && v.AdaptiveType == AdaptiveType.Video) ? v.FileSize * 1.35 : v.FileSize)
                                   where v.Resolution == MaxResolutionList.First().Resolution
                                   orderby Preference descending
                                   select v).FirstOrDefault();

            if (BestVideo != null) {
                BestFormatInfo Result = new BestFormatInfo();
                Result.BestVideo = BestVideo;
                // Even for non-DASH videos, we still want to know what audio is available even though it may not be downloaded.
                Result.BestAudio = SelectBestAudio(from v in list
                                                   where (v.CanExtractAudio || v.AdaptiveType == AdaptiveType.Audio)
                                                   orderby v.AudioBitrate descending
                                                   select v);
                return Result;
            } else
                return null;
        }

        /// <summary>
        /// Selects OGG (Vorbis) audio if available, otherwise AAC.
        /// </summary>
        /// <param name="list">The list of available audios.</param>
        /// <returns>The audio to download.</returns>
        public static VideoInfo SelectBestAudio(IEnumerable<VideoInfo> list) {
            if (list == null || list.Count() == 0)
                return null;

            int MaxBitrate = list.OrderByDescending(v => v.AudioBitrate).Max(v => v.AudioBitrate);
            list = list.Where(v => v.AudioBitrate == MaxBitrate);
            VideoInfo Result = list.Where(v => v.AudioType == AudioType.Opus).FirstOrDefault();
            if (Result == null)
                Result = list.Where(v => v.AudioType == AudioType.Opus).FirstOrDefault();
            if (Result == null)
                Result = list.Where(v => v.AudioType == AudioType.Aac).FirstOrDefault();
            if (Result == null)
                Result = list.FirstOrDefault();
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
            if (video == VideoType.WebM && (audio == AudioType.Vorbis || audio == AudioType.Opus))
                return ".webm";
            else if (video == VideoType.Mp4 && audio == AudioType.Aac)
                return ".mp4";
            else if (video == VideoType.Mp4 || video == VideoType.WebM)
                return ".mkv";
            else
                return GetCodecExtension(video);
        }

        /// <summary>
        /// Returns the file extension for specified video type.
        /// To avoid conflicting file names, the codec extension must be different than the final extension.
        /// </summary>
        /// <param name="video">The video type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        public string GetFinalExtension(string video, string audio) {
            video = video.ToLower();
            audio = audio.ToLower();
            if (video == ".vp8" || video == ".vp9")
                video = ".webm";
            if (video == ".h264")
                video = ".mp4";

            if (video == ".webm" && (audio == ".ogg" || audio == ".opus"))
                return ".webm";
            else if (video == ".mp4" && audio == ".aac")
                return ".mp4";
            else if (video == ".mp4" || video == ".webm")
                return ".mkv";
            else
                return video;
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
                if (item.Source.RequiresDecryption)
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
                var NextDownload = StartNextDownloadAsync().ConfigureAwait(false);

                // Raise events for the last file part only.
                if (downloadInfo.IsCompleted) {
                    try {
                        await DownloadCompletedAsync(downloadInfo).ConfigureAwait(false);
                    } catch {
                        downloadInfo.Status = DownloadStatus.Failed;
                    }
                } else if (downloadInfo.IsCanceled)
                    DownloadCanceled(downloadInfo);
                RaiseCallback(downloadInfo);

                await NextDownload;
            }
        }

        private async Task DownloadCompletedAsync(DownloadItem downloadInfo) {
            // Separate file extension.
            string Destination = downloadInfo.Files[0].Destination;
            string DestinationExt = Path.GetExtension(Destination);
            Destination = Destination.Substring(0, Destination.Length - Path.GetExtension(Destination).Length);
            Media video = downloadInfo.Request;

            if (downloadInfo.Files.Count > 1) {
                VideoType File1Type = downloadInfo.Files[0].Source.VideoType;
                AudioType File2Type = downloadInfo.Files[1].Source.AudioType;
                string File1Ext = GetCodecExtension(File1Type);
                string File2Ext = GetAudioExtension(File2Type);
                DestinationExt = GetFinalExtension(File1Type, File2Type);

                // Make sure output file doesn't already exist.
                File.Delete(Destination + DestinationExt);

                // Merge audio and video files.
                await Task.Run(() => FfmpegBusiness.JoinAudioVideo(Destination + File1Ext, Destination + File2Ext, Destination + DestinationExt, false, true));

                // Delete source files
                File.Delete(Destination + File1Ext);
                File.Delete(Destination + File2Ext);
            } else if (downloadInfo.UpgradeAudio) {
                // Get original video format.
                MediaInfoReader MediaReader = new MediaInfoReader();
                //await MediaReader.LoadInfoAsync(Settings.NaturalGroundingFolder + downloadInfo.Request.FileName);
                string VideoDestExt = ".mkv";
                //if (MediaReader.VideoFormat == "VP8" || MediaReader.VideoFormat == "VP9")
                //    VideoDestExt = ".webm";
                string VideoDest = downloadInfo.Destination + " (extract)" + VideoDestExt;

                // Keep existing video and upgrade audio.
                string AudioExt = GetAudioExtension(downloadInfo.Files[0].Source.AudioType);
                DestinationExt = GetFinalExtension(Path.GetExtension(downloadInfo.Request.FileName), AudioExt);

                // Merge audio and video files.
                await Task.Run(() => {
                    FfmpegBusiness.ExtractVideo(Settings.NaturalGroundingFolder + downloadInfo.Request.FileName, VideoDest, true);
                    if (FileHasContent(VideoDest))
                        FfmpegBusiness.JoinAudioVideo(VideoDest, Destination + AudioExt, Destination + DestinationExt, false, true);
                });

                // Delete source files
                File.Delete(VideoDest);
                File.Delete(Destination + AudioExt);

                if (FileHasContent(Destination + DestinationExt) && File.Exists(Settings.NaturalGroundingFolder + downloadInfo.Request.FileName))
                    FileOperationAPIWrapper.MoveToRecycleBin(Settings.NaturalGroundingFolder + downloadInfo.Request.FileName);
            }

            // Ensure download and merge succeeded.
            if (FileHasContent(Destination + DestinationExt)) {
                // Get final file name.
                DefaultMediaPath PathCalc = new DefaultMediaPath();
                string NewFileName = PathCalc.GetDefaultFileName(video.Artist, video.Title, video.MediaCategoryId, (MediaType)video.MediaTypeId);
                Directory.CreateDirectory(Path.GetDirectoryName(Settings.NaturalGroundingFolder + NewFileName));
                video.FileName = NewFileName + DestinationExt;

                // Move file and overwrite.
                if (downloadInfo.Request.FileName != null && File.Exists(Settings.NaturalGroundingFolder + downloadInfo.Request.FileName))
                    FileOperationAPIWrapper.MoveToRecycleBin(Settings.NaturalGroundingFolder + downloadInfo.Request.FileName);
                File.Move(Destination + DestinationExt, Settings.NaturalGroundingFolder + video.FileName);
            } else
                throw new FileNotFoundException("Audio and video streams could not be merged.");

            // Add to database
            EditVideoBusiness Business = new EditVideoBusiness();
            Media ExistingData = Business.GetVideoById(video.MediaId);
            if (ExistingData != null) {
                // Edit video info.
                ExistingData.FileName = video.FileName;
                ExistingData.Length = null;
                ExistingData.Height = null;
                Business.Save();
            } else {
                // Add new video info.
                Business.AddVideo(video);
                Business.Save();
            }
            downloadInfo.Request.FileName = video.FileName;

            downloadInfo.Status = DownloadStatus.Done;
        }

        private void DownloadCanceled(DownloadItem downloadInfo) {
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

        /// <summary>
        /// Returns whether specified file exists and contains data (at least 500KB).
        /// </summary>
        /// <param name="fileName">The path of the file to check.</param>
        /// <returns>Whether the file contains data.</returns>
        private bool FileHasContent(string fileName) {
            return File.Exists(fileName) && new FileInfo(fileName).Length > 524288;
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
        public VideoInfo BestVideo { get; set; }
        public VideoInfo BestAudio { get; set; }
        public string StatusText { get; set; }
    }
}
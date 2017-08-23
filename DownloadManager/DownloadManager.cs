using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EmergenceGuardian.FFmpeg;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace EmergenceGuardian.Downloader {
    public class DownloadManager {
        private YoutubeClient youtube = new YoutubeClient();
        private ObservableCollection<DownloadItem> downloadsList = new ObservableCollection<DownloadItem>();
        public DownloadOptions Options { get; set; } = new DownloadOptions();


        public DownloadManager() {
        }

        public ObservableCollection<DownloadItem> DownloadsList {
            get { return downloadsList; }
        }

        public event EventHandler DownloadAdded;

        /// <summary>
        /// Downloads specified video.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="upgradeAudio">If true, only the audio will be downloaded and it will be merged with the local video file.</param>
        /// <param name="callback">The method to call once download is completed.</param>
        public async Task DownloadVideoAsync(string url, string destination, string description, DownloadAction action, EventHandler<DownloadCompletedEventArgs> callback, DownloadOptions options, object data) {
            if (IsDownloadDuplicate(url))
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(destination));

            // Add DownloadItem right away before doing any async work.
            string DestinationNoExt = Path.Combine(Path.GetDirectoryName(destination), Path.GetFileNameWithoutExtension(destination));
            DownloadItem DownloadInfo = new DownloadItem(url, destination, DestinationNoExt, description, action, callback, options, data);
            Application.Current.Dispatcher.Invoke(() => {
                downloadsList.Insert(0, DownloadInfo);
                // Notify UI of new download to show window.
                DownloadAdded?.Invoke(this, new EventArgs());
            });

            if (downloadsList.Where(d => d.Status == DownloadStatus.Downloading || d.Status == DownloadStatus.Initializing).Count() < Options.SimultaneousDownloads)
                await StartDownloadAsync(DownloadInfo).ConfigureAwait(false);
        }

        private async Task StartDownloadAsync(DownloadItem downloadInfo) {
            downloadInfo.Status = DownloadStatus.Initializing;

            // Query the download URL for the right file.
            string VideoId = null;
            VideoInfo VInfo = null;
            if (YoutubeClient.TryParseVideoId(downloadInfo.Url, out VideoId)) {
                try {
                    VInfo = await youtube.GetVideoInfoAsync(VideoId);
                }
                catch {
                }
            }

            if (VInfo == null) {
                downloadInfo.Status = DownloadStatus.Failed;
                RaiseCallback(downloadInfo);
                return;
            }

            // Get the highest resolution format.
            BestFormatInfo BestFile = DownloadManager.SelectBestFormat(VInfo, downloadInfo.Options ?? Options);

            if (BestFile.BestVideo != null && (downloadInfo.Action == DownloadAction.Download || downloadInfo.Action == DownloadAction.DownloadVideo)) {
                downloadInfo.Files.Add(new FileProgress() {
                    Type = StreamType.Video,
                    Url = BestFile.BestVideo.Url,
                    Destination = downloadInfo.DestinationNoExt + GetVideoExtension(DownloadManager.GetVideoEncoding(BestFile.BestVideo)),
                    Stream = BestFile.BestVideo,
                    BytesTotal = BestFile.BestVideo.ContentLength
                });
            }

            if (BestFile.BestAudio != null && (downloadInfo.Action == DownloadAction.Download || downloadInfo.Action == DownloadAction.DownloadAudio)) {
                downloadInfo.Files.Add(new FileProgress() {
                    Type = StreamType.Audio,
                    Url = BestFile.BestAudio.Url,
                    Destination = downloadInfo.DestinationNoExt + GetAudioExtension(BestFile.BestAudio.AudioEncoding),
                    Stream = BestFile.BestAudio,
                    BytesTotal = BestFile.BestAudio.ContentLength
                });
            }

            // Add extension if Destination doesn't already include it.
            string Ext = DownloadManager.GetFinalExtension(BestFile.BestVideo, BestFile.BestAudio);
            if (!downloadInfo.Destination.ToLower().EndsWith(Ext))
                downloadInfo.Destination += Ext;

            await DownloadFilesAsync(downloadInfo).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the file extension for specified audio type.
        /// </summary>
        /// <param name="audio">The audio type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        public string GetVideoExtension(VideoEncoding video) {
            return "." + video.ToString().ToLower();
        }

        /// <summary>
        /// Returns the file extension for specified audio type.
        /// </summary>
        /// <param name="audio">The audio type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        public string GetAudioExtension(AudioEncoding audio) {
            return "." + audio.ToString().ToLower();
        }

        /// <summary>
        /// Downloads the specified list of files.
        /// </summary>
        /// <param name="downloadInfo">The information about the files to download.</param>
        /// <param name="callback">The function to call when download is completed.</param>
        private async Task DownloadFilesAsync(DownloadItem downloadInfo) {
            if (!downloadInfo.IsCanceled) {
                // Download all files.
                List<Task> DownloadTasks = new List<Task>();
                foreach (var item in downloadInfo.Files) {
                    DownloadTasks.Add(DownloadVideoAsync(downloadInfo, item));
                }
                await Task.WhenAll(DownloadTasks.ToArray()).ConfigureAwait(false);
            } else
                RaiseCallback(downloadInfo);
        }

        private async Task DownloadVideoAsync(DownloadItem downloadInfo, FileProgress fileInfo) {
            downloadInfo.Status = DownloadStatus.Downloading;
            CancellationTokenSource CancelToken = new CancellationTokenSource();
            var progressHandler = new Progress<double>(p => {
                if (downloadInfo.IsCanceled)
                    CancelToken.Cancel();
                else {
                    fileInfo.BytesDownloaded = (long)(fileInfo.BytesTotal * p);
                    downloadInfo.UpdateProgress();
                }
            });
            try {
                await youtube.DownloadMediaStreamAsync((MediaStreamInfo)fileInfo.Stream, fileInfo.Destination, progressHandler, CancelToken.Token).ConfigureAwait(false);
            }
            catch {
                downloadInfo.Status = DownloadStatus.Failed;
            }

            // Detect whether this is the last file.
            fileInfo.Done = true;
            if (downloadInfo.Files.Any(d => !d.Done) == false) {
                var NextDownload = StartNextDownloadAsync().ConfigureAwait(false);

                // Raise events for the last file part only.
                if (downloadInfo.IsCompleted) {
                    try {
                        await DownloadCompletedAsync(downloadInfo).ConfigureAwait(false);
                    }
                    catch {
                        downloadInfo.Status = DownloadStatus.Failed;
                    }
                } else if (downloadInfo.IsCanceled)
                    DownloadCanceled(downloadInfo);
                RaiseCallback(downloadInfo);

                await NextDownload;
            }
        }

        private async Task DownloadCompletedAsync(DownloadItem downloadInfo) {
            await Task.Run(() => OnMuxing(this, new DownloadCompletedEventArgs(downloadInfo))).ConfigureAwait(false);

            CompletionStatus Result = File.Exists(downloadInfo.Destination) ? CompletionStatus.Success : CompletionStatus.Error;
            var VideoFile = downloadInfo.Files.FirstOrDefault(f => f.Type == StreamType.Video);
            var AudioFile = downloadInfo.Files.FirstOrDefault(f => f.Type == StreamType.Audio);

            // Muxe regularly unless muxing in derived class.
            if (Result != CompletionStatus.Success)
                Result = await Task.Run(() => MediaMuxer.Muxe(VideoFile?.Destination, AudioFile?.Destination, downloadInfo.Destination)).ConfigureAwait(false);
            if (Result == CompletionStatus.Success)
                Result = File.Exists(downloadInfo.Destination) ? CompletionStatus.Success : CompletionStatus.Error;

            if (VideoFile != null)
                File.Delete(VideoFile.Destination);
            if (AudioFile != null)
                File.Delete(AudioFile.Destination);
            downloadInfo.Status = Result == CompletionStatus.Success ? DownloadStatus.Done : DownloadStatus.Failed;
            if (downloadInfo.Status != DownloadStatus.Failed)
                await Task.Run(() => OnCompleted(this, new DownloadCompletedEventArgs(downloadInfo))).ConfigureAwait(false);

            if (downloadInfo.Status != DownloadStatus.Done)
                downloadInfo.Status = DownloadStatus.Failed;
        }

        protected virtual void OnMuxing(object sender, DownloadCompletedEventArgs e) {
        }

        protected virtual void OnCompleted(object sender, DownloadCompletedEventArgs e) {
            e.DownloadInfo.Callback?.Invoke(sender, e);
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

        public bool IsDownloadDuplicate(string url) {
            bool Result = (from d in this.downloadsList
                           where (d.Status == DownloadStatus.Downloading || d.Status == DownloadStatus.Initializing || d.Status == DownloadStatus.Waiting) &&
                             string.Compare(d.Url, url, true) == 0
                           select d).Any();
            return Result;
        }

        private void RaiseCallback(DownloadItem downloadInfo) {
            if (downloadInfo.Callback != null)
                Application.Current.Dispatcher.Invoke(() => downloadInfo.Callback(this, new DownloadCompletedEventArgs(downloadInfo)));
        }


        public static string[] DownloadedExtensions = new string[] { ".mp4", ".webm", ".mkv", ".flv" };

        public static async Task<VideoInfo> GetDownloadInfoAsync(string url) {
            VideoInfo Result = null;
            try {
                YoutubeClient youtube = new YoutubeClient();
                string VideoId = null;
                if (YoutubeClient.TryParseVideoId(url, out VideoId))
                    Result = await youtube.GetVideoInfoAsync(VideoId);
            }
            catch {
            }
            return Result;
        }

        public static async Task<string> GetVideoTitle(string url) {
            try {
                VideoInfo VInfo = await DownloadManager.GetDownloadInfoAsync(url);
                if (VInfo != null)
                    return VInfo.Title;
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Returns the best format from the list in this order of availability: WebM, Mp4 or Flash.
        /// Mp4 will be chosen if WebM is over 35% smaller.
        /// </summary>
        /// <param name="list">The list of videos to chose from.</param>
        /// <returns>The best format available.</returns>
        public static BestFormatInfo SelectBestFormat(VideoInfo vinfo, DownloadOptions options) {
            var MaxResolutionList = (from v in vinfo.VideoStreams.Cast<MediaStreamInfo>().Union(vinfo.MixedStreams)
                                     where (options.MaxQuality == 0 || GetVideoHeight(v) <= options.MaxQuality)
                                     orderby GetVideoHeight(v) descending
                                     orderby GetVideoFrameRate(v) descending
                                     select v).ToList();

            MaxResolutionList = MaxResolutionList.Where(v => GetVideoHeight(v) == GetVideoHeight(MaxResolutionList.First())).ToList();
            //double Framerate = GetVideoFrameRate(MaxResolutionList.FirstOrDefault());
            //if (Framerate > 0)
            //    MaxResolutionList = MaxResolutionList.Where(v => GetVideoFrameRate(v) == Framerate).ToList();

            MediaStreamInfo BestVideo = (from v in MaxResolutionList
                                             // WebM VP9 encodes ~35% better. non-DASH is VP8 and isn't better than MP4.
                                         let Preference = (int)((GetVideoEncoding(v) == VideoEncoding.Vp9) ? v.ContentLength * 1.35 : v.ContentLength)
                                         where options.PreferredFormat == SelectStreamFormat.Best ||
                                            (options.PreferredFormat == SelectStreamFormat.MP4 && GetVideoEncoding(v) == VideoEncoding.H264) ||
                                            (options.PreferredFormat == SelectStreamFormat.VP9 && GetVideoEncoding(v) == VideoEncoding.Vp9)
                                         orderby Preference descending
                                         select v).FirstOrDefault();
            if (BestVideo == null)
                BestVideo = MaxResolutionList.FirstOrDefault();
            if (BestVideo != null) {
                BestFormatInfo Result = new BestFormatInfo {
                    BestVideo = BestVideo,
                    BestAudio = SelectBestAudio(vinfo, options),
                    Duration = vinfo.Duration
                };
                return Result;
            } else
                return null;
        }

        public static VideoEncoding GetVideoEncoding(MediaStreamInfo stream) {
            VideoStreamInfo VInfo = stream as VideoStreamInfo;
            MixedStreamInfo MInfo = stream as MixedStreamInfo;
            if (VInfo == null && MInfo == null)
                return VideoEncoding.H264;
            return VInfo?.VideoEncoding ?? MInfo.VideoEncoding;
        }

        public static int GetVideoHeight(MediaStreamInfo stream) {
            VideoStreamInfo VInfo = stream as VideoStreamInfo;
            MixedStreamInfo MInfo = stream as MixedStreamInfo;
            if (VInfo == null && MInfo == null)
                return 0;
            VideoQuality Q = VInfo?.VideoQuality ?? MInfo.VideoQuality;
            if (Q == VideoQuality.High4320)
                return 4320;
            else if (Q == VideoQuality.High3072)
                return 3072;
            else if (Q == VideoQuality.High2160)
                return 2160;
            else if (Q == VideoQuality.High1440)
                return 1440;
            else if (Q == VideoQuality.High1080)
                return 1080;
            else if (Q == VideoQuality.High720)
                return 720;
            else if (Q == VideoQuality.Medium480)
                return 480;
            else if (Q == VideoQuality.Medium360)
                return 360;
            else if (Q == VideoQuality.Low240)
                return 240;
            else if (Q == VideoQuality.Low144)
                return 144;
            else
                return 0;
        }

        public static double GetVideoFrameRate(MediaStreamInfo stream) {
            VideoStreamInfo VInfo = stream as VideoStreamInfo;
            if (VInfo != null)
                return VInfo.VideoFramerate;
            else
                return 0;
        }

        /// <summary>
        /// Selects Opus audio if available, otherwise Vorbis or AAC.
        /// </summary>
        /// <param name="list">The list of available audios.</param>
        /// <returns>The audio to download.</returns>
        public static AudioStreamInfo SelectBestAudio(VideoInfo vinfo, DownloadOptions options) {
            if (vinfo == null || vinfo.AudioStreams.Count() == 0)
                return null;

            var BestAudio = (from v in vinfo.AudioStreams
                             // Opus encodes ~20% better, Vorbis ~10% better than AAC
                             let Preference = (int)(v.ContentLength * (v.AudioEncoding == AudioEncoding.Opus ?  1.2 : v.AudioEncoding == AudioEncoding.Vorbis ? 1.1 : 1))
                             where options.PreferredAudio == SelectStreamFormat.Best ||
                             (options.PreferredAudio == SelectStreamFormat.MP4 && (v.AudioEncoding == AudioEncoding.Aac || v.AudioEncoding == AudioEncoding.Mp3)) ||
                             (options.PreferredAudio == SelectStreamFormat.VP9 && (v.AudioEncoding == AudioEncoding.Opus || v.AudioEncoding == AudioEncoding.Vorbis))
                             orderby Preference descending
                             select v).FirstOrDefault();
            return BestAudio;
        }

        /// <summary>
        /// Returns the file extension for specified video codec type.
        /// To avoid conflicting file names, the codec extension must be different than the final extension.
        /// </summary>
        /// <param name="video">The video type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        public static string GetCodecExtension(Container video) {
            if (video == Container.WebM)
                return ".vp9";
            else if (video == Container.Mp4)
                return ".h264";
            else if (video == Container.Flv)
                return ".flv";
            else if (video == Container.Tgpp)
                return ".mp4v";
            else
                return ".avi";
        }

        /// <summary>
        /// Returns the file extension for specified video type.
        /// To avoid conflicting file names, the codec extension must be different than the final extension.
        /// </summary>
        /// <param name="video">The video type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        public static string GetFinalExtension(MediaStreamInfo video, AudioStreamInfo audio) {
            if ((video == null || video.Container == Container.WebM) && (audio == null || audio.Container == Container.WebM))
                return ".webm";
            else if ((video == null || video.Container == Container.Mp4) && (audio == null || audio.Container == Container.Mp4 || audio.Container == Container.M4A))
                return ".mp4";
            else if (video != null && (video.Container == Container.Mp4 || video.Container == Container.WebM))
                return ".mkv";
            else if (video != null)
                return GetCodecExtension(video.Container);
            else if (audio != null)
                return GetCodecExtension(audio.Container);
            else
                return "";
        }
    }

    public class BestFormatInfo {
        public MediaStreamInfo BestVideo { get; set; }
        public AudioStreamInfo BestAudio { get; set; }
        public TimeSpan Duration { get; set; }
        public string StatusText { get; set; }
    }
}
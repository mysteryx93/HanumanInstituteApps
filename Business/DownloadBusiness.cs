using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DataAccess;
using EmergenceGuardian.FFmpeg;
using EmergenceGuardian.Downloader;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using System.ComponentModel;

namespace Business {
    public class DownloadBusiness : DownloadManager {
        public DownloadBusiness() {
        }

        public DownloadBusiness(DownloadOptions options) {
            Options = options;
        }

        /// <summary>
        /// Downloads specified video.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="queuePos">The position in the queue to auto-play, or -1.</param>
        /// <param name="upgradeAudio">If true, only the audio will be downloaded and it will be merged with the local video file.</param>
        /// <param name="callback">The method to call once download is completed.</param>
        public async Task DownloadVideoAsync(Media video, int queuePos, EventHandler<DownloadCompletedEventArgs> callback) {
            await DownloadVideoAsync(video, queuePos, callback, DownloadAction.Download);
        }

        /// <summary>
        /// Downloads specified video.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="queuePos">The position in the queue to auto-play, or -1.</param>
        /// <param name="upgradeAudio">If true, only the audio will be downloaded and it will be merged with the local video file.</param>
        /// <param name="callback">The method to call once download is completed.</param>
        public async Task DownloadVideoAsync(Media video, int queuePos, EventHandler<DownloadCompletedEventArgs> callback, DownloadAction action) {
            if (video == null || string.IsNullOrEmpty(video.DownloadUrl))
                throw new ArgumentException("Video object is null or doesn't contain a valid YouTube URL.");

            // Store in the Temp folder.
            DefaultMediaPath PathCalc = new DefaultMediaPath(Settings.TempFilesPath.Substring(Settings.NaturalGroundingFolder.Length));
            string Destination = Settings.NaturalGroundingFolder + PathCalc.GetDefaultFileName(video.Artist, video.Title, null, (MediaType)video.MediaTypeId);
            string DownloadDesc = Path.GetFileName(Destination);
            Directory.CreateDirectory(Settings.TempFilesPath);

            await DownloadVideoAsync(video.DownloadUrl, Destination, DownloadDesc, action, callback, Options, new DownloadItemData(video, queuePos)).ConfigureAwait(false);
        }

        protected override void OnMuxing(object sender, DownloadCompletedEventArgs e) {
            // Separate file extension.
            DownloadItemData IData = e.DownloadInfo.Data as DownloadItemData;

            FileProgress VideoFile = e.DownloadInfo.Files.FirstOrDefault(f => f.Type == StreamType.Video);
            FileProgress AudioFile = e.DownloadInfo.Files.FirstOrDefault(f => f.Type == StreamType.Audio);
            string SrcFile = IData.Media.FileName != null ? Settings.NaturalGroundingFolder + IData.Media.FileName : null;
            CompletionStatus Result = CompletionStatus.Success;

            if (IData.Media.FileName != null && File.Exists(SrcFile) && (VideoFile == null || AudioFile == null)) {
                // Upgrade audio or video
                FFmpegProcess MInfo = MediaInfo.GetFileInfo(SrcFile);
                string VideoFormat = VideoFile != null ? Path.GetExtension(VideoFile.Destination).TrimStart('.') : MInfo.VideoStream?.Format;
                string AudioFormat = AudioFile != null ? Path.GetExtension(AudioFile.Destination).TrimStart('.') : MInfo.AudioStream?.Format;
                string VideoDestExt = GetFinalExtension(VideoFormat, AudioFormat);
                e.DownloadInfo.Destination = e.DownloadInfo.DestinationNoExt + VideoDestExt;
                Result = MediaMuxer.Muxe(VideoFile?.Destination ?? SrcFile, AudioFile?.Destination ?? SrcFile, e.DownloadInfo.Destination);
            }
            if (Result == CompletionStatus.Success && File.Exists(SrcFile))
                FileOperationAPIWrapper.MoveToRecycleBin(SrcFile);

            e.DownloadInfo.Status = Result == CompletionStatus.Success ? DownloadStatus.Done : DownloadStatus.Failed;
        }

        protected override void OnCompleted(object sender, DownloadCompletedEventArgs e) {
            DownloadItemData IData = e.DownloadInfo.Data as DownloadItemData;
            Media Video = IData.Media;
            string Destination = e.DownloadInfo.Destination;
            string DestinationExt = Path.GetExtension(Destination);
            Destination = Destination.Substring(0, Destination.Length - Path.GetExtension(Destination).Length);

            // Ensure download and merge succeeded.
            if (!FileHasContent(e.DownloadInfo.Destination)) {
                e.DownloadInfo.Status = DownloadStatus.Failed;
                return;
            }

            // Get final file name.
            DefaultMediaPath PathCalc = new DefaultMediaPath();
            string NewFileName = PathCalc.GetDefaultFileName(Video.Artist, Video.Title, Video.MediaCategoryId, (MediaType)Video.MediaTypeId);
            Directory.CreateDirectory(Path.GetDirectoryName(Settings.NaturalGroundingFolder + NewFileName));
            Video.FileName = NewFileName + DestinationExt;

            // Move file and overwrite.
            string DstFile = Settings.NaturalGroundingFolder + Video.FileName;
            if (File.Exists(DstFile))
                FileOperationAPIWrapper.MoveToRecycleBin(DstFile);
            File.Move(Destination + DestinationExt, DstFile);

            // Add to database
            EditVideoBusiness Business = new EditVideoBusiness();
            Media ExistingData = Business.GetVideoById(Video.MediaId);
            if (ExistingData != null) {
                // Edit video info.
                ExistingData.FileName = Video.FileName;
                ExistingData.Length = null;
                ExistingData.Height = null;
                Business.Save();
            } else {
                // Add new video info.
                Business.AddVideo(Video);
                Business.Save();
            }

            base.OnCompleted(sender, e);
        }

        /// <summary>
        /// Returns whether specified file exists and contains data (at least 500KB).
        /// </summary>
        /// <param name="fileName">The path of the file to check.</param>
        /// <returns>Whether the file contains data.</returns>
        private bool FileHasContent(string fileName) {
            return File.Exists(fileName) && new FileInfo(fileName).Length > 524288;
        }

        public static BestFormatInfo SelectBestFormat(VideoInfo vinfo) {
            return DownloadManager.SelectBestFormat(vinfo, Settings.SavedFile.Download);
        }


        /// <summary>
        /// Returns the file extension for specified video type.
        /// To avoid conflicting file names, the codec extension must be different than the final extension.
        /// </summary>
        /// <param name="video">The video type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        public static string GetFinalExtension(string video, string audio) {
            video = video.ToLower();
            audio = audio.ToLower();
            if (video == "vp8" || video == "vp9")
                video = "webm";
            if (video == "h264")
                video = "mp4";
            if (audio == "opus" || audio == "vorbis")
                audio = "webm";
            if (audio == "aac")
                audio = "mp4";

            if (video == "webm" && audio == "webm")
                return ".webm";
            else if (video == "mp4" && audio == "mp4")
                return ".mp4";
            else
                return ".mkv";
        }
    }

    public class DownloadItemData {
        public DownloadItemData() {
        }

        public DownloadItemData(Media media, int queuePos) {
            this.Media = media;
            this.QueuePos = queuePos;
        }

        public Media Media { get; set; }
        /// <summary>
        /// Indicate the position in the playlist for autoplay, or -1 to disable playback after complete.
        /// </summary>
        public int QueuePos { get; set; }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using EmergenceGuardian.CommonServices;
using EmergenceGuardian.Encoder;
using EmergenceGuardian.DownloadManager;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;
using YoutubeExplode.Videos.Streams;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    #region Interface

    public interface IDownloadBusiness {
        /// <summary>
        /// Returns the information of an active download with specified mediaId, or null.
        /// </summary>
        /// <param name="mediaId">The ID of the media to look for.</param>
        /// <returns>The download information of the requested media.</returns>
        DownloadMediaInfo GetActiveDownloadByMediaId(Guid mediaId);
        /// <summary>
        /// Downloads specified video.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="queuePos">The position in the queue to auto-play, or -1.</param>
        /// <param name="upgradeAudio">If true, only the audio will be downloaded and it will be merged with the local video file.</param>
        /// <param name="callback">The method to call once download is completed.</param>
        Task DownloadVideoAsync(Media video, int queuePos, DownloadTaskEventHandler callback);
        /// <summary>
        /// Downloads specified video.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="queuePos">The position in the queue to auto-play, or -1.</param>
        /// <param name="upgradeAudio">If true, only the audio will be downloaded and it will be merged with the local video file.</param>
        /// <param name="callback">The method to call once download is completed.</param>
        Task DownloadVideoAsync(Media video, int queuePos, DownloadTaskEventHandler callback, bool downloadVideo, bool downloadAudio, DownloadOptions options);
        /// <summary>
        /// Returns the best stream formats for specified stream infoset.
        /// </summary>
        /// <param name="vinfo">An infoset returned by YoutubeExplode library.</param>
        /// <returns>Information about the best streams.</returns>
        BestFormatInfo SelectBestFormat(StreamManifest vinfo);
        /// <summary>
        /// Returns the file extension for specified video type.
        /// To avoid conflicting file names, the codec extension must be different than the final extension.
        /// </summary>
        /// <param name="video">The video type to get file extension for.</param>
        /// <param name="audio">The audio type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        string GetFinalExtension(string video, string audio);
    }

    #endregion

    /// <summary>
    /// Manages extra aspects of media file downloads specific to this application such as file path management and database entries.
    /// </summary>
    public class DownloadBusiness : IDownloadBusiness {

        #region Declarations / Constructors

        private IDownloadManager download;
        protected readonly ISettings settings;
        protected readonly IFileSystemService fileSystem;
        protected readonly IAppPathService appPath;
        private IDefaultMediaPath defaultPath;
        private IDownloadMediaFactory downloadFactory;
        private IYouTubeStreamSelector streamSelector;
        private IMediaInfoReader mediaInfo;
        private IMediaMuxer mediaMuxer;
        private IMediaAccess mediaAccess;

        public DownloadBusiness(IDownloadManager downloadManager, ISettings settings, IFileSystemService fileSystemService, IAppPathService appPathService, IDefaultMediaPath defaultMediaPath, IDownloadMediaFactory downloadFactory, IYouTubeStreamSelector streamSelector, IMediaInfoReader mediaInfo, IMediaMuxer mediaMuxer, IMediaAccess mediaAccess) {
            this.download = downloadManager ?? throw new ArgumentNullException(nameof(downloadManager));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            this.appPath = appPathService ?? throw new ArgumentNullException(nameof(appPathService));
            this.defaultPath = defaultMediaPath ?? throw new ArgumentNullException(nameof(defaultMediaPath));
            this.downloadFactory = downloadFactory ?? throw new ArgumentNullException(nameof(downloadFactory));
            this.streamSelector = streamSelector ?? throw new ArgumentNullException(nameof(streamSelector));
            this.mediaInfo = mediaInfo ?? throw new ArgumentNullException(nameof(mediaInfo));
            this.mediaMuxer = mediaMuxer ?? throw new ArgumentNullException(nameof(mediaMuxer));
            this.mediaAccess = mediaAccess ?? throw new ArgumentNullException(nameof(mediaAccess));
            download.BeforeMuxing += Download_BeforeMuxing;
            download.Completed += Download_Completed;
        }

        #endregion

        /// <summary>
        /// Returns the information of an active download with specified mediaId, or null.
        /// </summary>
        /// <param name="mediaId">The ID of the media to look for.</param>
        /// <returns>The download information of the requested media.</returns>
        public DownloadMediaInfo GetActiveDownloadByMediaId(Guid mediaId) {
            return download.DownloadsList.FirstOrDefault(d => !d.IsCompleted && (d as DownloadMediaInfo).Media.MediaId == mediaId) as DownloadMediaInfo;
        }

        /// <summary>
        /// Downloads specified video.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="queuePos">The position in the queue to auto-play, or -1.</param>
        /// <param name="upgradeAudio">If true, only the audio will be downloaded and it will be merged with the local video file.</param>
        /// <param name="callback">The method to call once download is completed.</param>
        public async Task DownloadVideoAsync(Media video, int queuePos, DownloadTaskEventHandler callback) {
            await DownloadVideoAsync(video, queuePos, callback, true, true, null);
        }

        /// <summary>
        /// Downloads specified video.
        /// </summary>
        /// <param name="video">The video to download.</param>
        /// <param name="queuePos">The position in the queue to auto-play, or -1.</param>
        /// <param name="upgradeAudio">If true, only the audio will be downloaded and it will be merged with the local video file.</param>
        /// <param name="callback">The method to call once download is completed.</param>
        public async Task DownloadVideoAsync(Media video, int queuePos, DownloadTaskEventHandler callback, bool downloadVideo, bool downloadAudio, DownloadOptions options) {
            if (video == null || string.IsNullOrEmpty(video.DownloadUrl))
                throw new ArgumentException("Video object is null or doesn't contain a valid YouTube URL.");

            // Store in the Temp folder.
            string Destination = appPath.DownloaderTempPath + defaultPath.GetDefaultFileName(video.Artist, video.Title, null, (MediaType)video.MediaTypeId, "");
            string DownloadDesc = fileSystem.Path.GetFileName(Destination);
            fileSystem.Directory.CreateDirectory(appPath.DownloaderTempPath);

            DownloadMediaInfo DownloadTask = downloadFactory.Create(video, queuePos, video.DownloadUrl, Destination, DownloadDesc, downloadVideo, downloadAudio, callback, options);
            await download.AddDownloadAsync(DownloadTask).ConfigureAwait(false);
        }

        /// <summary>
        /// Allows upgrading audio or video of existing local file when only one of the two streams has been downloaded.
        /// </summary>
        /// <param name="e">Contains information about the download task.</param>
        private void Download_BeforeMuxing(object sender, DownloadTaskEventArgs e) {
            // Check if we need to upgrade an existing local media file.
            DownloadMediaInfo DownloadTask = e.DownloadTask as DownloadMediaInfo;
            string SrcFile = DownloadTask.Media.FileName != null ? settings.NaturalGroundingFolder + DownloadTask.Media.FileName : null;
            DownloadFileInfo VideoFile = e.DownloadTask.Files.FirstOrDefault(f => f.Type == StreamType.Video);
            DownloadFileInfo AudioFile = e.DownloadTask.Files.FirstOrDefault(f => f.Type == StreamType.Audio);
            CompletionStatus Result = CompletionStatus.Success;
            if (DownloadTask.Media.FileName != null && fileSystem.File.Exists(SrcFile) && (VideoFile == null || AudioFile == null)) {
                // Upgrade audio or video.
                IFileInfoFFmpeg MInfo = mediaInfo.GetFileInfo(SrcFile);
                string VideoFormat = VideoFile != null ? fileSystem.Path.GetExtension(VideoFile.Destination).TrimStart('.') : MInfo.VideoStream?.Format;
                string AudioFormat = AudioFile != null ? fileSystem.Path.GetExtension(AudioFile.Destination).TrimStart('.') : MInfo.AudioStream?.Format;
                string VideoDestExt = GetFinalExtension(VideoFormat, AudioFormat);
                e.DownloadTask.Destination = download.GetPathNoExt(e.DownloadTask.Destination) + VideoDestExt;
                Result = mediaMuxer.Muxe(VideoFile?.Destination ?? SrcFile, AudioFile?.Destination ?? SrcFile, e.DownloadTask.Destination);
            }

            // Cleanup.
            if (Result == CompletionStatus.Success && fileSystem.File.Exists(SrcFile))
                fileSystem.MoveToRecycleBin(SrcFile);
            else
                e.DownloadTask.Status = DownloadStatus.Failed;
        }

        /// <summary>
        /// When download is completed, move it into a new location and add it into the database.
        /// </summary>
        /// <param name="e">Contains information about the download task.</param>
        private void Download_Completed(object sender, DownloadTaskEventArgs e) {
            DownloadMediaInfo DownloadTask = e.DownloadTask as DownloadMediaInfo;
            Media Video = DownloadTask.Media;

            string Src = e.DownloadTask.Destination;
            string SrcExt = fileSystem.Path.GetExtension(Src);
            Src = Src.Substring(0, Src.Length - fileSystem.Path.GetExtension(Src).Length);

            // Get final file name.
            DefaultMediaPath PathCalc = new DefaultMediaPath();
            string Dst = PathCalc.GetDefaultFileName(Video.Artist, Video.Title, Video.MediaCategoryId, (MediaType)Video.MediaTypeId);
            fileSystem.EnsureDirectoryExists(settings.NaturalGroundingFolder + Dst);
            Video.FileName = Dst + SrcExt;

            // Move file and overwrite.
            string DstFile = settings.NaturalGroundingFolder + Video.FileName;
            if (fileSystem.File.Exists(DstFile))
                fileSystem.MoveToRecycleBin(DstFile);
            fileSystem.File.Move(Src + SrcExt, DstFile);

            // Add to database
            Media ExistingData = mediaAccess.GetMediaById(Video.MediaId);
            if (ExistingData != null) {
                // Edit video info.
                ExistingData.FileName = Video.FileName;
                ExistingData.Length = null;
                ExistingData.Height = null;
                mediaAccess.Save();
            } else {
                // Add new video info.
                mediaAccess.AddMedia(Video);
                mediaAccess.Save();
            }
        }

        /// <summary>
        /// Returns the best stream formats for specified stream infoset.
        /// </summary>
        /// <param name="vinfo">An infoset returned by YoutubeExplode library.</param>
        /// <returns>Information about the best streams.</returns>
        public BestFormatInfo SelectBestFormat(StreamManifest vinfo) {
            return streamSelector.SelectBestFormat(vinfo, settings.Data.Download);
        }

        /// <summary>
        /// Returns the file extension for specified video type.
        /// To avoid conflicting file names, the codec extension must be different than the final extension.
        /// </summary>
        /// <param name="video">The video type to get file extension for.</param>
        /// <param name="audio">The audio type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        public string GetFinalExtension(string video, string audio) {
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
}

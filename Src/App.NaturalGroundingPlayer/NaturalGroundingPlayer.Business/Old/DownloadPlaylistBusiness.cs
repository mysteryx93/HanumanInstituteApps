using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HanumanInstitute.Encoder;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using HanumanInstitute.CommonServices;
using HanumanInstitute.DownloadManager;
using HanumanInstitute.NaturalGroundingPlayer.DataAccess;

namespace HanumanInstitute.NaturalGroundingPlayer.Business {

    #region Interface

    /// <summary>
    /// Scans the playlist for videos with higher resolution available, or for broken links, and downloads those videos.
    /// </summary>
    public interface IDownloadPlaylistBusiness {

    }

    #endregion

    /// <summary>
    /// Scans the playlist for videos with higher resolution available, or for broken links, and downloads those videos.
    /// </summary>
    public class DownloadPlaylistBusiness : IDownloadPlaylistBusiness {

        #region Declarations / Constructors

        private Dictionary<Guid, ScanResultItem> scanResults = new Dictionary<Guid, ScanResultItem>(10);
        private DownloadOptions downloadOptions = new DownloadOptions();

        private IDownloadBusiness download;
        private IDownloadManager downloadManager;
        private IYouTubeStreamSelector youtubeSelector;
        private IPlayerAccess playerAccess;
        protected readonly IFileSystemService fileSystem;
        private IMediaInfoReader mediaInfo;
        private IMediaMuxer mediaMuxer;
        protected readonly ISettings settings;
        protected readonly IAppPathService appPath;
        private IMediaAccess mediaAccess;

        public DownloadPlaylistBusiness(DownloadBusiness downloadBusiness, IDownloadManager downloadManager, IYouTubeStreamSelector youtubeSelector, IPlayerAccess playerAccess, IFileSystemService fileSystemService, IMediaInfoReader mediaInfo, IMediaMuxer mediaMuxer, ISettings settings, IAppPathService appPathService, IMediaAccess mediaAccess) {
            this.download = downloadBusiness ?? throw new ArgumentNullException(nameof(downloadBusiness));
            this.downloadManager = downloadManager ?? throw new ArgumentNullException(nameof(downloadManager));
            this.youtubeSelector = youtubeSelector ?? throw new ArgumentNullException(nameof(youtubeSelector));
            this.playerAccess = playerAccess ?? throw new ArgumentNullException(nameof(playerAccess));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            this.mediaInfo = mediaInfo ?? throw new ArgumentNullException(nameof(mediaInfo));
            this.mediaMuxer = mediaMuxer ?? throw new ArgumentNullException(nameof(mediaMuxer));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.appPath = appPathService ?? throw new ArgumentNullException(nameof(appPathService));
            this.mediaAccess = mediaAccess ?? throw new ArgumentNullException(nameof(mediaAccess));
        }

        #endregion

        /// <summary>
        /// Starts scanning the media list for available downloads.
        /// </summary>
        /// <param name="selection">A list of media items to scan.</param>
        /// <param name="cancel">A token allowing to cancel the task.</param>
        public async Task StartScan(List<MediaListItem> selection, CancellationToken cancel) {
            await selection.ForEachAsync(5, cancel, async item => {
                Media VideoData = playerAccess.GetVideoById(item.MediaId.Value);
                if (VideoData != null && !item.IsBusy) {
                    try {
                        // Query the server for media info.
                        SetStatus(item, VideoListItemStatusEnum.DownloadingInfo);
                        VideoInfo VInfo = await downloadManager.GetDownloadInfoAsync(VideoData.DownloadUrl);
                        if (VInfo != null) {
                            // Get the highest resolution format.
                            BestFormatInfo VideoFormat = download.SelectBestFormat(VInfo.Streams);
                            if (VideoFormat == null || VideoFormat.BestVideo == null)
                                SetStatus(item, VideoListItemStatusEnum.Failed);
                            else
                                SetStatus(item, await IsHigherQualityAvailable(VideoFormat, settings.NaturalGroundingFolder + VideoData.FileName));
                            if (VideoFormat != null && !string.IsNullOrEmpty(VideoFormat.StatusText))
                                SetStatus(item, item.Status, item.StatusText + string.Format(" ({0})", VideoFormat.StatusText));
                        } else
                            SetStatus(item, VideoListItemStatusEnum.InvalidUrl);
                    }
                    catch {
                        SetStatus(item, VideoListItemStatusEnum.Failed);
                    }
                }
            });
        }

        /// <summary>
        /// Restores the Status and StatusText fields from the cache into specified data list.
        /// </summary>
        /// <param name="list">The list of data to restore cached data into.</param>
        public void LoadStatusFromCache(List<MediaListItem> list) {
            ScanResultItem Cache;
            foreach (MediaListItem item in list) {
                if (scanResults.ContainsKey(item.MediaId.Value)) {
                    Cache = scanResults[item.MediaId.Value];
                    item.Status = Cache.Status;
                    item.StatusText = Cache.StatusText;
                }
            }
        }

        /// <summary>
        /// Returns the InfoReader from the last call to IsHigherQualityAvailable.
        /// Note: this is not thread-safe! Only works for 1 call at a time (per class instance).
        /// </summary>
        public IFileInfoFFmpeg LastFileInfo { get; set; }

        /// <summary>
        /// Returns whether the local file should be replaced by the YouTube version.
        /// </summary>
        /// <param name="serverFile">The information of the available server file.</param>
        /// <param name="localFile">A path to the local file.</param>
        /// <returns>True if the local file should be replaced.</returns>
        public async Task<VideoListItemStatusEnum> IsHigherQualityAvailable(BestFormatInfo serverFile, string localFile) {
            // If there is no local file, it should be downloaded.
            if (!fileSystem.File.Exists(localFile))
                return VideoListItemStatusEnum.HigherQualityAvailable;

            // Read local file info.
            string LocalFileExt = fileSystem.Path.GetExtension(localFile).ToLower();
            IFileInfoFFmpeg fileInfo = await Task.Run(() => mediaInfo.GetFileInfo(localFile));
            LastFileInfo = fileInfo;

            VideoListItemStatusEnum Result = IsHigherQualityAvailableInternal(serverFile, localFile, fileInfo);

            if (Result == VideoListItemStatusEnum.OK) {
                // Check if video has right container.
                string VFormat = fileInfo.VideoStream?.Format;
                string AFormat = fileInfo.AudioStream?.Format;
                VFormat = VFormat?.ToLower();
                AFormat = AFormat?.ToLower();
                if (VFormat == "h264" && (AFormat == null || AFormat == "aac") && LocalFileExt != ".mp4")
                    return VideoListItemStatusEnum.WrongContainer;
                else if (VFormat == "webm" && (AFormat == null || AFormat == "opus" || AFormat == "vorbis") && LocalFileExt != ".webm")
                    return VideoListItemStatusEnum.WrongContainer;
                else
                    return VideoListItemStatusEnum.OK;
            } else
                return Result;
        }

        public VideoListItemStatusEnum IsHigherQualityAvailableInternal(BestFormatInfo serverFile, string localFile, IFileInfoFFmpeg fileInfo) {
            string LocalFileExt = fileSystem.Path.GetExtension(localFile).ToLower();
            // If local file is FLV and there's another format available, it should be downloaded.
            if (LocalFileExt == ".flv")
                return VideoListItemStatusEnum.HigherQualityAvailable;

            // Original VCD files and files of unrecognized extensions should not be replaced.
            if (!downloadManager.DownloadedExtensions.Contains(LocalFileExt) || fileInfo?.VideoStream?.Format == "mpeg1video") {
                serverFile.StatusText = "Not from YouTube";
                return VideoListItemStatusEnum.OK;
            }

            // For server file size, estimate 4% extra for audio. Estimate 30% advantage for VP9 format. non-DASH WebM is VP8 and doesn't have that bonus.
            long ServerFileSize = (long)(serverFile.BestVideo.Size.TotalBytes * 1.04);
            if (youtubeSelector.GetVideoEncoding(serverFile.BestVideo) == YouTubeVideoEncoding.Vp9)
                ServerFileSize = (long)(ServerFileSize * 1.3);
            long LocalFileSize = fileSystem.FileInfo.FromFileName(localFile).Length;
            if (fileInfo?.VideoStream?.Format == "vp9")
                LocalFileSize = (long)(LocalFileSize * 1.3);

            // If server resolution is better, download unless local file is bigger.
            int LocalFileHeight = fileInfo?.VideoStream?.Height ?? 0;
            if (youtubeSelector.GetVideoHeight(serverFile.BestVideo) > LocalFileHeight) {
                if (ServerFileSize > LocalFileSize)
                    return VideoListItemStatusEnum.HigherQualityAvailable;
                else if (ServerFileSize != 0)
                    return VideoListItemStatusEnum.OK;
            } else if (youtubeSelector.GetVideoHeight(serverFile.BestVideo) < LocalFileHeight) {
                // If local resolution is higher, keep.
                return VideoListItemStatusEnum.OK;
            }

            // Choose whether to download only audio, only video, or both.
            bool DownloadVideo = false;
            bool DownloadAudio = false;

            // Is estimated server file size is at least 15% larger than local file (for same resolution), download.
            if (ServerFileSize > LocalFileSize * 1.15)
                DownloadVideo = true;
            // If PreferredFormat is set to a format, download that format.
            else if (downloadOptions != null) {
                if (downloadOptions.PreferredFormat == SelectStreamFormat.MP4 && fileInfo.VideoStream?.Format == "vp9" && (youtubeSelector.GetVideoEncoding(serverFile.BestVideo) == YouTubeVideoEncoding.H264))
                    DownloadVideo = true;
                else if (downloadOptions.PreferredFormat == SelectStreamFormat.VP9 && fileInfo.VideoStream?.Format == "h264" && (youtubeSelector.GetVideoEncoding(serverFile.BestVideo) == YouTubeVideoEncoding.Vp9 || youtubeSelector.GetVideoEncoding(serverFile.BestVideo) == YouTubeVideoEncoding.Vp8))
                    DownloadVideo = true;
            }

            if (fileInfo.AudioStream == null)
                DownloadAudio = true;
            // Can only upgrade is video length is same.
            else if (Math.Abs((fileInfo.FileDuration - serverFile.Duration).TotalSeconds) < 1) {
                // download audio and merge with local video.
                string LocalAudio = fileInfo.AudioStream.Format;
                string RemoteAudio = serverFile.BestAudio?.AudioCodec ?? YouTubeAudioEncoding.Aac;
                if (fileInfo.AudioStream.Bitrate == 0 && LocalAudio == "opus" || LocalAudio == "vorbis")
                    fileInfo.AudioStream.Bitrate = 160; // FFmpeg doesn't return bitrate of Opus and Vorbis audios, but it's 160.
                if (fileInfo.AudioStream.Bitrate == 0)
                    fileInfo.AudioStream.Bitrate = GetAudioBitrateMuxe(localFile);
                int LocalAudioBitRate = fileInfo.AudioStream.Bitrate;
                long ServerAudioBitRate = serverFile.BestAudio != null ? (long)serverFile.BestAudio.Bitrate.KiloBitsPerSecond : 0;
                // MediaInfo returns no bitrate for MKV containers with AAC audio.
                fileInfo.AudioStream.Bitrate = 160;
                if (LocalAudioBitRate > 0 || LocalFileExt != ".mkv") {
                    if ((LocalAudioBitRate == 0 || LocalAudioBitRate < ServerAudioBitRate * .8))
                        DownloadAudio = true;
                }
                if ((LocalAudio == "opus" && RemoteAudio == YouTubeAudioEncoding.Vorbis) || (LocalAudio == "vorbis" && RemoteAudio == YouTubeAudioEncoding.Opus))
                    DownloadAudio = true;
            } else
                DownloadAudio = DownloadVideo;

            if (DownloadVideo && DownloadAudio)
                return VideoListItemStatusEnum.HigherQualityAvailable;
            else if (DownloadVideo)
                return VideoListItemStatusEnum.BetterVideoAvailable;
            else if (DownloadAudio)
                return VideoListItemStatusEnum.BetterAudioAvailable;

            return VideoListItemStatusEnum.OK;
        }

        public async Task StartDownload(List<MediaListItem> selection, CancellationToken cancel) {
            // Download videos and upgrades.
            selection = selection.Where(s => s.CanDownload || s.Status == VideoListItemStatusEnum.WrongContainer).ToList();
            foreach (var item in selection) {
                item.IsBusy = true;
            }
            await selection.ForEachAsync(2, cancel, async item => {
                if (item.Status == VideoListItemStatusEnum.WrongContainer)
                    await Task.Run(() => FixContainer(item));
                else
                    await DownloadFile(item, item.Status != VideoListItemStatusEnum.BetterAudioAvailable, item.Status != VideoListItemStatusEnum.BetterVideoAvailable);
                item.IsBusy = false;
            });
            foreach (var item in selection) {
                item.IsBusy = false;
            }
        }

        private async Task DownloadFile(MediaListItem item, bool downloadVideo, bool downloadAudio) {
            if (item != null && item.MediaId != null) {
                Media ItemData = playerAccess.GetVideoById(item.MediaId.Value);
                if (ItemData != null) {
                    SetStatus(item, VideoListItemStatusEnum.Downloading, null);
                    await download.DownloadVideoAsync(ItemData, -1,
                        (sender, e) => {
                            SetStatus(item, e.DownloadTask.IsCompleted ? VideoListItemStatusEnum.Done : VideoListItemStatusEnum.Failed);
                        }, downloadVideo, downloadAudio, downloadOptions);
                }
            }
        }

        private void FixContainer(MediaListItem item) {
            SetStatus(item, VideoListItemStatusEnum.Converting);
            string SrcFile = settings.NaturalGroundingFolder + item.FileName;
            if (item.FileName != null && fileSystem.File.Exists(SrcFile)) {
                IFileInfoFFmpeg FileInfo = mediaInfo.GetFileInfo(SrcFile);
                string Ext1 = fileSystem.Path.GetExtension(item.FileName).ToLower();
                string Ext2 = download.GetFinalExtension(FileInfo.VideoStream?.Format, FileInfo.AudioStream?.Format);
                if ((Ext2 == ".mp4" || Ext2 == ".webm") && Ext1 != Ext2) {
                    string DstFile = item.FileName.Substring(0, item.FileName.Length - Ext1.Length) + Ext2;
                    if (mediaMuxer.Muxe(SrcFile, SrcFile, settings.NaturalGroundingFolder + DstFile) == CompletionStatus.Success) {
                        fileSystem.MoveToRecycleBin(SrcFile);
                        // Change database binding.
                        Media ExistingData = mediaAccess.GetMediaById(item.MediaId.Value);
                        if (ExistingData != null) {
                            // Edit video info.
                            ExistingData.FileName = DstFile;
                            mediaAccess.Save();
                            SetStatus(item, VideoListItemStatusEnum.Done);
                            return;
                        }
                    }
                }
            }
            SetStatus(item, VideoListItemStatusEnum.Failed);
        }

        public void SetStatus(MediaListItem item, VideoListItemStatusEnum status) {
            SetStatus(item, status, null);
        }

        public void SetStatus(MediaListItem item, VideoListItemStatusEnum status, string statusText) {
            item.Status = status;
            if (statusText != null)
                item.StatusText = statusText;

            // Store results in cache.
            scanResults[item.MediaId.Value] = new ScanResultItem(item.Status, item.StatusText);
        }

        public int GetAudioBitrateMuxe(string file) {
            int Result = 0;
            string TmpFile = appPath.SystemTempPath + "GetBitrate - " + fileSystem.Path.GetFileNameWithoutExtension(file) + ".aac";
            if (mediaMuxer.Muxe(null, file, TmpFile) == CompletionStatus.Success) {
                IFileInfoFFmpeg fileInfo = mediaInfo.GetFileInfo(TmpFile);
                Result = fileInfo.AudioStream?.Bitrate ?? 0;
            }
            fileSystem.File.Delete(TmpFile);
            return Result;
        }
    }
}

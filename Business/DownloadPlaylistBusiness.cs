using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using YoutubeExtractor;
using System.IO;
using EmergenceGuardian.FFmpeg;

namespace Business {
    /// <summary>
    /// Scans the playlist for videos with higher resolution available, or for broken links, and downloads those videos.
    /// </summary>
    public class DownloadPlaylistBusiness {
        public DownloadBusiness DownloadManager;
        private Dictionary<Guid, ScanResultItem> scanResults = new Dictionary<Guid, ScanResultItem>(10);

        public DownloadPlaylistBusiness() {
        }

        public async Task StartScan(List<VideoListItem> selection, CancellationToken cancel) {
            await selection.ForEachAsync(5, cancel, async item => {
                Media VideoData = PlayerAccess.GetVideoById(item.MediaId.Value);

                if (VideoData != null) {
                    try {
                        // Query the server for media info.
                        SetStatus(item, VideoListItemStatusEnum.DownloadingInfo);
                        var VTask = DownloadBusiness.GetDownloadUrlsAsync(VideoData.DownloadUrl);
                        var VideoList = await VTask;
                        if (VideoList != null) {
                            // Get the highest resolution format.
                            BestFormatInfo VideoFormat = DownloadBusiness.SelectBestFormat(VideoList);
                            if (VideoFormat == null || VideoFormat.BestVideo == null)
                                SetStatus(item, VideoListItemStatusEnum.Failed);
                            else {
                                SetStatus(item, await IsHigherQualityAvailable(Settings.NaturalGroundingFolder + VideoData.FileName, VideoFormat));
                            }
                            if (VideoFormat != null && !string.IsNullOrEmpty(VideoFormat.StatusText))
                                SetStatus(item, item.Status, item.StatusText + string.Format(" ({0})", VideoFormat.StatusText));
                        } else
                            SetStatus(item, VideoListItemStatusEnum.InvalidUrl);
                    } catch {
                        SetStatus(item, VideoListItemStatusEnum.Failed);
                    }
                }
            });
        }

        /// <summary>
        /// Restores the Status and StatusText fields from the cache into specified data list.
        /// </summary>
        /// <param name="list">The list of data to restore cached data into.</param>
        public void LoadStatusFromCache(List<VideoListItem> list) {
            ScanResultItem Cache;
            foreach (VideoListItem item in list) {
                if (scanResults.ContainsKey(item.MediaId.Value)) {
                    Cache = scanResults[item.MediaId.Value];
                    item.Status = Cache.Status;
                    item.StatusText = Cache.StatusText;
                }
            }
        }

        /// <summary>
        /// Returns whether the local file should be replaced by the YouTube version.
        /// </summary>
        /// <param name="localFile">A path to the local file.</param>
        /// <param name="serverFile">The information of the available server file.</param>
        /// <returns>True if the local file should be replaced.</returns>
        public async Task<VideoListItemStatusEnum> IsHigherQualityAvailable(string localFile, BestFormatInfo serverFile) {
            // If there is no local file, it should be downloaded.
            if (!File.Exists(localFile))
                return VideoListItemStatusEnum.HigherQualityAvailable;

            // If local file is FLV and there's another format available, it should be downloaded.
            string LocalFileExt = Path.GetExtension(localFile).ToLower();
            if (LocalFileExt == ".flv" && serverFile.BestVideo.VideoType != VideoType.Flash)
                return VideoListItemStatusEnum.HigherQualityAvailable;

            // Original VCD files and files of unrecognized extensions should not be replaced.
            FFmpegProcess InfoReader = await Task.Run(() => MediaInfo.GetFileInfo(localFile));
            if (!DownloadBusiness.DownloadedExtensions.Contains(LocalFileExt) || InfoReader?.VideoStream?.Format == "mpeg1video") {
                serverFile.StatusText = "Not from YouTube";
                return VideoListItemStatusEnum.OK;
            }

            // For server file size, estimate 10% extra for audio. Estimate 35% advantage for VP9 format. non-DASH WebM is VP8 and doesn't have that bonus.
            long ServerFileSize = (long)(serverFile.BestVideo.FileSize * 1.1);
            if (serverFile.BestVideo.VideoType == VideoType.WebM && serverFile.BestVideo.AdaptiveType == AdaptiveType.Video)
                ServerFileSize = (long)(ServerFileSize * 1.35);
            long LocalFileSize = new FileInfo(localFile).Length;
            if (InfoReader?.VideoStream?.Format == "vp9")
                LocalFileSize = (long)(LocalFileSize * 1.35);

            // If server resolution is better, download unless local file is bigger.
            int LocalFileHeight = InfoReader?.VideoStream?.Height ?? 0;
            if (serverFile.BestVideo.Resolution > LocalFileHeight) {
                if (ServerFileSize > LocalFileSize)
                    return VideoListItemStatusEnum.HigherQualityAvailable;
                else if (ServerFileSize != 0) {
                    // non-DASH videos have no file size specified, and we won't replace local video with non-DASH video.
                    serverFile.StatusText = "Local file larger";
                    return VideoListItemStatusEnum.OK;
                }
            }

            // Is estimated server file size is at least 15% larger than local file (for same resolution), download.
            if (ServerFileSize > LocalFileSize * 1.15)
                return VideoListItemStatusEnum.HigherQualityAvailable;

            // download audio and merge with local video. (that didn't work, ffmpeg failed to merge back)
            int LocalAudioBitRate = InfoReader?.AudioStream?.Bitrate ?? 0;
            if (LocalAudioBitRate == 0 && InfoReader?.AudioStream.Format == "opus" || InfoReader?.AudioStream.Format == "vorbis")
                LocalAudioBitRate = 160; // FFmpeg doesn't return bitrate of Opus and Vorbis audios, but it's 160.
            int ServerAudioBitRate = serverFile.BestAudio != null ? serverFile.BestAudio.AudioBitrate : serverFile.BestVideo.AudioBitrate;
            // Fix a bug where MediaInfo returns no bitrate for MKV containers with AAC audio.
            if (LocalAudioBitRate > 0 || LocalFileExt != ".mkv") {
                if ((LocalAudioBitRate == 0 || LocalAudioBitRate < ServerAudioBitRate * .8) && serverFile.BestVideo.Resolution == LocalFileHeight) {
                    // Only redownload for audio if video file size is similar. Videos with AdaptiveType=None don't have file size.
                    if (ServerFileSize > LocalFileSize * .9 && serverFile.BestVideo.AdaptiveType == AdaptiveType.Video) {
                        serverFile.StatusText = "Audio";
                        return VideoListItemStatusEnum.HigherQualityAvailable;
                    } else {
                        serverFile.StatusText = "";
                        return VideoListItemStatusEnum.BetterAudioAvailable;
                    }
                }
            }

            return VideoListItemStatusEnum.OK;
        }

        public async Task StartDownload(List<VideoListItem> selection, bool upgradeAudio) {
            IEnumerable<VideoListItem> DownloadList;
            if (upgradeAudio)
                DownloadList = selection.Where(s => s.CanUpgradeAudio);
            else
                DownloadList = selection.Where(s => s.Status == VideoListItemStatusEnum.HigherQualityAvailable);
            List<Task> TaskList = new List<Task>();
            foreach (VideoListItem item in DownloadList) {
                TaskList.Add(DownloadFile(item, upgradeAudio));
            }
            await Task.WhenAll(TaskList);
        }

        private async Task DownloadFile(VideoListItem item, bool upgradeAudio) {
            if (DownloadManager != null && item != null && item.MediaId != null) {
                Media ItemData = PlayerAccess.GetVideoById(item.MediaId.Value);
                if (ItemData != null) {
                    SetStatus(item, VideoListItemStatusEnum.Downloading, null);
                    if (!upgradeAudio && ItemData.FileName != null && File.Exists(Settings.NaturalGroundingFolder + ItemData.FileName))
                        FileOperationAPIWrapper.MoveToRecycleBin(Settings.NaturalGroundingFolder + ItemData.FileName);
                    await DownloadManager.DownloadVideoAsync(ItemData, -1, upgradeAudio,
                        (sender, e) => {
                            SetStatus(item, e.DownloadInfo.IsCompleted ? VideoListItemStatusEnum.Done : VideoListItemStatusEnum.Failed);
                        });
                }
            }
        }

        public void SetStatus(VideoListItem item, VideoListItemStatusEnum status) {
            SetStatus(item, status, null);
        }

        public void SetStatus(VideoListItem item, VideoListItemStatusEnum status, string statusText) {
            item.Status = status;
            if (statusText != null)
                item.StatusText = statusText;

            // Store results in cache.
            scanResults[item.MediaId.Value] = new ScanResultItem(item.Status, item.StatusText);
        }
    }

    public class ScanResultItem {
        // public Guid MediaId { get; set; }
        public VideoListItemStatusEnum Status { get; set; }
        public string StatusText { get; set; }

        public ScanResultItem() {
        }

        public ScanResultItem(VideoListItemStatusEnum status, string statusText) {
            this.Status = status;
            this.StatusText = statusText;
        }
    }
}

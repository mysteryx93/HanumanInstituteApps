using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using YoutubeExtractor;
using System.IO;

namespace Business {
    /// <summary>
    /// Scans the playlist for videos with higher resolution available, or for broken links, and downloads those videos.
    /// </summary>
    public class DownloadPlaylistBusiness {
        public DownloadBusiness DownloadManager;
        private Dictionary<Guid, ScanResultItem> scanResults = new Dictionary<Guid, ScanResultItem>();

        public DownloadPlaylistBusiness() {
        }

        public async Task StartScan(List<VideoListItem> selection, CancellationToken cancel) {
            await selection.ForEachAsync(5, cancel, async item => {
                MediaInfoReader InfoReader = new MediaInfoReader();
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

            // Original VCD files shouldn't be replaced.
            MediaInfoReader InfoReader = new MediaInfoReader();
            await InfoReader.LoadInfoAsync(localFile);
            int LocalFileHeight = InfoReader.Height ?? 0;
            if (LocalFileHeight == 288) {
                serverFile.StatusText = "Original VCD";
                return VideoListItemStatusEnum.OK;
            }

            // For server file size, estimate 10% extra for audio. Estimate 35% advantage for WebM format.
            long ServerFileSize = (long)(serverFile.BestVideo.FileSize * 1.1);
            if (serverFile.BestVideo.VideoType == VideoType.WebM)
                ServerFileSize = (long)(ServerFileSize * 1.35);
            long LocalFileSize = new FileInfo(localFile).Length;
            if (LocalFileExt == ".webm")
                LocalFileSize = (long)(LocalFileSize * 1.35);

            // If server resolution is better, download unless local file is bigger.
            if (serverFile.BestVideo.Resolution > RoundResolutionUp(LocalFileHeight)) {
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
            if (serverFile.BestAudio != null) {
                int? LocalAudioBitRate = InfoReader.AudioBitRate;
                if (LocalAudioBitRate == null || LocalAudioBitRate < serverFile.BestAudio.AudioBitrate * .8) {
                    // Only redownload for audio if video file size is similar. Videos with AdaptiveType=None don't have file size.
                    if (ServerFileSize > LocalFileSize * .9 || serverFile.BestVideo.AdaptiveType == AdaptiveType.None) {
                        serverFile.StatusText = "Audio";
                        return VideoListItemStatusEnum.HigherQualityAvailable;
                    } else
                        serverFile.StatusText = "Better audio available";
                }
            }

            return VideoListItemStatusEnum.OK;
        }

        /// <summary>
        /// Round the resolution up to a YouTube standard, because sometimes YouTube offers 1440p but give a 1280p video.
        /// </summary>
        /// <param name="resolution">The resolution to round up to a standard number.</param>
        /// <returns>A standard YouTube resolution</returns>
        public int RoundResolutionUp(int resolution) {
            if (resolution > 1440)
                return 2160;
            else if (resolution > 1080)
                return 1440;
            else if (resolution > 720)
                return 1080;
            else if (resolution > 480)
                return 720;
            else if (resolution > 360)
                return 480;
            else if (resolution > 240)
                return 360;
            else
                return 240;
        }

        public async Task StartDownload(List<VideoListItem> selection) {
            var DownloadList = selection.Where(s => s.Status == VideoListItemStatusEnum.HigherQualityAvailable || s.Status == VideoListItemStatusEnum.BetterAudioAvailable);
            List<Task> TaskList = new List<Task>();
            foreach (VideoListItem item in DownloadList) {
                TaskList.Add(DownloadFile(item));
            }
            await Task.WhenAll(TaskList);
        }

        public async Task DownloadFile(VideoListItem item) {
            if (DownloadManager != null && item != null && item.MediaId != null) {
                Media ItemData = PlayerAccess.GetVideoById(item.MediaId.Value);
                if (ItemData != null) {
                    bool UpgradeAudio = item.Status == VideoListItemStatusEnum.BetterAudioAvailable;
                    SetStatus(item, VideoListItemStatusEnum.Downloading, null);
                    if (!UpgradeAudio && ItemData.FileName != null && File.Exists(Settings.NaturalGroundingFolder + ItemData.FileName))
                        FileOperationAPIWrapper.MoveToRecycleBin(Settings.NaturalGroundingFolder + ItemData.FileName);
                    await DownloadManager.DownloadVideoAsync(ItemData, -1, UpgradeAudio,
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

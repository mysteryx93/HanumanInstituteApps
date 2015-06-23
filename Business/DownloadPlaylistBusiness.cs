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
        private Dictionary<Guid, ScanResultItem> scanResults = new Dictionary<Guid,ScanResultItem>();

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
                            List<string> A = VideoList.Where(v => v.Resolution == 240 || v.Resolution == 360).Select(v => v.DownloadUrl).Distinct().ToList();
                            int OnlineResolution = (from v in VideoList
                                                    where (Settings.SavedFile.MaxDownloadQuality == 0 || v.Resolution <= Settings.SavedFile.MaxDownloadQuality)
                                                    orderby v.Resolution descending
                                                    select v.Resolution).FirstOrDefault();
                            // Select format in this order: WebM, Mp4, or Flash.
                            BestFormatInfo VideoFormat = await DownloadBusiness.SelectBestFormatWithStatus(VideoList.Where(v => v.Resolution == OnlineResolution));
                            if (VideoFormat == null || VideoFormat.BestFile == null)
                                SetStatus(item, VideoListItemStatusEnum.Failed);
                            else if (await IsHigherQualityAvailable(Settings.NaturalGroundingFolder + VideoData.FileName, VideoFormat)) {
                                // else if (LocalFileFormat == null || LocalFileHeight < OnlineResolution || (VideoFormat.BestFile.VideoType == VideoType.WebM && (LocalFileFormat == ".mp4" || LocalFileFormat == ".flv")) || ((VideoFormat.BestFile.VideoType == VideoType.WebM && LocalFileFormat == ".flv"))) {
                                SetStatus(item, VideoListItemStatusEnum.HigherQualityAvailable);
                            } else
                                SetStatus(item, VideoListItemStatusEnum.OK);
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
        public async Task<bool> IsHigherQualityAvailable(string localFile, BestFormatInfo serverFile) {
            // If there is no local file, it should be downloaded.
            if (!File.Exists(localFile))
                return true;

            // If local file is FLV and there's another format available, it should be downloaded.
            string LocalFileExt = Path.GetExtension(localFile).ToLower();
            if (LocalFileExt == ".flv" && serverFile.BestFile.VideoType != VideoType.Flash)
                return true;

            // MKV files processed with the Media Encoder shouldn't be replaced.
            if (LocalFileExt == ".mkv")
                return false;

            // Original VCD files shouldn't be replaced.
            MediaInfoReader InfoReader = new MediaInfoReader();
            await InfoReader.LoadInfoAsync(localFile);
            int LocalFileHeight = InfoReader.Height ?? 0;
            if (LocalFileHeight == 288)
                return false;

            // For server file size, estimate 10% extra for audio. Estimate 35% advantage for WebM format.
            double ServerFileSize = serverFile.Size * 1.1;
            if (serverFile.BestFile.VideoType == VideoType.WebM)
                ServerFileSize *= .35;
            double LocalFileSize = new FileInfo(localFile).Length;
            if (LocalFileExt == ".webm")
                LocalFileSize *= .35;

            // If server resolution is better, download unless local file is bigger.
            if (serverFile.BestFile.Resolution > LocalFileHeight && ServerFileSize > LocalFileSize)
                return true;

            // Is estimated server file size is at least 15% larger than local file (for same resolution), download.
            if (ServerFileSize > LocalFileSize * 1.15)
                return true;

            return false;
        }

        public async Task StartDownload(List<VideoListItem> selection) {
            var DownloadList = selection.Where(s => s.Status == VideoListItemStatusEnum.HigherQualityAvailable);
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
                    SetStatus(item, VideoListItemStatusEnum.Downloading, null);
                    if (ItemData.FileName != null && File.Exists(Settings.NaturalGroundingFolder + ItemData.FileName))
                        FileOperationAPIWrapper.MoveToRecycleBin(Settings.NaturalGroundingFolder + ItemData.FileName);
                    await DownloadManager.DownloadVideoAsync(ItemData, -1, (sender, e) => {
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

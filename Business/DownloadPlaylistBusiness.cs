using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DataAccess;
using System.IO;
using EmergenceGuardian.FFmpeg;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;
using EmergenceGuardian.Downloader;

namespace Business {
    /// <summary>
    /// Scans the playlist for videos with higher resolution available, or for broken links, and downloads those videos.
    /// </summary>
    public class DownloadPlaylistBusiness {
        public DownloadBusiness Manager;
        private Dictionary<Guid, ScanResultItem> scanResults = new Dictionary<Guid, ScanResultItem>(10);

        public DownloadPlaylistBusiness() {
        }

        public async Task StartScan(List<VideoListItem> selection, CancellationToken cancel) {
            await selection.ForEachAsync(5, cancel, async item => {
                Media VideoData = PlayerAccess.GetVideoById(item.MediaId.Value);
                if (VideoData != null && !item.IsBusy) {
                    try {
                        // Query the server for media info.
                        SetStatus(item, VideoListItemStatusEnum.DownloadingInfo);
                        VideoInfo VInfo = await DownloadManager.GetDownloadInfoAsync(VideoData.DownloadUrl);
                        if (VInfo != null) {
                            // Get the highest resolution format.
                            BestFormatInfo VideoFormat = DownloadBusiness.SelectBestFormat(VInfo.Streams);
                            if (VideoFormat == null || VideoFormat.BestVideo == null)
                                SetStatus(item, VideoListItemStatusEnum.Failed);
                            else
                                SetStatus(item, await IsHigherQualityAvailable(VideoFormat, Settings.NaturalGroundingFolder + VideoData.FileName));
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
        /// Returns the InfoReader from the last call to IsHigherQualityAvailable.
        /// Note: this is not thread-safe! Only works for 1 call at a time (per class instance).
        /// </summary>
        public FFmpegProcess LastInfoReader { get; set; }

        /// <summary>
        /// Returns whether the local file should be replaced by the YouTube version.
        /// </summary>
        /// <param name="serverFile">The information of the available server file.</param>
        /// <param name="localFile">A path to the local file.</param>
        /// <returns>True if the local file should be replaced.</returns>
        public async Task<VideoListItemStatusEnum> IsHigherQualityAvailable(BestFormatInfo serverFile, string localFile) {
            // If there is no local file, it should be downloaded.
            if (!File.Exists(localFile))
                return VideoListItemStatusEnum.HigherQualityAvailable;

            // Read local file info.
            string LocalFileExt = Path.GetExtension(localFile).ToLower();
            FFmpegProcess InfoReader = await Task.Run(() => MediaInfo.GetFileInfo(localFile));
            LastInfoReader = InfoReader;

            VideoListItemStatusEnum Result = IsHigherQualityAvailableInternal(serverFile, localFile, InfoReader);

            if (Result == VideoListItemStatusEnum.OK) {
                // Check if video has right container.
                string VFormat = InfoReader.VideoStream?.Format;
                string AFormat = InfoReader.AudioStream?.Format;
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

        public VideoListItemStatusEnum IsHigherQualityAvailableInternal(BestFormatInfo serverFile, string localFile, FFmpegProcess InfoReader) {
            string LocalFileExt = Path.GetExtension(localFile).ToLower();
            // If local file is FLV and there's another format available, it should be downloaded.
            if (LocalFileExt == ".flv" && serverFile.BestVideo.Container != Container.Flv)
                return VideoListItemStatusEnum.HigherQualityAvailable;

            // Original VCD files and files of unrecognized extensions should not be replaced.
            if (!DownloadManager.DownloadedExtensions.Contains(LocalFileExt) || InfoReader?.VideoStream?.Format == "mpeg1video") {
                serverFile.StatusText = "Not from YouTube";
                return VideoListItemStatusEnum.OK;
            }

            // For server file size, estimate 4% extra for audio. Estimate 30% advantage for VP9 format. non-DASH WebM is VP8 and doesn't have that bonus.
            long ServerFileSize = (long)(serverFile.BestVideo.Size * 1.04);
            if (DownloadManager.GetVideoEncoding(serverFile.BestVideo) == VideoEncoding.Vp9)
                ServerFileSize = (long)(ServerFileSize * 1.3);
            long LocalFileSize = new FileInfo(localFile).Length;
            if (InfoReader?.VideoStream?.Format == "vp9")
                LocalFileSize = (long)(LocalFileSize * 1.3);

            // If server resolution is better, download unless local file is bigger.
            int LocalFileHeight = InfoReader?.VideoStream?.Height ?? 0;
            if (DownloadManager.GetVideoHeight(serverFile.BestVideo) > LocalFileHeight) {
                if (ServerFileSize > LocalFileSize)
                    return VideoListItemStatusEnum.HigherQualityAvailable;
                else if (ServerFileSize != 0)
                    return VideoListItemStatusEnum.OK;
            } else if (DownloadManager.GetVideoHeight(serverFile.BestVideo) < LocalFileHeight) {
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
            else if (Manager != null) {
                if (Manager.Options.PreferredFormat == SelectStreamFormat.MP4 && InfoReader.VideoStream?.Format == "vp9" && (DownloadManager.GetVideoEncoding(serverFile.BestVideo) == VideoEncoding.H264 || DownloadManager.GetVideoEncoding(serverFile.BestVideo) == VideoEncoding.H263))
                    DownloadVideo = true;
                else if (Manager.Options.PreferredFormat == SelectStreamFormat.VP9 && InfoReader.VideoStream?.Format == "h264" && (DownloadManager.GetVideoEncoding(serverFile.BestVideo) == VideoEncoding.Vp9 || DownloadManager.GetVideoEncoding(serverFile.BestVideo) == VideoEncoding.Vp8))
                    DownloadVideo = true;
            }

            if (InfoReader.AudioStream == null)
                DownloadAudio = true;
            // Can only upgrade is video length is same.
            else if (Math.Abs((InfoReader.FileDuration - serverFile.Duration).TotalSeconds) < 1) {
                // download audio and merge with local video.
                string LocalAudio = InfoReader.AudioStream.Format;
                AudioEncoding RemoteAudio = serverFile.BestAudio?.AudioEncoding ?? AudioEncoding.Aac;
                if (InfoReader.AudioStream.Bitrate == 0 && LocalAudio == "opus" || LocalAudio == "vorbis")
                    InfoReader.AudioStream.Bitrate = 160; // FFmpeg doesn't return bitrate of Opus and Vorbis audios, but it's 160.
                if (InfoReader.AudioStream.Bitrate == 0)
                    InfoReader.AudioStream.Bitrate = GetAudioBitrateMuxe(localFile);
                int LocalAudioBitRate = InfoReader.AudioStream.Bitrate;
                long ServerAudioBitRate = serverFile.BestAudio != null ? serverFile.BestAudio.Bitrate / 1024 : 0;
                // MediaInfo returns no bitrate for MKV containers with AAC audio.
                InfoReader.AudioStream.Bitrate = 160;
                if (LocalAudioBitRate > 0 || LocalFileExt != ".mkv") {
                    if ((LocalAudioBitRate == 0 || LocalAudioBitRate < ServerAudioBitRate * .8))
                        DownloadAudio = true;
                }
                if ((LocalAudio == "opus" && RemoteAudio == AudioEncoding.Vorbis) || (LocalAudio == "vorbis" && RemoteAudio == AudioEncoding.Opus))
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

        public async Task StartDownload(List<VideoListItem> selection, CancellationToken cancel) {
            // Download videos and upgrades.
            selection = selection.Where(s => s.CanDownload || s.Status == VideoListItemStatusEnum.WrongContainer).ToList();
            foreach (var item in selection) {
                item.IsBusy = true;
            }
            await selection.ForEachAsync(2, cancel, async item => {
                if (item.Status == VideoListItemStatusEnum.WrongContainer)
                    await Task.Run(() => FixContainer(item));
                else
                    await DownloadFile(item, item.Status == VideoListItemStatusEnum.BetterAudioAvailable ? DownloadAction.DownloadAudio : item.Status == VideoListItemStatusEnum.BetterVideoAvailable ? DownloadAction.DownloadVideo : DownloadAction.Download);
                item.IsBusy = false;
            });
            foreach (var item in selection) {
                item.IsBusy = false;
            }
        }

        private async Task DownloadFile(VideoListItem item, DownloadAction action) {
            if (Manager != null && item != null && item.MediaId != null) {
                Media ItemData = PlayerAccess.GetVideoById(item.MediaId.Value);
                if (ItemData != null) {
                    SetStatus(item, VideoListItemStatusEnum.Downloading, null);
                    await Manager.DownloadVideoAsync(ItemData, -1,
                        (sender, e) => {
                            SetStatus(item, e.DownloadInfo.IsCompleted ? VideoListItemStatusEnum.Done : VideoListItemStatusEnum.Failed);
                        }, action, null);
                }
            }
        }

        private void FixContainer(VideoListItem item) {
            SetStatus(item, VideoListItemStatusEnum.Converting);
            string SrcFile = Settings.NaturalGroundingFolder + item.FileName;
            if (item.FileName != null && File.Exists(SrcFile)) {
                FFmpegProcess FileInfo = MediaInfo.GetFileInfo(SrcFile);
                string Ext1 = Path.GetExtension(item.FileName).ToLower();
                string Ext2 = DownloadBusiness.GetFinalExtension(FileInfo.VideoStream?.Format, FileInfo.AudioStream?.Format);
                if ((Ext2 == ".mp4" || Ext2 == ".webm") && Ext1 != Ext2) {
                    string DstFile = item.FileName.Substring(0, item.FileName.Length - Ext1.Length) + Ext2;
                    if (MediaMuxer.Muxe(SrcFile, SrcFile, Settings.NaturalGroundingFolder + DstFile) == CompletionStatus.Success) {
                        FileOperationAPIWrapper.MoveToRecycleBin(SrcFile);
                        // Change database binding.
                        EditVideoBusiness Business = new EditVideoBusiness();
                        Media ExistingData = Business.GetVideoById(item.MediaId.Value);
                        if (ExistingData != null) {
                            // Edit video info.
                            ExistingData.FileName = DstFile;
                            Business.Save();
                            SetStatus(item, VideoListItemStatusEnum.Done);
                            return;
                        }
                    }
                }
            }
            SetStatus(item, VideoListItemStatusEnum.Failed);
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

        public static int GetAudioBitrateMuxe(string file) {
            int Result = 0;
            string TmpFile = PathManager.TempFilesPath + "GetBitrate - " + Path.GetFileNameWithoutExtension(file) + ".aac";
            if (MediaMuxer.Muxe(null, file, TmpFile) == CompletionStatus.Success) {
                FFmpegProcess InfoReader = MediaInfo.GetFileInfo(TmpFile);
                Result = InfoReader.AudioStream?.Bitrate ?? 0;
            }
            File.Delete(TmpFile);
            return Result;
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

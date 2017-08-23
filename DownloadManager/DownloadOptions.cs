using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergenceGuardian.Downloader {
    /// <summary>
    /// Contains options for automatic download stream selection.
    /// </summary>
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class DownloadOptions {
        public DownloadOptions() {
        }

        /// <summary>
        /// Gets or sets the maximum height of the video stream to download. Useful to limit bandwidth usage.
        /// </summary>
        public int MaxQuality { get; set; } = 0;
        public SelectStreamFormat PreferredFormat { get; set; } = SelectStreamFormat.Best;
        public SelectStreamFormat PreferredAudio { get; set; } = SelectStreamFormat.Best;
        public int SimultaneousDownloads { get; set; } = 2;

        public DownloadOptions Copy() {
            DownloadOptions Result = new DownloadOptions();
            Result.MaxQuality = MaxQuality;
            Result.PreferredFormat = PreferredFormat;
            Result.PreferredAudio = PreferredAudio;
            Result.SimultaneousDownloads = SimultaneousDownloads;
            return Result;
        }
    }

    /// <summary>
    /// Represents the types of video streams offered by YouTube.
    /// </summary>
    public enum SelectStreamFormat {
        Best,
        MP4,
        VP9
    }
}

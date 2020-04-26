using System;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Contains options for automatic download stream selection.
    /// </summary>
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class DownloadOptions
    {
        public DownloadOptions() { }

        /// <summary>
        /// Gets or sets the maximum height of the video stream to download. Useful to limit bandwidth usage.
        /// </summary>
        public int MaxQuality { get; set; } = 0;
        /// <summary>
        /// Gets or sets the preferred video stream type to download.
        /// </summary>
        public SelectStreamFormat PreferredFormat { get; set; } = SelectStreamFormat.Best;
        /// <summary>
        /// Gets or sets the preferred audio stream type to download.
        /// </summary>
        public SelectStreamFormat PreferredAudio { get; set; } = SelectStreamFormat.Best;
        /// <summary>
        /// Gets or sets the maximum amount of simultaneous downloads to allow.
        /// </summary>
        public int ConcurrentDownloads { get; set; } = 2;

        /// <summary>
        /// Returns a copy of this instance.
        /// </summary>
        public DownloadOptions Clone() => (DownloadOptions)MemberwiseClone();
    }
}

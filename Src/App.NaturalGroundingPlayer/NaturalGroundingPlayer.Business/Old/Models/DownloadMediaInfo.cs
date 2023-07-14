using System;
using HanumanInstitute.CommonServices;
using HanumanInstitute.Downloads;

namespace HanumanInstitute.NaturalGroundingPlayer.Models
{
    /// <summary>
    /// Holds extra information related to the download task specific to this application.
    /// </summary>
    public class DownloadMediaInfo
    {
        public DownloadMediaInfo(Media media, IDownloadTask download)
        {
            Media = media.CheckNotNull(nameof(media));
            Download = download.CheckNotNull(nameof(download));
        }

        /// <summary>
        /// Gets or sets information about the media element being downloaded.
        /// </summary>
        public Media Media { get; set; }
        /// <summary>
        /// Indicate the position in the playlist for autoplay, or -1 to disable playback after complete.
        /// </summary>
        public int QueuePos { get; set; }

        public string Destination { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public IDownloadTask Download { get; set; }
    }
}

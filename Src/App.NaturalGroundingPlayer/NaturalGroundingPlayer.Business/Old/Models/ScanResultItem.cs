using System;

namespace HanumanInstitute.NaturalGroundingPlayer.Models
{
    /// <summary>
    /// Contains the result of a DownloadPlaylist scan.
    /// </summary>
    public class ScanResultItem
    {
        /// <summary>
        /// Returns the status of the media file, which operation can be performed.
        /// </summary>
        public VideoListItemStatus Status { get; set; }
        /// <summary>
        /// Returns the status text to be displayed to the user.
        /// </summary>
        public string StatusText { get; set; } = string.Empty;

        public ScanResultItem() { }

        public ScanResultItem(VideoListItemStatus status, string statusText)
        {
            this.Status = status;
            this.StatusText = statusText;
        }
    }
}

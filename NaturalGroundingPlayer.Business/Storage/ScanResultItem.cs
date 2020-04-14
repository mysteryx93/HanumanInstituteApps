using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {
    /// <summary>
    /// Contains the result of a DownloadPlaylist scan.
    /// </summary>
    public class ScanResultItem {
        /// <summary>
        /// Returns the status of the media file, which operation can be performed.
        /// </summary>
        public VideoListItemStatusEnum Status { get; set; }
        /// <summary>
        /// Returns the status text to be displayed to the user.
        /// </summary>
        public string StatusText { get; set; }

        public ScanResultItem() { }

        public ScanResultItem(VideoListItemStatusEnum status, string statusText) {
            this.Status = status;
            this.StatusText = statusText;
        }
    }
}

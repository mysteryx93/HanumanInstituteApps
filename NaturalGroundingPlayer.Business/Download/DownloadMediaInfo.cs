using EmergenceGuardian.DownloadManager;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    /// <summary>
    /// Holds extra information related to the download task specific to this application.
    /// </summary>
    public class DownloadMediaInfo : DownloadTaskInfo {
        public DownloadMediaInfo() { }

        public DownloadMediaInfo(string url, string destination, string title, bool downloadVideo, bool downloadAudio, DownloadTaskEventHandler callback, DownloadOptions options) : 
            base(url, destination, title, downloadVideo, downloadAudio, callback, options) { }

        /// <summary>
        /// Gets or sets information about the media element being downloaded.
        /// </summary>
        public Media Media { get; set; }
        /// <summary>
        /// Indicate the position in the playlist for autoplay, or -1 to disable playback after complete.
        /// </summary>
        public int QueuePos { get; set; }
    }
}


namespace HanumanInstitute.DownloadManager {
    /// <summary>
    /// Represents the type of stream.
    /// </summary>
    public enum StreamType {
        Video,
        Audio
    }

    /// <summary>
    /// Represents the current status of a download.
    /// </summary>
    public enum DownloadStatus {
        Waiting,
        Initializing,
        Downloading,
        Done,
        Canceled,
        Failed
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

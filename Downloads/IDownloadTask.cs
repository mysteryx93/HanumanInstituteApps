using System.Threading.Tasks;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Represents a media file download task, which can consist of multiple download streams.
    /// </summary>
    public interface IDownloadTask
    {
        /// <summary>
        /// Starts the download.
        /// </summary>
        Task DownloadAsync();
        /// <summary>
        /// Gets the download status information.
        /// </summary>
        DownloadTaskStatus TaskStatus { get; }
    }
}
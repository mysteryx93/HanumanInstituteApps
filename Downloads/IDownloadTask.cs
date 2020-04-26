using System;
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
        /// Gets or sets the URL to download from.
        /// </summary>
        Uri Url { get; }
        /// <summary>
        /// Gets or sets the destination path to store the file locally.
        /// </summary>
        string Destination { get; }
        /// <summary>
        /// Gets or sets whether to download the video stream.
        /// </summary>
        bool DownloadVideo { get; }
        /// <summary>
        /// Gets or sets whether to download the audio stream.
        /// </summary>
        bool DownloadAudio { get; }
        /// <summary>
        /// Gets the download status information.
        /// </summary>
        DownloadTaskStatus TaskStatus { get; }
    }
}

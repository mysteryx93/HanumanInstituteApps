using System;
using System.Collections.Generic;
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
        /// Gets the list of file streams being downloaded.
        /// </summary>
        IList<DownloadTaskFile> Files { get; }
        /// <summary>
        /// Gets the download status.
        /// </summary>
        DownloadStatus Status { get; }
        /// <summary>
        /// Cancels the download operation.
        /// </summary>
        void Cancel();
        /// <summary>
        /// Marks the download operation as failed.
        /// </summary>
        void Fail();
        /// <summary>
        /// Occurs before performing the muxing operation.
        /// </summary>
        event DownloadTaskEventHandler? BeforeMuxing;
        /// <summary>
        /// Occurus when progress information is updated.
        /// </summary>
        event DownloadTaskEventHandler? ProgressUpdated;
        /// <summary>
        /// Gets or sets the title of the downloaded media.
        /// </summary>
        string Title { get; set; }
        /// <summary>
        /// Gets or sets the progress of all streams as percentage.
        /// </summary>
        double ProgressValue { get; }
        /// <summary>
        /// Gets or sets the progress of all streams as a string representation.
        /// </summary>
        string ProgressText { get; set; }
    }
}

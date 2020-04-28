using System;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Creates new instances of IDownloadTasko.
    /// </summary>
    public interface IDownloadTaskFactory
    {
        /// <summary>
        /// Creates a new IDownloadTaskInfo initialized with specified values.
        /// </summary>
        /// <param name="url">The URL of the media to download.</param>
        /// <param name="destination">The destination path to store the file locally.</param>
        /// <param name="downloadVideo">Whether to download the video stream.</param>
        /// <param name="downloadAudio">Whether to download the audio stream.</param>
        /// <param name="taskStatus">An object containing download status information.</param>
        /// <param name="options">The download options.</param>
        /// <returns>The new IDownloadTask instance.</returns>
        IDownloadTask Create(Uri url, string destination, bool downloadVideo, bool downloadAudio, DownloadOptions options);
    }
}

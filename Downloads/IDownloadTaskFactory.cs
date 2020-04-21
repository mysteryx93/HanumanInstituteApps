using System;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Creates new instances of IDownloadTaskInfo.
    /// </summary>
    public interface IDownloadTaskFactory
    {
        /// <summary>
        /// Creates a new IDownloadTaskInfo with default values.
        /// </summary>
        /// <returns>The new IDownloadTaskInfo instance.</returns>
        DownloadTaskInfo Create();
        /// <summary>
        /// Creates a new IDownloadTaskInfo initialized with specified values.
        /// </summary>
        /// <param name="url">The URL to download.</param>
        /// <param name="destination">The destination path to store the file locally.</param>
        /// <param name="title">The title of the downloaded file.</param>
        /// <param name="downloadVideo">Whether to download the video stream.</param>
        /// <param name="downloadAudio">Whether to download the audio stream.</param>
        /// <param name="callback">The function to be called once download is completed.</param>
        /// <param name="options">The download options.</param>
        /// <returns>The new IDownloadTaskInfo instance.</returns>
        DownloadTaskInfo Create(Uri url, string destination, string title, bool downloadVideo, bool downloadAudio, DownloadTaskEventHandler callback, DownloadOptions options);
    }
}

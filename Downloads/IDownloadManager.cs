using System;
using System.Threading.Tasks;

namespace HanumanInstitute.Downloads
{

    /// <summary>
    /// Manages media file downloads.
    /// </summary>
    public interface IDownloadManager
    {
        /// <summary>
        /// Occurs when a new download task is added to the list.
        /// </summary>
        event DownloadTaskEventHandler DownloadAdded;
        /// <summary>
        /// Starts a new download task and adds it to the downloads pool.
        /// </summary>
        /// <param name="url">The URL of the video to download</param>
        /// <param name="destination">The destination where to save the downloaded file.</param>
        /// <param name="taskStatus">An object that will receive status updates for this download.</param>
        /// <param name="downloadVideo">Whether to download the video stream.</param>
        /// <param name="downloadAudio">Whether to downloda the audio stream.</param>
        Task<DownloadTaskStatus> DownloadAsync(Uri url, string destination, DownloadTaskStatus taskStatus, bool downloadVideo = true, bool downloadAudio = true);
        /// <summary>
        /// Returns the title for specified download URL.
        /// </summary>
        /// <param name="url">The download URL to get the title for.</param>
        /// <returns>A title, or null if it failed to retrieve the title.</returns>
        Task<string> GetVideoTitleAsync(Uri url);
        /// <summary>
        /// Returns the download info for specified URL.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>The download information.</returns>
        Task<VideoInfo> GetDownloadInfoAsync(Uri url);
    }
}

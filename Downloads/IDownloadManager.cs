using System;
using System.Net.Http;
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
        /// <param name="taskCreatedCallback">Callback to receive an instance of the download task.</param>
        /// <param name="downloadVideo">Whether to download the video stream.</param>
        /// <param name="downloadAudio">Whether to downloda the audio stream.</param>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        Task<DownloadStatus> DownloadAsync(Uri url, string destination, DownloadTaskEventHandler? taskCreatedCallback = null, bool downloadVideo = true, bool downloadAudio = true);
        /// <summary>
        /// Starts a new download task and adds it to the downloads pool.
        /// </summary>
        /// <param name="url">The URL of the video to download</param>
        /// <param name="destination">The destination where to save the downloaded file.</param>
        /// <param name="taskCreatedCallback">Callback to receive an instance of the download task.</param>
        /// <param name="downloadVideo">Whether to download the video stream.</param>
        /// <param name="downloadAudio">Whether to downloda the audio stream.</param>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        Task<DownloadStatus> DownloadAsync(string url, string destination, DownloadTaskEventHandler? taskCreatedCallback = null, bool downloadVideo = true, bool downloadAudio = true);
        /// <summary>
        /// Returns the title for specified download URL.
        /// </summary>
        /// <param name="url">The download URL to get the title for.</param>
        /// <returns>A title, or null if it failed to retrieve the title.</returns>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        Task<string> GetVideoTitleAsync(Uri url);
        /// <summary>
        /// Returns the title for specified download URL.
        /// </summary>
        /// <param name="url">The download URL to get the title for.</param>
        /// <returns>A title, or null if it failed to retrieve the title.</returns>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        Task<string> GetVideoTitleAsync(string url);
        /// <summary>
        /// Returns the download info for specified URL.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>The download information.</returns>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        Task<VideoInfo?> GetDownloadInfoAsync(Uri url);
        /// <summary>
        /// Returns the download info for specified URL.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>The download information.</returns>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        Task<VideoInfo?> GetDownloadInfoAsync(string url);
    }
}

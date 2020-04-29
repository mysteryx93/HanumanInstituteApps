using System;
using System.Net.Http;
using System.Threading.Tasks;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

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
        Task<DownloadStatus> DownloadAsync(Uri url, string destination, DownloadTaskEventHandler? taskCreatedCallback = null, bool downloadVideo = true, bool downloadAudio = true, DownloadOptions? options = null);
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
        Task<DownloadStatus> DownloadAsync(string url, string destination, DownloadTaskEventHandler? taskCreatedCallback = null, bool downloadVideo = true, bool downloadAudio = true, DownloadOptions? options = null);
        /// <summary>
        /// Returns information about specified video.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>The download information.</returns>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        Task<Video> QueryVideoAsync(string url);
        /// <summary>
        /// Returns information about specified video.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>The download information.</returns>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        Task<Video> QueryVideoAsync(Uri url);
        /// <summary>
        /// Returns streams information for specified video.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>Information about available streams.</returns>
        Task<StreamManifest> QueryStreamInfoAsync(string url);
        /// <summary>
        /// Returns streams information for specified video.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>Information about available streams.</returns>
        Task<StreamManifest> QueryStreamInfoAsync(Uri url);
    }
}

using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using YoutubeExplode.Videos;
using HanumanInstitute.CommonServices;
using HanumanInstitute.Downloads.Properties;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Manages media file downloads.
    /// </summary>
    public class DownloadManager : IDownloadManager, IDisposable
    {
        private readonly IDownloadTaskFactory _taskFactory;
        private readonly IOptions<DownloadOptions> _options;
        private readonly IYouTubeDownloader _youTube;
        public const int MaxConcurrentDownloads = 50;

        public DownloadManager(IDownloadTaskFactory taskFactory, IYouTubeDownloader youTube, IOptions<DownloadOptions> options)
        {
            _taskFactory = taskFactory.CheckNotNull(nameof(taskFactory));
            _youTube = youTube.CheckNotNull(nameof(youTube));
            _options = options.CheckNotNull(nameof(options));

            _pool = new SemaphoreSlimDynamic(options.Value.ConcurrentDownloads, MaxConcurrentDownloads);
        }

        /// <summary>
        /// Occurs when a new download task is added to the list.
        /// </summary>
        public event DownloadTaskEventHandler? DownloadAdded;

        private readonly SemaphoreSlimDynamic _pool;

        /// <summary>
        /// Starts a new download task and adds it to the downloads pool.
        /// </summary>
        /// <param name="url">The URL of the video to download</param>
        /// <param name="destination">The destination where to save the downloaded file.</param>
        /// <param name="taskStatus">An object that will receive status updates for this download.</param>
        /// <param name="downloadVideo">Whether to download the video stream.</param>
        /// <param name="downloadAudio">Whether to downloda the audio stream.</param>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        public async Task<DownloadTaskStatus> DownloadAsync(string url, string destination, DownloadTaskStatus? taskStatus = null, bool downloadVideo = true, bool downloadAudio = true) =>
            await DownloadAsync(new Uri(url), destination, taskStatus, downloadVideo, downloadAudio).ConfigureAwait(false);

        /// <summary>
        /// Starts a new download task and adds it to the downloads pool.
        /// </summary>
        /// <param name="url">The URL of the video to download</param>
        /// <param name="destination">The destination where to save the downloaded file.</param>
        /// <param name="taskStatus">An object that will receive status updates for this download.</param>
        /// <param name="downloadVideo">Whether to download the video stream.</param>
        /// <param name="downloadAudio">Whether to downloda the audio stream.</param>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        public async Task<DownloadTaskStatus> DownloadAsync(Uri url, string destination, DownloadTaskStatus? taskStatus = null, bool downloadVideo = true, bool downloadAudio = true)
        {
            url.CheckNotNull(nameof(url));
            destination.CheckNotNullOrEmpty(nameof(destination));
            taskStatus ??= new DownloadTaskStatus();

            var task = _taskFactory.Create(url, destination, downloadVideo, downloadAudio, taskStatus, _options.Value.Clone());

            // Notify UI of new download to show window.
            DownloadAdded?.Invoke(this, new DownloadTaskEventArgs(task));

            // Adjust pool size if ConcurrentDownloads settings changed.
            _pool.ChangeCapacity(_options.Value.ConcurrentDownloads);

            // Wait for pool to be ready.
            await _pool.WaitAsync().ConfigureAwait(false);

            // Download the file(s).
            await task.DownloadAsync().ConfigureAwait(false);

            // Release the pool and allow next download to start.
            _pool.Release();
            _pool.ChangeCapacity(_options.Value.ConcurrentDownloads);

            return taskStatus;
        }

        /// <summary>
        /// Returns the title for specified download URL.
        /// </summary>
        /// <param name="url">The download URL to get the title for.</param>
        /// <returns>A title, or null if it failed to retrieve the title.</returns>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        public async Task<string> GetVideoTitleAsync(string url) =>
            await GetVideoTitleAsync(new Uri(url)).ConfigureAwait(false);

        /// <summary>
        /// Returns the title for specified download URL.
        /// </summary>
        /// <param name="url">The download URL to get the title for.</param>
        /// <returns>A title, or null if it failed to retrieve the title.</returns>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        public async Task<string> GetVideoTitleAsync(Uri url)
        {
            var id = ParseVideoId(url.CheckNotNull(nameof(url)));

            var vInfo = await _youTube.QueryVideoAsync(id).ConfigureAwait(false);
            return vInfo.Title;
        }

        /// <summary>
        /// Returns the download info for specified URL.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>The download information.</returns>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        public async Task<VideoInfo?> GetDownloadInfoAsync(string url) =>
            await GetDownloadInfoAsync(new Uri(url)).ConfigureAwait(false);

        /// <summary>
        /// Returns the download info for specified URL.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>The download information.</returns>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        public async Task<VideoInfo?> GetDownloadInfoAsync(Uri url)
        {
            var id = ParseVideoId(url.CheckNotNull(nameof(url)));

            var t1 = _youTube.QueryVideoAsync(id);
            var t2 = _youTube.QueryStreamInfoAsync(id);
            await Task.WhenAll(new Task[] { t1, t2 }).ConfigureAwait(false);
            return new VideoInfo(t1.Result, t2.Result);
        }

        /// <summary>
        /// Parses the VideoId and throws a UriFormatException if the Uri is invalid.
        /// </summary>
        /// <param name="url">The Url to download from.</param>
        /// <returns>The parsed YouTube VideoId.</returns>
        /// <exception cref="UriFormatException">Uri does not contain a valid YouTube video ID.</exception>
        private VideoId ParseVideoId(Uri url)
        {
            url.CheckNotNull(nameof(url));

            try
            {
                var result = new VideoId(url.AbsoluteUri);
                if (string.IsNullOrEmpty(result.Value))
                {
                    throw new UriFormatException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidYouTubeId, url.AbsoluteUri));
                }
                return result;
            }
            catch (ArgumentException)
            {
                throw new UriFormatException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidYouTubeId, url.AbsoluteUri));
            }
        }


        private bool _disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _pool.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using YoutubeExplode.Videos;
using HanumanInstitute.CommonServices;
using HanumanInstitute.Downloads.Properties;
using YoutubeExplode.Videos.Streams;

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
        /// <param name="taskCreatedCallback">Callback to receive an instance of the download task.</param>
        /// <param name="downloadVideo">Whether to download the video stream.</param>
        /// <param name="downloadAudio">Whether to downloda the audio stream.</param>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        public async Task<DownloadStatus> DownloadAsync(string url, string destination, DownloadTaskEventHandler? taskCreatedCallback = null,
            bool downloadVideo = true, bool downloadAudio = true, DownloadOptions? options = null) =>
                await DownloadAsync(new Uri(url), destination, taskCreatedCallback, downloadVideo, downloadAudio, options).ConfigureAwait(false);

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
        public async Task<DownloadStatus> DownloadAsync(Uri url, string destination, DownloadTaskEventHandler? taskCreatedCallback = null,
            bool downloadVideo = true, bool downloadAudio = true, DownloadOptions? options = null)
        {
            url.CheckNotNull(nameof(url));
            destination.CheckNotNullOrEmpty(nameof(destination));
            if (!downloadVideo && !downloadAudio) { throw new ArgumentException(Resources.NoVideoNoAudio); }

            var task = _taskFactory.Create(url, destination, downloadVideo, downloadAudio, options ?? _options.Value.Clone());

            // Notify UI of new download to show window.
            var e = new DownloadTaskEventArgs(task);
            taskCreatedCallback?.Invoke(this, e);
            DownloadAdded?.Invoke(this, e);

            // Adjust pool size if ConcurrentDownloads settings changed.
            _pool.ChangeCapacity(_options.Value.ConcurrentDownloads);

            // Wait for pool to be ready.
            await _pool.WaitAsync().ConfigureAwait(false);

            // Download the file(s).
            await task.DownloadAsync().ConfigureAwait(false);

            // Release the pool and allow next download to start.
            _pool.TryRelease();
            _pool.ChangeCapacity(_options.Value.ConcurrentDownloads);

            return task.Status;
        }

        /// <summary>
        /// Returns information about specified video.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>The download information.</returns>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        public async Task<Video> QueryVideoAsync(string url) =>
            await QueryVideoAsync(new Uri(url)).ConfigureAwait(false);

        /// <summary>
        /// Returns information about specified video.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>The download information.</returns>
        /// <exception cref="HttpRequestException">There was an error while processing the request.</exception>
        /// <exception cref="TaskCanceledException">Download requred was cancelled or timed out.</exception>
        /// <exception cref="UriFormatException">The Url is invalid.</exception>
        public async Task<Video> QueryVideoAsync(Uri url)
        {
            var id = ParseVideoId(url.CheckNotNull(nameof(url)));
            return await _youTube.QueryVideoAsync(id).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns streams information for specified video.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>Information about available streams.</returns>
        public async Task<StreamManifest> QueryStreamInfoAsync(string url) =>
            await QueryStreamInfoAsync(new Uri(url)).ConfigureAwait(false);

        /// <summary>
        /// Returns streams information for specified video.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>Information about available streams.</returns>
        public async Task<StreamManifest> QueryStreamInfoAsync(Uri url)
        {
            var id = ParseVideoId(url.CheckNotNull(nameof(url)));
            return await _youTube.QueryStreamInfoAsync(id).ConfigureAwait(false);
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

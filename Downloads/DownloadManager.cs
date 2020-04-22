using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using YoutubeExplode.Videos;
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

        public DownloadManager() { }

        public DownloadManager(IDownloadTaskFactory taskFactory, IYouTubeDownloader youTube, IOptions<DownloadOptions> options)
        {
            _taskFactory = taskFactory.CheckNotNull(nameof(taskFactory));
            _youTube = youTube.CheckNotNull(nameof(youTube));
            _options = options.CheckNotNull(nameof(options));

            _pool = new SemaphoreSlim(0, options.Value.SimultaneousDownloads);
        }

        /// <summary>
        /// Occurs when a new download task is added to the list.
        /// </summary>
        public event DownloadTaskEventHandler DownloadAdded;

        private SemaphoreSlim _pool;

        /// <summary>
        /// Starts a new download task and adds it to the downloads pool.
        /// </summary>
        /// <param name="url">The URL of the video to download</param>
        /// <param name="destination">The destination where to save the downloaded file.</param>
        /// <param name="taskStatus">An object that will receive status updates for this download.</param>
        /// <param name="downloadVideo">Whether to download the video stream.</param>
        /// <param name="downloadAudio">Whether to downloda the audio stream.</param>
        public async Task<DownloadTaskStatus> DownloadAsync(Uri url, string destination, DownloadTaskStatus taskStatus, bool downloadVideo = true, bool downloadAudio = true)
        {
            url.CheckNotNull(nameof(url));
            destination.CheckNotNullOrEmpty(nameof(destination));
            taskStatus = taskStatus ?? new DownloadTaskStatus();

            var task = _taskFactory.Create(url, destination, downloadVideo, downloadAudio, taskStatus, _options.Value.Clone());

            // Notify UI of new download to show window.
            DownloadAdded?.Invoke(this, new DownloadTaskEventArgs(task));

            // Wait for pool to be ready.
            await _pool.WaitAsync().ConfigureAwait(false);

            // Download the file(s).
            await task.DownloadAsync().ConfigureAwait(false);

            // Release the pool and allow next download to start.
            _pool.Release();

            return taskStatus;
        }

        /// <summary>
        /// Returns the title for specified download URL.
        /// </summary>
        /// <param name="url">The download URL to get the title for.</param>
        /// <returns>A title, or null if it failed to retrieve the title.</returns>
        public async Task<string> GetVideoTitleAsync(Uri url)
        {
            url.CheckNotNull(nameof(url));
            try
            {
                Video VInfo = await _youTube.QueryVideoAsync(url).ConfigureAwait(false);
                return VInfo.Title;
            }
            catch
            {
            }
            return null;
        }

        /// <summary>
        /// Returns the download info for specified URL.
        /// </summary>
        /// <param name="url">The URL to probe.</param>
        /// <returns>The download information.</returns>
        public async Task<VideoInfo> GetDownloadInfoAsync(Uri url)
        {
            url.CheckNotNull(nameof(url));
            try
            {
                Task<Video> T1 = _youTube.QueryVideoAsync(url);
                Task<StreamManifest> T2 = _youTube.QueryStreamInfoAsync(url);
                await Task.WhenAll(new Task[] { T1, T2 }).ConfigureAwait(false);
                return new VideoInfo(T1.Result, T2.Result);
            }
            catch
            {
            }
            return null;
        }

        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _pool.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

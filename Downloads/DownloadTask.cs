using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HanumanInstitute.CommonServices;
using HanumanInstitute.Downloads.Properties;
using HanumanInstitute.FFmpeg;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Represents a media file download task, which can consist of multiple download streams.
    /// </summary>
    public class DownloadTask : IDownloadTask
    {
        private readonly IYouTubeDownloader _youTube;
        private readonly IYouTubeStreamSelector _streamSelector;
        private readonly IFileSystemService _fileSystem;
        private readonly IMediaMuxer _mediaMuxer;

        public DownloadTask(IYouTubeDownloader youTube, IYouTubeStreamSelector streamSelector, IFileSystemService fileSystem, IMediaMuxer mediaMuxer,
            Uri url, string destination, bool downloadVideo, bool downloadAudio, DownloadOptions options)
        {
            if (!downloadVideo && !downloadAudio) { throw new ArgumentException(Resources.NoVideoNoAudio); }

            _youTube = youTube.CheckNotNull(nameof(youTube));
            _streamSelector = streamSelector.CheckNotNull(nameof(streamSelector));
            _fileSystem = fileSystem.CheckNotNull(nameof(fileSystem));
            _mediaMuxer = mediaMuxer.CheckNotNull(nameof(mediaMuxer));

            Url = url.CheckNotNull(nameof(url));
            Destination = destination.CheckNotNullOrEmpty(nameof(destination));
            DownloadVideo = downloadVideo;
            DownloadAudio = downloadAudio;
            _options = options.CheckNotNull(nameof(options));
        }

        /// <summary>
        /// Gets or sets the URL to download from.
        /// </summary>
        public Uri Url { get; private set; }
        /// <summary>
        /// Gets or sets the destination path to store the file locally.
        /// </summary>
        public string Destination { get; private set; }
        /// <summary>
        /// Gets or sets whether to download the video stream.
        /// </summary>
        public bool DownloadVideo { get; private set; }
        /// <summary>
        /// Gets or sets whether to download the audio stream.
        /// </summary>
        public bool DownloadAudio { get; private set; }
        /// <summary>
        /// Gets or sets the download options.
        /// </summary>
        private readonly DownloadOptions _options;
        /// <summary>
        /// Gets the list of file streams being downloaded.
        /// </summary>
        public IList<DownloadTaskFile> Files { get; } = new List<DownloadTaskFile>();
        private DownloadStatus _status = DownloadStatus.Waiting;
        private bool _isCalled = false;

        /// <summary>
        /// Occurs before performing the muxing operation.
        /// </summary>
        public event DownloadTaskEventHandler? BeforeMuxing;
        /// <summary>
        /// Occurus when progress information is updated.
        /// </summary>
        public event DownloadTaskEventHandler? ProgressUpdated;
        /// <summary>
        /// Gets or sets the progress of all streams as percentage.
        /// </summary>
        public double ProgressValue { get; internal set; }
        /// <summary>
        /// Gets or sets the progress of all streams as a string representation.
        /// </summary>
        public string ProgressText { get; set; } = string.Empty;

        /// <summary>
        /// Starts the download.
        /// </summary>
        public async Task DownloadAsync()
        {
            // Ensure this method is called only once.
            if (_isCalled) { throw new InvalidOperationException(Resources.DownloadTaskCallOnce); }
            _isCalled = true;

            // The task might have been cancelled while waiting in queue.
            if (IsCancelled) { return; }

            // Start download.
            Status = DownloadStatus.Initializing;
            _fileSystem.EnsureDirectoryExists(Destination);

            // Query the download URL for the right file.
            StreamManifest? streams = null;
            try
            {
                streams = await _youTube.QueryStreamInfoAsync(Url.AbsoluteUri).ConfigureAwait(false);
            }
            catch (HttpRequestException) { }
            catch (TaskCanceledException) { }

            // Get the highest resolution format.
            IVideoStreamInfo? bestVideo = null;
            IAudioStreamInfo? bestAudio = null;
            if (streams != null)
            {
                bestVideo = _streamSelector.SelectBestVideo(streams, _options);
                bestAudio = _streamSelector.SelectBestAudio(streams, _options);
            }

            // Make sure we could retrieve download streams.
            if (bestVideo == null && bestAudio == null)
            {
                Status = DownloadStatus.Failed;
                return;
            }

            // Add the best video stream.
            if (bestVideo != null && DownloadVideo)
            {
                Files.Add(new DownloadTaskFile(StreamType.Video, new Uri(bestVideo.Url),
                    _fileSystem.GetPathWithoutExtension(Destination) + ".video",
                    bestVideo, bestVideo.Size.TotalBytes));
            }

            // Add the best audio stream.
            if (bestAudio != null && DownloadAudio)
            {
                Files.Add(new DownloadTaskFile(StreamType.Audio, new Uri(bestAudio.Url),
                    _fileSystem.GetPathWithoutExtension(Destination) + ".audio",
                    bestAudio, bestAudio.Size.TotalBytes));
            }

            // Add extension if Destination doesn't already include it.
            var ext = "." + _streamSelector.GetFinalExtension(bestVideo, bestAudio);
            if (!Destination.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
            {
                Destination += ext;
            }

            if (!IsCancelled)
            {
                // Download all files.
                var downloadTasks = Files.Select(f => DownloadFileAsync(f)).ToArray();
                await Task.WhenAll(downloadTasks).ConfigureAwait(false);

                if (IsCompleted())
                {
                    await DownloadCompletedAsync().ConfigureAwait(false);
                }
                else
                {
                    await Task.Delay(100).ConfigureAwait(false);
                    DeleteTempFiles();
                }
            }
        }

        /// <summary>
        /// Returns whether all files are finished downloading.
        /// </summary>
        private bool IsCompleted()
        {
            if (!IsCancelled && Files.Count > 0)
            {
                var result = true;
                foreach (var item in Files)
                {
                    if (!_fileSystem.File.Exists(item.Destination) || _fileSystem.FileInfo.FromFileName(item.Destination).Length < item.Length)
                    {
                        result = false;
                    }
                }
                return result;
            }
            return false;
        }

        /// <summary>
        /// Downloads a file stream. There can be multiple streams per download.
        /// </summary>
        /// <param name="fileInfo">Information about the specific file stream to download.</param>
        private async Task DownloadFileAsync(DownloadTaskFile fileInfo)
        {
            Status = DownloadStatus.Downloading;
            using var cancelToken = new CancellationTokenSource();

            try
            {
                await _youTube.DownloadAsync(
                    (IStreamInfo)fileInfo.Stream, fileInfo.Destination, ProgressHandler, cancelToken.Token).ConfigureAwait(false);

                void ProgressHandler(double percent)
                {
                    fileInfo.Downloaded = (long)(fileInfo.Length * percent);
                    UpdateProgress();

                    if (IsCancelled)
                    {
                        try
                        {
                            cancelToken.Cancel();
                        }
                        catch (ObjectDisposedException) { } // In case task is already done.
                    }
                }
            }
            catch (HttpRequestException) { Status = DownloadStatus.Failed; }
            catch (TaskCanceledException) { Status = DownloadStatus.Failed; }
        }

        /// <summary>
        /// Occurs when a file download is completed.
        /// </summary>
        private async Task DownloadCompletedAsync()
        {
            Status = DownloadStatus.Finalizing;
            _fileSystem.DeleteFileSilent(Destination);
            if (BeforeMuxing != null)
            {
                try
                {
                    BeforeMuxing?.Invoke(this, new DownloadTaskEventArgs(this));
                    // await Task.Run(() => BeforeMuxing?.Invoke(this, new DownloadTaskEventArgs(this))).ConfigureAwait(false);
                }
                catch
                {
                    Status = DownloadStatus.Failed;
                    throw;
                }
            }

            if (IsCancelled)
            {
                DeleteTempFiles();
                return;
            }

            var result = _fileSystem.File.Exists(Destination) ? CompletionStatus.Success : CompletionStatus.Failed;
            var videoFile = Files.FirstOrDefault(f => f.Type == StreamType.Video);
            var audioFile = Files.FirstOrDefault(f => f.Type == StreamType.Audio);

            // Muxe regularly unless muxing in event handler.
            if (result != CompletionStatus.Success)
            {
                result = await Task.Run(() => _mediaMuxer.Muxe(videoFile?.Destination, audioFile?.Destination, Destination)).ConfigureAwait(false);
            }

            if (result == CompletionStatus.Success)
            {
                result = _fileSystem.File.Exists(Destination) ? CompletionStatus.Success : CompletionStatus.Failed;
            }

            DeleteTempFiles();

            Status = result == CompletionStatus.Success ? DownloadStatus.Success : DownloadStatus.Failed;

            // Ensure download and merge succeeded.
            if (!_fileSystem.File.Exists(Destination))
            {
                Status = DownloadStatus.Failed;
            }

            if (Status != DownloadStatus.Success)
            {
                Status = DownloadStatus.Failed;
            }
        }

        /// <summary>
        /// Delete partially-downloaded files.
        /// </summary>
        private void DeleteTempFiles()
        {
            foreach (var item in Files)
            {
                _fileSystem.File.Delete(item.Destination);
            }
        }

        /// <summary>
        /// Gets the download status.
        /// </summary>
        public DownloadStatus Status
        {
            get => _status;
            private set
            {
                // Updates the status information.
                _status = value;
                ProgressText = GetStatusText(value);
                ProgressUpdated?.Invoke(this, new DownloadTaskEventArgs(this));

                static string GetStatusText(DownloadStatus status)
                {
                    switch (status)
                    {
                        case DownloadStatus.Waiting:
                            return Resources.StatusWaiting;
                        case DownloadStatus.Initializing:
                        case DownloadStatus.Downloading:
                            return Resources.StatusInitializing;
                        case DownloadStatus.Success:
                            return Resources.StatusDone;
                        case DownloadStatus.Finalizing:
                            return Resources.StatusFinalizing;
                        case DownloadStatus.Canceled:
                            return Resources.StatusCanceled;
                        case DownloadStatus.Failed:
                            return Resources.StatusFailed;
                        default:
                            return string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the status text with progress percentage. Only call this method if status is Downloading.
        /// </summary>
        private void UpdateProgress()
        {
            if (Status != DownloadStatus.Downloading) { return; }

            long totalBytes = 0;
            long downloaded = 0;
            var bytesTotalLoaded = true;
            foreach (var item in Files)
            {
                if (item.Length > 0)
                {
                    totalBytes += item.Length;
                }
                else
                {
                    bytesTotalLoaded = false;
                }

                downloaded += item.Downloaded;
            }
            if (bytesTotalLoaded)
            {
                ProgressValue = (double)downloaded / totalBytes;
                ProgressText = ProgressValue.ToString("p1", CultureInfo.CurrentCulture);
            }
            try
            {
                ProgressUpdated?.Invoke(this, new DownloadTaskEventArgs(this));
            }
            catch
            {
                Status = DownloadStatus.Failed;
                throw;
            }
        }

        /// <summary>
        /// Cancels the download operation.
        /// </summary>
        public void Cancel()
        {
            if (Status != DownloadStatus.Success && Status != DownloadStatus.Failed)
            {
                Status = DownloadStatus.Canceled;
            }
        }

        /// <summary>
        /// Marks the download operation as failed.
        /// </summary>
        public void Fail()
        {
            if (Status != DownloadStatus.Success)
            {
                Status = DownloadStatus.Failed;
            }
        }

        /// <summary>
        /// Returns whether the download was canceled or failed.
        /// </summary>
        private bool IsCancelled => (Status == DownloadStatus.Canceled || Status == DownloadStatus.Failed);
    }
}

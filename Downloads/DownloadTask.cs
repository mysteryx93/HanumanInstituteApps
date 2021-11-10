using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HanumanInstitute.CommonServices;
using HanumanInstitute.Downloads.Properties;
using HanumanInstitute.FFmpeg;
using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Represents a media file download task, which can consist of multiple download streams.
    /// </summary>
    public class DownloadTask : IDownloadTask
    {
        private readonly IYouTubeDownloader _youTube;
        private readonly IFileSystemService _fileSystem;
        private readonly IMediaMuxer _mediaMuxer;

        public DownloadTask(IYouTubeDownloader youTube, IFileSystemService fileSystem, IMediaMuxer mediaMuxer,
            StreamQueryInfo streamsQuery, string destination)
        {
            Query = streamsQuery.CheckNotNull(nameof(streamsQuery));
            if (Query.OutputVideo == null && Query.OutputAudio == null) { throw new ArgumentException(Resources.RequestHasNoStream); }

            _youTube = youTube.CheckNotNull(nameof(youTube));
            _fileSystem = fileSystem.CheckNotNull(nameof(fileSystem));
            _mediaMuxer = mediaMuxer.CheckNotNull(nameof(mediaMuxer));

            Destination = destination.CheckNotNullOrEmpty(nameof(destination));
        }

        /// <summary>
        /// Gets or sets the destination path to store the file locally.
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// Gets the analyzed download query.
        /// </summary>
        public StreamQueryInfo Query { get; private set; }
        /// <summary>
        /// Gets the list of file streams being downloaded.
        /// </summary>
        public IList<DownloadTaskFile> Files { get; } = new List<DownloadTaskFile>();
        private DownloadStatus _status = DownloadStatus.Waiting;
        private bool _isCalled;

        /// <summary>
        /// Occurs before performing the muxing operation.
        /// </summary>
        public event MuxeTaskEventHandler? Muxing;
        /// <summary>
        /// Occurus when progress information is updated.
        /// </summary>
        public event DownloadTaskEventHandler? ProgressUpdated;
        /// <summary>
        /// Gets the progress of all streams as percentage.
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
            //StreamManifest? streams = null;
            //try
            //{
            //    streams = await _youTube.QueryStreamInfoAsync(Url.AbsoluteUri).ConfigureAwait(false);
            //}
            //catch (HttpRequestException) { }
            //catch (TaskCanceledException) { }

            // Get the highest resolution format.
            //IVideoStreamInfo? Query.Video = null;
            //IAudioStreamInfo? Query.Audio = null;
            //var isMuxed = false;
            //if (streams != null)
            //{
            //    if (DownloadVideo)
            //    {
            //        Query.Video = _streamSelector.SelectQuery.Video(streams, _options);
            //        isMuxed = Query.Video is MuxedStreamInfo;
            //    }
            //    if (DownloadAudio && !isMuxed)
            //    {
            //        Query.Audio = _streamSelector.SelectQuery.Audio(streams, _options);
            //        if (!DownloadVideo)
            //        {
            //            isMuxed = Query.Audio is MuxedStreamInfo;
            //        }
            //    }
            //}

            // Make sure we could retrieve download streams.
            if (Query.Video == null && Query.Audio == null)
            {
                Status = DownloadStatus.Failed;
                return;
            }

            // Add the best video stream.
            if (Query.Video != null)
            {
                Files.Add(new DownloadTaskFile(true, Query.OutputAudio != null && Query.Audio == null, new Uri(Query.Video.Url),
                    _fileSystem.GetPathWithoutExtension(Destination) + ".tempvideo",
                    Query.Video, Query.Video.Size.Bytes));
            }

            // Add the best audio stream.
            if (Query.Audio != null)
            {
                Files.Add(new DownloadTaskFile(Query.OutputVideo != null && Query.Video == null, true, new Uri(Query.Audio.Url),
                    _fileSystem.GetPathWithoutExtension(Destination) + ".tempaudio",
                    Query.Audio, Query.Audio.Size.Bytes));
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
                    var fileExists = _fileSystem.File.Exists(item.Destination);
                    if (!fileExists || _fileSystem.FileInfo.FromFileName(item.Destination).Length < item.Length)
                    {
                        result = false;
                    }
                    // In case one file gets deleted before all streams complete.
                    if (!fileExists && item.Downloaded > 0)
                    {
                        Status = DownloadStatus.Failed;
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
            catch (System.IO.IOException) { Status = DownloadStatus.Failed; }
        }

        /// <summary>
        /// Occurs when a file download is completed.
        /// </summary>
        private async Task DownloadCompletedAsync()
        {
            Status = DownloadStatus.Finalizing;
            _fileSystem.DeleteFileSilent(Destination);

            var videoFile = Files.FirstOrDefault(f => f.HasVideo);
            var audioFile = Files.FirstOrDefault(f => f.HasAudio);

            // Handle already-muxed files containing both audio and video.
            if (videoFile?.HasAudio == true)
            {
                if (Query.DownloadVideo && Query.DownloadAudio)
                {
                    // Move file, no muxing required.
                    try
                    {
                        _fileSystem.File.Move(videoFile.Destination, Destination);
                        Status = DownloadStatus.Success;
                    }
                    catch (System.IO.IOException) { Status = DownloadStatus.Failed; }
                    catch (UnauthorizedAccessException) { Status = DownloadStatus.Failed; }
                }
                else
                {
                    // Extract one stream.
                    await MuxeStreams(Query.DownloadVideo ? videoFile?.Destination : null, Query.DownloadAudio ? videoFile?.Destination : null).ConfigureAwait(false);
                }
            }
            else
            {
                // Muxe normal streams.
                await MuxeStreams(videoFile?.Destination, audioFile?.Destination).ConfigureAwait(false);
            }

            DeleteTempFiles();
        }

        private async Task MuxeStreams(string? videoFile, string? audioFile)
        {
            // Allow custom muxing.
            if (Muxing != null)
            {
                try
                {
                    await Task.Run(() => Muxing?.Invoke(this, new MuxeTaskEventArgs(this, videoFile, audioFile))).ConfigureAwait(false);
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

            var muxeSuccess = _fileSystem.File.Exists(Destination);

            // If not done through event, do standard muxing.
            if (!muxeSuccess)
            {
                var taskResult = await Task.Run(() => _mediaMuxer.Muxe(videoFile, audioFile, Destination)).ConfigureAwait(false);
                muxeSuccess = taskResult == CompletionStatus.Success;
            }

            if (muxeSuccess)
            {
                muxeSuccess = _fileSystem.File.Exists(Destination);
            }

            Status = muxeSuccess ? DownloadStatus.Success : DownloadStatus.Failed;
        }

        /// <summary>
        /// Delete partially-downloaded files.
        /// </summary>
        private void DeleteTempFiles()
        {
            foreach (var item in Files)
            {
                _fileSystem.DeleteFileSilent(item.Destination);
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
                if (_status == value) { return; }

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

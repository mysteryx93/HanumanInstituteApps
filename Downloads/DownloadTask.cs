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
        /// Gets or sets the title of the downloaded media.
        /// </summary>
        public string Title { get; set; } = string.Empty;
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
            Video? vInfo = null;
            StreamManifest? vStream = null;

            try
            {
                var id = new VideoId(Url.AbsoluteUri);
                var t1 = _youTube.QueryVideoAsync(id);
                var t2 = _youTube.QueryStreamInfoAsync(id);
                await Task.WhenAll(new Task[] { t1, t2 }).ConfigureAwait(false);
                vInfo = t1.Result;
                vStream = t2.Result;
            }
            catch (HttpRequestException) { }
            catch (TaskCanceledException) { }

            // Get the highest resolution format.
            var bestFileNullable = vStream != null ? _streamSelector.SelectBestFormat(vStream, _options) : null;

            // Make sure we could retrieve download streams.
            if (bestFileNullable == null)
            {
                Status = DownloadStatus.Failed;
                return;
            }

            var bestFile = bestFileNullable;
            bestFile.Duration = vInfo?.Duration ?? TimeSpan.Zero;

            // Add the best video stream.
            if (bestFile.BestVideo != null && DownloadVideo)
            {
                Files.Add(new DownloadTaskFile(StreamType.Video, new Uri(bestFile.BestVideo.Url),
                    _fileSystem.GetPathWithoutExtension(Destination) + GetVideoExtension(_streamSelector.GetVideoEncoding(bestFile.BestVideo)),
                    bestFile.BestVideo, bestFile.BestVideo.Size.TotalBytes));
            }

            // Add the best audio stream.
            if (bestFile.BestAudio != null && DownloadAudio)
            {
                Files.Add(new DownloadTaskFile(StreamType.Audio, new Uri(bestFile.BestAudio.Url),
                    _fileSystem.GetPathWithoutExtension(Destination) + GetAudioExtension(bestFile.BestAudio.AudioCodec),
                    bestFile.BestAudio, bestFile.BestAudio.Size.TotalBytes));
            }

            // Add extension if Destination doesn't already include it.
            var ext = GetFinalExtension(bestFile.BestVideo, bestFile.BestAudio);
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
            if (Status != DownloadStatus.Downloading) { throw new InvalidOperationException(Resources.UpdateProgressInvalidStatus); }

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


        /// <summary>
        /// Returns the file extensions that can be created by the downloader.
        /// </summary>
        //public IList<string> DownloadedExtensions => _downloadedExtensions ?? (_downloadedExtensions = new List<string> { ".mp4", ".webm", ".mkv", ".flv" });
        //private IList<string> _downloadedExtensions;

        /// <summary>
        /// Returns the file extension for specified audio type.
        /// </summary>
        /// <param name="audio">The audio type to get file extension for.</param>
        /// <returns>The file extension.</returns>
#pragma warning disable CA1308 // Normalize strings to uppercase
        private static string GetVideoExtension(string codec)
        {
            codec.CheckNotNullOrEmpty(nameof(codec));
            return "." + codec.ToLowerInvariant();
        }

        public static string FileWithExt(string file, string ext)
        {
            file.CheckNotNullOrEmpty(nameof(file));
            ext.CheckNotNullOrEmpty(nameof(ext));
            return file + "." + ext.ToLowerInvariant();
        }

        /// <summary>
        /// Returns the file extension for specified audio type.
        /// </summary>
        /// <param name="codec">The audio type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        private static string GetAudioExtension(string codec)
        {
            codec.CheckNotNullOrEmpty(nameof(codec));
            return "." + codec.ToLowerInvariant();
        }
#pragma warning restore CA1308 // Normalize strings to uppercase

        /// <summary>
        /// Returns the file extension for specified video codec type.
        /// To avoid conflicting file names, the codec extension must be different than the final extension.
        /// </summary>
        /// <param name="video">The video type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        private static string GetCodecExtension(Container video)
        {
            if (video.Equals(Container.WebM))
            {
                return ".vp9";
            }
            else if (video.Equals(Container.Mp4))
            {
                return ".h264";
            }
            else if (video.Equals(Container.Tgpp))
            {
                return ".mp4v";
            }
            else
            {
                return ".avi";
            }
        }

        /// <summary>
        /// Returns the file extension for specified video type.
        /// To avoid conflicting file names, the codec extension must be different than the final extension.
        /// </summary>
        /// <param name="video">The video type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        private static string GetFinalExtension(IVideoStreamInfo? video, IAudioStreamInfo? audio)
        {
            if ((video == null || video.Container.Equals(Container.WebM)) && (audio == null || audio.Container.Equals(Container.WebM)))
            {
                return ".webm";
            }
            else if ((video == null || video.Container.Equals(Container.Mp4)) && (audio == null || audio.Container.Equals(Container.Mp4)))
            {
                return ".mp4";
            }
            else if (video != null && (video.Container.Equals(Container.Mp4) || video.Container.Equals(Container.WebM)))
            {
                return ".mkv";
            }
            else if (video != null)
            {
                return GetCodecExtension(video.Container);
            }
            else if (audio != null)
            {
                return GetCodecExtension(audio.Container);
            }
            else
            {
                return "";
            }
        }
    }
}

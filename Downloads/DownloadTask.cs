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
            Uri url, string destination, bool downloadVideo, bool downloadAudio, DownloadTaskStatus taskStatus, DownloadOptions options)
        {
            _youTube = youTube.CheckNotNull(nameof(youTube));
            _streamSelector = streamSelector.CheckNotNull(nameof(streamSelector));
            _fileSystem = fileSystem.CheckNotNull(nameof(fileSystem));
            _mediaMuxer = mediaMuxer.CheckNotNull(nameof(mediaMuxer));

            Url = url.CheckNotNull(nameof(url));
            Destination = destination.CheckNotNullOrEmpty(nameof(destination));
            DownloadVideo = downloadVideo;
            DownloadAudio = downloadAudio;
            TaskStatus = taskStatus.CheckNotNull(nameof(taskStatus));
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
        /// Gets or sets the list of file streams being downloaded.
        /// </summary>
        private readonly List<DownloadTaskFile> _files = new List<DownloadTaskFile>();
        /// <summary>
        /// Returns an object containing download status information.
        /// </summary>
        public DownloadTaskStatus TaskStatus { get; private set; } = new DownloadTaskStatus();
        private bool _isCalled = false;

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
                TaskStatus.Status = DownloadStatus.Failed;
                return;
            }

            var bestFile = bestFileNullable;
            bestFile.Duration = vInfo?.Duration ?? TimeSpan.Zero;

            // Add the best video stream.
            if (bestFile.BestVideo != null && DownloadVideo)
            {
                _files.Add(new DownloadTaskFile(StreamType.Video, new Uri(bestFile.BestVideo.Url),
                    _fileSystem.GetPathWithoutExtension(Destination) + GetVideoExtension(_streamSelector.GetVideoEncoding(bestFile.BestVideo)),
                    bestFile.BestVideo, bestFile.BestVideo.Size.TotalBytes));
            }

            // Add the best audio stream.
            if (bestFile.BestAudio != null && DownloadAudio)
            {
                _files.Add(new DownloadTaskFile(StreamType.Audio, new Uri(bestFile.BestAudio.Url),
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
                var downloadTasks = _files.Select(f => DownloadFileAsync(f)).ToArray();
                await Task.WhenAll(downloadTasks).ConfigureAwait(false);
            }
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
                    (IVideoStreamInfo)fileInfo.Stream, fileInfo.Destination, ProgressHandler, cancelToken.Token).ConfigureAwait(false);

                void ProgressHandler(double percent)
                {
                    if (IsCancelled)
                    {
                        cancelToken.Cancel();
                    }
                    else
                    {
                        fileInfo.Downloaded = (long)(fileInfo.Length * percent);
                        UpdateProgress();
                    }
                }
            }
            catch (HttpRequestException) { Status = DownloadStatus.Failed; }
            catch (TaskCanceledException) { Status = DownloadStatus.Failed; }

            // Detect whether this is the last file.
            fileInfo.Done = true;
            if (IsDone)
            {
                // Raise events for the last file part only.
                if (IsCompleted)
                {
                    await DownloadCompletedAsync().ConfigureAwait(false);
                }
                else if (IsCancelled)
                {
                    DownloadCanceled();
                }
            }
        }

        /// <summary>
        /// Occurs when a file download is completed.
        /// </summary>
        private async Task DownloadCompletedAsync()
        {
            await TaskStatus.OnBeforeMuxingAsync(this, new DownloadTaskEventArgs(this)).ConfigureAwait(false);

            var result = _fileSystem.File.Exists(Destination) ? CompletionStatus.Success : CompletionStatus.Failed;
            var videoFile = _files.FirstOrDefault(f => f.Type == StreamType.Video);
            var audioFile = _files.FirstOrDefault(f => f.Type == StreamType.Audio);

            // Muxe regularly unless muxing in event handler.
            if (result != CompletionStatus.Success)
            {
                result = await Task.Run(() => _mediaMuxer.Muxe(videoFile?.Destination, audioFile?.Destination, Destination)).ConfigureAwait(false);
            }

            if (result == CompletionStatus.Success)
            {
                result = _fileSystem.File.Exists(Destination) ? CompletionStatus.Success : CompletionStatus.Failed;
            }

            // Delete temp files.
            if (videoFile != null)
            {
                _fileSystem.File.Delete(videoFile.Destination);
            }

            if (audioFile != null)
            {
                _fileSystem.File.Delete(audioFile.Destination);
            }

            Status = result == CompletionStatus.Success ? DownloadStatus.Done : DownloadStatus.Failed;

            // Ensure download and merge succeeded.
            if (!FileHasContent(Destination))
            {
                Status = DownloadStatus.Failed;
            }

            //if (Status != DownloadStatus.Failed)
            //{
            //    DownloadTaskEventArgs CompletedArgs = new DownloadTaskEventArgs();
            //    await TaskStatus.OnCompletedAsync(this, CompletedArgs).ConfigureAwait(false);
            //}

            if (Status != DownloadStatus.Done)
            {
                Status = DownloadStatus.Failed;
            }
        }

        /// <summary>
        /// Occurs when a download task is cancelled.
        /// </summary>
        private void DownloadCanceled()
        {
            Thread.Sleep(100);

            // Delete partially-downloaded files.
            foreach (var item in _files)
            {
                _fileSystem.File.Delete(item.Destination);
            }
        }

        /// <summary>
        /// Sets the download status for specified download task and updates the progress text.
        /// </summary>
        /// <param name="download">The download task to update.</param>
        /// <param name="status">The new download status.</param>
        private DownloadStatus Status
        {
            get => TaskStatus.Status;
            set
            {
                TaskStatus.Status = value;
                TaskStatus.ProgressText = GetStatusText(value);

                static string GetStatusText(DownloadStatus status)
                {
                    switch (status)
                    {
                        case DownloadStatus.Waiting:
                            return Resources.StatusWaiting;
                        case DownloadStatus.Initializing:
                        case DownloadStatus.Downloading:
                            return Resources.StatusInitializing;
                        case DownloadStatus.Done:
                            return Resources.StatusDone;
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
            foreach (var item in _files)
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
                TaskStatus.ProgressValue = ((double)downloaded / totalBytes) * 100;
                TaskStatus.ProgressText = TaskStatus.ProgressValue.ToString("p1", CultureInfo.CurrentCulture);
            }
        }

        private bool IsDone => !_files.Any(d => !d.Done);

        /// <summary>
        /// Returns whether all files are finished downloading.
        /// </summary>
        private bool IsCompleted
        {
            get
            {
                if (_files.Count > 0)
                {
                    var result = true;
                    foreach (var item in _files)
                    {
                        if (item.Length == 0 || item.Downloaded < item.Length)
                        {
                            result = false;
                        }
                    }
                    return result;
                }
                return false;
            }
        }

        /// <summary>
        /// Returns whether the download was canceled or failed.
        /// </summary>
        private bool IsCancelled => (TaskStatus.Status == DownloadStatus.Canceled || TaskStatus.Status == DownloadStatus.Failed);


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

        /// <summary>
        /// Returns whether specified file exists and contains data (at least 500KB).
        /// </summary>
        /// <param name="fileName">The path of the file to check.</param>
        /// <returns>Whether the file contains data.</returns>
        private bool FileHasContent(string fileName)
        {
            return _fileSystem.File.Exists(fileName) && _fileSystem.FileInfo.FromFileName(fileName).Length > 524288;
        }
    }
}

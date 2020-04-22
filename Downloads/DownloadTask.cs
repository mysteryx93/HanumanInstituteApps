using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using HanumanInstitute.CommonServices;
using HanumanInstitute.Downloads.Properties;
using HanumanInstitute.FFmpeg;

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

            _url = url.CheckNotNull(nameof(url));
            _destination = destination.CheckNotNullOrEmpty(nameof(destination));
            _downloadVideo = downloadVideo;
            _downloadAudio = downloadAudio;
            TaskStatus = taskStatus.CheckNotNull(nameof(taskStatus));
            _options = options.CheckNotNull(nameof(options));
        }

        /// <summary>
        /// Gets or sets the URL to download from.
        /// </summary>
        private readonly Uri _url;
        /// <summary>
        /// Gets or sets the destination path to store the file locally.
        /// </summary>
        private string _destination;
        /// <summary>
        /// Gets or sets whether to download the video stream.
        /// </summary>
        private readonly bool _downloadVideo;
        /// <summary>
        /// Gets or sets whether to download the audio stream.
        /// </summary>
        private readonly bool _downloadAudio;
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
        private bool isCalled = false;

        /// <summary>
        /// Starts the download.
        /// </summary>
        public async Task DownloadAsync()
        {
            // Ensure this method is called only once.
            if (isCalled) { throw new InvalidOperationException(Resources.DownloadTaskCallOnce); }
            isCalled = true;

            // The task might have been cancelled while waiting in queue.
            if (IsCancelled) { return; }

            // Start download.
            Status = DownloadStatus.Initializing;
            _fileSystem.EnsureDirectoryExists(_destination);

            // Query the download URL for the right file.
            Video VInfo = null;
            StreamManifest VStream = null;

            try
            {
                VideoId id = new VideoId(_url.AbsoluteUri);
                Task<Video> T1 = _youTube.QueryVideoAsync(id);
                Task<StreamManifest> T2 = _youTube.QueryStreamInfoAsync(id);
                await Task.WhenAll(new Task[] { T1, T2 }).ConfigureAwait(false);
                VInfo = T1.Result;
                VStream = T2.Result;
            }
            catch { }

            // Make sure we could retrieve download streams.
            if (VStream == null)
            {
                Status = DownloadStatus.Failed;
                return;
            }

            // Get the highest resolution format.
            BestFormatInfo BestFile = _streamSelector.SelectBestFormat(VStream, _options);
            BestFile.Duration = VInfo.Duration;

            // Add the best video stream.
            if (BestFile.BestVideo != null && _downloadVideo)
            {
                _files.Add(new DownloadTaskFile(StreamType.Video, new Uri(BestFile.BestVideo.Url),
                    _fileSystem.GetPathWithoutExtension(_destination) + GetVideoExtension(_streamSelector.GetVideoEncoding(BestFile.BestVideo)),
                    BestFile.BestVideo, BestFile.BestVideo.Size.TotalBytes));
            }

            // Add the best audio stream.
            if (BestFile.BestAudio != null && _downloadAudio)
            {
                _files.Add(new DownloadTaskFile(StreamType.Audio, new Uri(BestFile.BestAudio.Url),
                    _fileSystem.GetPathWithoutExtension(_destination) + GetAudioExtension(BestFile.BestAudio.AudioCodec),
                    BestFile.BestAudio, BestFile.BestAudio.Size.TotalBytes));
            }

            // Add extension if Destination doesn't already include it.
            string Ext = GetFinalExtension(BestFile.BestVideo, BestFile.BestAudio);
            if (!_destination.EndsWith(Ext, StringComparison.InvariantCultureIgnoreCase))
            {
                _destination += Ext;
            }

            if (!IsCancelled)
            {
                // Download all files.
                Task[] DownloadTasks = _files.Select(f => DownloadFileAsync(, f)).ToArray();
                await Task.WhenAll(DownloadTasks).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Downloads a file stream. There can be multiple streams per download.
        /// </summary>
        /// <param name="">Information about the download task.</param>
        /// <param name="fileInfo">Information about the specific file stream to download.</param>
        private async Task DownloadFileAsync(, DownloadTaskFile fileInfo)
        {
            Status = DownloadStatus.Downloading;
            using (CancellationTokenSource cancelToken = new CancellationTokenSource())
            {
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
                catch
                {
                    Status = DownloadStatus.Failed;
                }
            }

            // Detect whether this is the last file.
            fileInfo.Done = true;
            if (IsDone)
            {
                // Raise events for the last file part only.
                if (IsCompleted)
                {
                    try
                    {
                        await DownloadCompletedAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        Status = DownloadStatus.Failed;
                    }
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
        /// <param name="">Information about the download task.</param>
        private async Task DownloadCompletedAsync()
        {
            await TaskStatus.OnBeforeMuxingAsync(this, new DownloadTaskEventArgs()).ConfigureAwait(false);

            CompletionStatus result = _fileSystem.File.Exists(_destination) ? CompletionStatus.Success : CompletionStatus.Failed;
            DownloadTaskFile videoFile = _files.FirstOrDefault(f => f.Type == StreamType.Video);
            DownloadTaskFile audioFile = _files.FirstOrDefault(f => f.Type == StreamType.Audio);

            // Muxe regularly unless muxing in event handler.
            if (result != CompletionStatus.Success)
            {
                result = await Task.Run(() => _muxer.Muxe(videoFile?.Destination, audioFile?.Destination, _destination)).ConfigureAwait(false);
            }

            if (result == CompletionStatus.Success)
            {
                result = _fileSystem.File.Exists(_destination) ? CompletionStatus.Success : CompletionStatus.Failed;
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
            if (!FileHasContent(_destination))
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
        /// <param name="">Information about the download task.</param>
        private void DownloadCanceled()
        {
            Thread.Sleep(100);

            // Delete partially-downloaded files.
            foreach (DownloadTaskFile item in _files)
            {
                _fileSystem.File.Delete(item.Destination);
            }
        }

        /// <summary>
        /// Sets the download status for specified download task and updates the progress text.
        /// </summary>
        /// <param name="download">The download task to update.</param>
        /// <param name="status">The new download status.</param>
        private DownloadStatus Status {
            get => TaskStatus.Status;
            set {
                TaskStatus.Status = value;
                TaskStatus.ProgressText = GetStatusText(value);

                string GetStatusText(DownloadStatus status)
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
            bool bytesTotalLoaded = true;
            foreach (DownloadTaskFile item in _files)
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
        private bool IsCompleted {
            get {
                if (_files.Count > 0)
                {
                    bool result = true;
                    foreach (DownloadTaskFile item in _files)
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
        private static string GetFinalExtension(IVideoStreamInfo video, IAudioStreamInfo audio)
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

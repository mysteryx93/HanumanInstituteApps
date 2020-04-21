using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using HanumanInstitute.CommonServices;
using HanumanInstitute.FFmpeg;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Manages media file downloads.
    /// </summary>
    public class DownloadManager : IDownloadManager
    {
        /// <summary>
        /// Gets the list of active and pending downloads.
        /// </summary>
        public ObservableCollection<DownloadTaskInfo> DownloadsList { get; private set; } = new ObservableCollection<DownloadTaskInfo>();
        /// <summary>
        /// Occurs when a new download task is added to the list.
        /// </summary>
        public event EventHandler DownloadAdded;
        /// <summary>
        /// Occurs before performing the muxing operation.
        /// </summary>
        public event DownloadTaskEventHandler BeforeMuxing;
        /// <summary>
        /// Occurs when the download is completed.
        /// </summary>
        public event DownloadTaskEventHandler Completed;

        private readonly YouTubeDownloader _youTube;
        private readonly IFileSystemService _fileSystem;
        private readonly IMediaMuxer _muxer;
        private readonly IYouTubeStreamSelector _streamSelector;

        public DownloadManager() { }

        public DownloadManager(IFileSystemService fileSystemService, IMediaMuxer mediaMuxer, YouTubeDownloader youTube, IYouTubeStreamSelector streamSelector)
        {
            _fileSystem = fileSystemService.CheckNotNull(nameof(fileSystemService));
            _muxer = mediaMuxer.CheckNotNull(nameof(mediaMuxer));
            _youTube = youTube.CheckNotNull(nameof(youTube));
            _streamSelector = streamSelector.CheckNotNull(nameof(streamSelector));
        }


        /// <summary>
        /// Starts a new download task and add it to the list.
        /// </summary>
        /// <param name="downloadTask">Information about the download task.</param>
        public async Task AddDownloadAsync(DownloadTaskInfo downloadTask)
        {
            downloadTask.CheckNotNull(nameof(downloadTask));

            if (IsDownloadDuplicate(downloadTask.Url))
            {
                return;
            }

            _fileSystem.EnsureDirectoryExists(downloadTask.Destination);

            DownloadsList.Insert(0, downloadTask);
            // Notify UI of new download to show window.
            DownloadAdded?.Invoke(this, new EventArgs());

            if (DownloadsList.Where(d => d.Status == DownloadStatus.Downloading || d.Status == DownloadStatus.Initializing).Count() < downloadTask.Options.SimultaneousDownloads)
            {
                await InititalizeYouTubeDownloadAsync(downloadTask).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Initializes an asynchronous file download task.
        /// </summary>
        /// <param name="downloadTask">The download task to start.</param>
        private async Task InititalizeYouTubeDownloadAsync(DownloadTaskInfo downloadTask)
        {
            downloadTask.Status = DownloadStatus.Initializing;

            // Query the download URL for the right file.
            Video VInfo = null;
            StreamManifest VStream = null;

            try
            {
                VideoId id = new VideoId(downloadTask.Url.AbsoluteUri);
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
                downloadTask.Status = DownloadStatus.Failed;
                RaiseCallback(downloadTask);
                return;
            }

            // Get the highest resolution format.
            BestFormatInfo BestFile = _streamSelector.SelectBestFormat(VStream, downloadTask.Options);
            BestFile.Duration = VInfo.Duration;

            // Add the best video stream.
            if (BestFile.BestVideo != null && downloadTask.DownloadVideo)
            {
                downloadTask.Files.Add(new DownloadFileInfo(StreamType.Video, new Uri(BestFile.BestVideo.Url),
                    GetPathNoExt(downloadTask.Destination) + GetVideoExtension(_streamSelector.GetVideoEncoding(BestFile.BestVideo)),
                    BestFile.BestVideo, BestFile.BestVideo.Size.TotalBytes));
            }

            // Add the best audio stream.
            if (BestFile.BestAudio != null && downloadTask.DownloadAudio)
            {
                downloadTask.Files.Add(new DownloadFileInfo(StreamType.Audio, new Uri(BestFile.BestAudio.Url),
                    GetPathNoExt(downloadTask.Destination) + GetAudioExtension(BestFile.BestAudio.AudioCodec),
                    BestFile.BestAudio, BestFile.BestAudio.Size.TotalBytes));
            }

            // Add extension if Destination doesn't already include it.
            string Ext = DownloadManager.GetFinalExtension(BestFile.BestVideo, BestFile.BestAudio);
            if (!downloadTask.Destination.EndsWith(Ext, StringComparison.InvariantCultureIgnoreCase))
            {
                downloadTask.Destination += Ext;
            }

            await StartDownloadTaskAsync(downloadTask).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs a file download task which can consist of several file streams.
        /// </summary>
        /// <param name="downloadTask">Information about the download task.</param>
        private async Task StartDownloadTaskAsync(DownloadTaskInfo downloadTask)
        {
            if (!downloadTask.IsCancelled)
            {
                // Download all files.
                Task[] DownloadTasks = downloadTask.Files.Select(f => DownloadFileAsync(downloadTask, f)).ToArray();
                await Task.WhenAll(DownloadTasks).ConfigureAwait(false);
            }
            else
            {
                RaiseCallback(downloadTask);
            }
        }

        /// <summary>
        /// Downloads a file stream. There can be multiple streams per download.
        /// </summary>
        /// <param name="downloadTask">Information about the download task.</param>
        /// <param name="fileInfo">Information about the specific file stream to download.</param>
        private async Task DownloadFileAsync(DownloadTaskInfo downloadTask, DownloadFileInfo fileInfo)
        {
            downloadTask.Status = DownloadStatus.Downloading;
            using (CancellationTokenSource cancelToken = new CancellationTokenSource())
            {
                try
                {
                    await _youTube.DownloadAsync(
                        (IVideoStreamInfo)fileInfo.Stream, fileInfo.Destination, ProgressHandler, cancelToken.Token).ConfigureAwait(false);

                    void ProgressHandler(double percent)
                    {
                        if (downloadTask.IsCancelled)
                        {
                            cancelToken.Cancel();
                        }
                        else
                        {
                            fileInfo.Downloaded = (long)(fileInfo.Length * percent);
                            downloadTask.UpdateProgress();
                        }
                    }
                }
                catch
                {
                    downloadTask.Status = DownloadStatus.Failed;
                }
            }

            // Detect whether this is the last file.
            fileInfo.Done = true;
            if (downloadTask.Files.Any(d => !d.Done) == false)
            {
                System.Runtime.CompilerServices.ConfiguredTaskAwaitable NextDownload = StartNextDownloadAsync().ConfigureAwait(false);

                // Raise events for the last file part only.
                if (downloadTask.IsCompleted)
                {
                    try
                    {
                        await DownloadCompletedAsync(downloadTask).ConfigureAwait(false);
                    }
                    catch
                    {
                        downloadTask.Status = DownloadStatus.Failed;
                    }
                }
                else if (downloadTask.IsCancelled)
                {
                    DownloadCanceled(downloadTask);
                }

                RaiseCallback(downloadTask);

                await NextDownload;
            }
        }

        /// <summary>
        /// Occurs when a file download is completed.
        /// </summary>
        /// <param name="downloadTask">Information about the download task.</param>
        private async Task DownloadCompletedAsync(DownloadTaskInfo downloadTask)
        {
            if (BeforeMuxing != null)
            {
                await Task.Run(() => BeforeMuxing.Invoke(this, new DownloadTaskEventArgs(downloadTask))).ConfigureAwait(false);
            }

            CompletionStatus Result = _fileSystem.File.Exists(downloadTask.Destination) ? CompletionStatus.Success : CompletionStatus.Failed;
            DownloadFileInfo VideoFile = downloadTask.Files.FirstOrDefault(f => f.Type == StreamType.Video);
            DownloadFileInfo AudioFile = downloadTask.Files.FirstOrDefault(f => f.Type == StreamType.Audio);

            // Muxe regularly unless muxing in event handler.
            if (Result != CompletionStatus.Success)
            {
                Result = await Task.Run(() => _muxer.Muxe(VideoFile?.Destination, AudioFile?.Destination, downloadTask.Destination)).ConfigureAwait(false);
            }

            if (Result == CompletionStatus.Success)
            {
                Result = _fileSystem.File.Exists(downloadTask.Destination) ? CompletionStatus.Success : CompletionStatus.Failed;
            }

            // Delete temp files.
            if (VideoFile != null)
            {
                _fileSystem.File.Delete(VideoFile.Destination);
            }

            if (AudioFile != null)
            {
                _fileSystem.File.Delete(AudioFile.Destination);
            }

            downloadTask.Status = Result == CompletionStatus.Success ? DownloadStatus.Done : DownloadStatus.Failed;

            // Ensure download and merge succeeded.
            if (!FileHasContent(downloadTask.Destination))
            {
                downloadTask.Status = DownloadStatus.Failed;
            }

            if (downloadTask.Status != DownloadStatus.Failed)
            {
                // Invoke 2 callbacks: one on the Download Manager, one specified when adding the download.
                DownloadTaskEventArgs CompletedArgs = new DownloadTaskEventArgs(downloadTask);
                await Task.Run(() =>
                {
                    Completed?.Invoke(this, CompletedArgs);
                    downloadTask.Callback?.Invoke(this, CompletedArgs);
                }).ConfigureAwait(false);
            }

            if (downloadTask.Status != DownloadStatus.Done)
            {
                downloadTask.Status = DownloadStatus.Failed;
            }
        }

        /// <summary>
        /// Occurs when a download task is cancelled.
        /// </summary>
        /// <param name="downloadTask">Information about the download task.</param>
        private void DownloadCanceled(DownloadTaskInfo downloadTask)
        {
            //if (downloadTask.Status == DownloadStatus.Canceled)
            //{
            //    downloadTask.Status = DownloadStatus.Canceled;
            //}
            //else if (downloadTask.Status == DownloadStatus.Failed)
            //{
            //    downloadTask.Status = DownloadStatus.Failed;
            //}

            Thread.Sleep(100);

            // Delete partially-downloaded files.
            foreach (DownloadFileInfo item in downloadTask.Files)
            {
                _fileSystem.File.Delete(item.Destination);
            }
        }

        /// <summary>
        /// Starts the next pending download in the list.
        /// </summary>
        private async Task StartNextDownloadAsync()
        {
            // Start next download.
            DownloadTaskInfo NextDownload = DownloadsList.Where(d => d.Status == DownloadStatus.Waiting).LastOrDefault();
            if (NextDownload != null)
            {
                await InititalizeYouTubeDownloadAsync(NextDownload).ConfigureAwait(false);
            }
        }


        /// <summary>
        /// Returns the file extensions that can be created by the downloader.
        /// </summary>
        public IList<string> DownloadedExtensions => _downloadedExtensions ?? (_downloadedExtensions = new List<string> { ".mp4", ".webm", ".mkv", ".flv" });
        private IList<string> _downloadedExtensions;

        /// <summary>
        /// Returns specified path without its file extension.
        /// </summary>
        /// <param name="path">The path to truncate extension from.</param>
        /// <returns>A file path with no file extension.</returns>
        public string GetPathNoExt(string path)
        {
            path.CheckNotNullOrEmpty(nameof(path));
            return _fileSystem.Path.Combine(_fileSystem.Path.GetDirectoryName(path), _fileSystem.Path.GetFileNameWithoutExtension(path));
        }

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

        /// <summary>
        /// Returns whether specified URL is already being downloaded.
        /// </summary>
        /// <param name="url">The URL to check for.</param>
        /// <returns>Whether the URL is already in the list of downloads.</returns>
        public bool IsDownloadDuplicate(Uri url)
        {
            bool Result = (from d in DownloadsList
                           where (d.Status == DownloadStatus.Downloading || d.Status == DownloadStatus.Initializing || d.Status == DownloadStatus.Waiting) &&
                             Uri.Compare(d.Url, url, UriComponents.Host | UriComponents.PathAndQuery, UriFormat.SafeUnescaped, StringComparison.InvariantCultureIgnoreCase) == 0
                           select d).Any();
            return Result;
        }

        /// <summary>
        /// Returns whether specified file exists and contains data (at least 500KB).
        /// </summary>
        /// <param name="fileName">The path of the file to check.</param>
        /// <returns>Whether the file contains data.</returns>
        public bool FileHasContent(string fileName)
        {
            return _fileSystem.File.Exists(fileName) && _fileSystem.FileInfo.FromFileName(fileName).Length > 524288;
        }

        /// <summary>
        /// Invokes the callback specified when adding the download.
        /// </summary>
        /// <param name="downloadTask">Information about the download task.</param>
        private void RaiseCallback(DownloadTaskInfo downloadTask)
        {
            downloadTask?.Callback(this, new DownloadTaskEventArgs(downloadTask));
        }
    }
}

using System;
using YoutubeExplode;
using HanumanInstitute.CommonServices;
using HanumanInstitute.FFmpeg;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Creates new instances of IDownloadTaskInfo.
    /// </summary>
    public class DownloadTaskFactory : IDownloadTaskFactory
    {
        private readonly IYouTubeDownloader _youTube;
        private readonly IYouTubeStreamSelector _streamSelector;
        private readonly IFileSystemService _fileSystem;
        private readonly IMediaMuxer _mediaMuxer;

        public DownloadTaskFactory() : this(new YouTubeDownloader(new YoutubeClient()), new YouTubeStreamSelector(), new FileSystemService(), new MediaMuxer(new ProcessWorkerFactory()))
        { }

        public DownloadTaskFactory(IYouTubeDownloader youTube, IYouTubeStreamSelector streamSelector, IFileSystemService fileSystem, IMediaMuxer mediaMuxer)
        {
            _youTube = youTube.CheckNotNull(nameof(youTube));
            _streamSelector = streamSelector.CheckNotNull(nameof(streamSelector));
            _fileSystem = fileSystem.CheckNotNull(nameof(fileSystem));
            _mediaMuxer = mediaMuxer.CheckNotNull(nameof(mediaMuxer));
        }

        /// <summary>
        /// Creates a new IDownloadTaskInfo initialized with specified values.
        /// </summary>
        /// <param name="url">The URL of the media to download.</param>
        /// <param name="destination">The destination path to store the file locally.</param>
        /// <param name="downloadVideo">Whether to download the video stream.</param>
        /// <param name="downloadAudio">Whether to download the audio stream.</param>
        /// <param name="taskStatus">An object containing download status information.</param>
        /// <param name="options">The download options.</param>
        /// <returns>The new IDownloadTask instance.</returns>
        public IDownloadTask Create(Uri url, string destination, bool downloadVideo, bool downloadAudio, DownloadTaskStatus taskStatus, DownloadOptions options)
        {
            return new DownloadTask(_youTube, _streamSelector, _fileSystem, _mediaMuxer,
                url, destination, downloadVideo, downloadAudio, taskStatus, options);
        }
    }
}

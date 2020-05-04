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
        private readonly IFileSystemService _fileSystem;
        private readonly IMediaMuxer _mediaMuxer;

        //public DownloadTaskFactory() : this(new YouTubeDownloader(new YoutubeClient()), new FileSystemService(), new MediaMuxer(new ProcessWorkerFactory()))
        //{ }

        public DownloadTaskFactory(IYouTubeDownloader youTube, IFileSystemService fileSystem, IMediaMuxer mediaMuxer)
        {
            _youTube = youTube.CheckNotNull(nameof(youTube));
            _fileSystem = fileSystem.CheckNotNull(nameof(fileSystem));
            _mediaMuxer = mediaMuxer.CheckNotNull(nameof(mediaMuxer));
        }

        /// <summary>
        /// Creates a new IDownloadTaskInfo initialized with specified values.
        /// </summary>
        /// <param name="streamQuery">The analyzed download query.</param>
        /// <param name="destination">The destination path to store the file locally.</param>
        /// <returns>The new IDownloadTask instance.</returns>
        public IDownloadTask Create(StreamQueryInfo streamQuery, string destination)
        {
            return new DownloadTask(_youTube, _fileSystem, _mediaMuxer,
                streamQuery, destination);
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Provides YouTube download functions.
    /// </summary>
    public class YouTubeDownloader : IYouTubeDownloader
    {
        private readonly YoutubeClient _youTube;

        public YouTubeDownloader(YoutubeClient youTube)
        {
            _youTube = youTube;
        }

        /// <summary>
        /// Returns information about specified YouTube video.
        /// </summary>
        /// <param name="url">The url of the video.</param>
        /// <returns>The video info.</returns>
        public async Task<Video> QueryVideoAsync(Uri url) => await QueryVideoAsync(GetVideoId(url)).ConfigureAwait(false);

        /// <summary>
        /// Returns information about specified YouTube video.
        /// </summary>
        /// <param name="id">The id of the video.</param>
        /// <returns>The video info.</returns>
        public async Task<Video> QueryVideoAsync(VideoId videoId)
        {
            videoId.Value.CheckNotNullOrEmpty(nameof(videoId));
            return await _youTube.Videos.GetAsync(videoId).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns streams information for specified YouTube video.
        /// </summary>
        /// <param name="url">The url of the video.</param>
        /// <returns>Information about available streams.</returns>
        public async Task<StreamManifest> QueryStreamInfoAsync(Uri url) => await QueryStreamInfoAsync(GetVideoId(url)).ConfigureAwait(false);

        /// <summary>
        /// Returns streams information for specified YouTube video.
        /// </summary>
        /// <param name="id">The id of the video.</param>
        /// <returns>Information about available streams.</returns>
        public async Task<StreamManifest> QueryStreamInfoAsync(VideoId videoId)
        {
            videoId.Value.CheckNotNullOrEmpty(nameof(videoId));
            return await _youTube.Videos.Streams.GetManifestAsync(videoId).ConfigureAwait(false);
        }

        /// <summary>
        /// Download the actual stream which is identified by the specified metadata to the specified file.
        /// </summary>
        /// <param name="streamInfo">The stream to download.</param>
        /// <param name="filePath">The file path where to save the download.</param>
        /// <param name="progressCallback">A method to call when progress is updated.</param>
        /// <param name="cancellationToken">A cancellation token allowing to cancel the operation.</param>
        public async Task DownloadAsync(IStreamInfo streamInfo, string filePath, Action<double> progressCallback = null, CancellationToken cancellationToken = default)
        {
            await _youTube.Videos.Streams.DownloadAsync(streamInfo, 
                filePath, 
                progressCallback != null ? new Progress<double>(progressCallback) : null, 
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns the VideoId for specified URL. You can use this method to avoid parsing the id several times.
        /// </summary>
        /// <param name="url">The video URL to parse.</param>
        /// <returns>The video id.</returns>
        public VideoId GetVideoId(Uri url)
        {
            url.CheckNotNull(nameof(url));
            return new VideoId(url.AbsoluteUri);
        }
    }
}

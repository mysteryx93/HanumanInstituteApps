using System;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Provides YouTube download functions.
    /// </summary>
    public interface IYouTubeDownloader
    {
        /// <summary>
        /// Returns information about specified YouTube video.
        /// </summary>
        /// <param name="id">The id of the video.</param>
        /// <returns>The video info.</returns>
        Task<Video> QueryVideoAsync(VideoId videoId);
        /// <summary>
        /// Returns streams information for specified YouTube video.
        /// </summary>
        /// <param name="id">The id of the video.</param>
        /// <returns>Information about available streams.</returns>
        Task<StreamManifest> QueryStreamInfoAsync(VideoId videoId);
        /// <summary>
        /// Download the actual stream which is identified by the specified metadata to the specified file.
        /// </summary>
        /// <param name="streamInfo">The stream to download.</param>
        /// <param name="filePath">The file path where to save the download.</param>
        /// <param name="progressCallback">A method to call when progress is updated.</param>
        /// <param name="cancellationToken">A cancellation token allowing to cancel the operation.</param>
        Task DownloadAsync(IStreamInfo streamInfo, string filePath, Action<double>? progressCallback = null, CancellationToken cancellationToken = default);
    }
}

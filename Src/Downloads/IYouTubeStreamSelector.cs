using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.Downloads;

/// <summary>
/// Selects the best streams for YouTube downloads.
/// </summary>
public interface IYouTubeStreamSelector
{
    /// <summary>
    /// Analyzes download streams and returns the formats to downloads.
    /// </summary>
    /// <param name="streams">The download streams.</param>
    /// <param name="downloadVideo">Whether to download the video.</param>
    /// <param name="downloadAudio">Whether to download the audio.</param>
    /// <param name="options">The download options.</param>
    /// <returns>The analysis results.</returns>
    StreamQueryInfo SelectStreams(StreamManifest streams, bool downloadVideo, bool downloadAudio, DownloadOptions? options);
}

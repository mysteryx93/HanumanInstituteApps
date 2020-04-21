using System;
using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Selects the best streams for YouTube downloads.
    /// </summary>
    public interface IYouTubeStreamSelector
    {
        /// <summary>
        /// Returns the best format from the list in this order of availability: WebM, Mp4 or Flash.
        /// Mp4 will be chosen if WebM is over 35% smaller.
        /// </summary>
        /// <param name="vstream">The list of video streams to chose from.</param>
        /// <param name="options">Options for stream selection.</param>
        /// <returns>The best format available.</returns>
        BestFormatInfo SelectBestFormat(StreamManifest vstream, DownloadOptions options);
        /// <summary>
        /// Selects Opus audio if available, otherwise Vorbis or AAC.
        /// </summary>
        /// <param name="vinfo">The list of available audio streams.</param>
        /// <param name="options">Options for stream selection.</param>
        /// <returns>The audio to download.</returns>
        IAudioStreamInfo SelectBestAudio(StreamManifest vinfo, DownloadOptions options);
        /// <summary>
        /// Returns the encoding format of specified download stream.
        /// </summary>
        /// <param name="stream">The download stream for which to get the encoding.</param>
        /// <returns>The video encoding format.</returns>
        string GetVideoEncoding(IStreamInfo stream);
        /// <summary>
        /// Returns the height of specified video stream.
        /// </summary>
        /// <param name="stream">The video stream to get information for.</param>
        /// <returns>The video height.</returns>
        int GetVideoHeight(IStreamInfo stream);
        /// <summary>
        /// Returns the frame rate of specified video stream.
        /// </summary>
        /// <param name="stream">The stream to get information for.</param>
        /// <returns>The video frame rate.</returns>
        double GetVideoFrameRate(IStreamInfo stream);
    }
}

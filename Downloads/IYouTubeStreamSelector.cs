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
        /// Selects the best video stream available according to options.
        /// </summary>
        /// <param name="vinfo">The list of available streams.</param>
        /// <param name="options">Options for stream selection.</param>
        /// <returns>The video to download.</returns>
        IVideoStreamInfo? SelectBestVideo(StreamManifest vinfo, DownloadOptions options);
        /// <summary>
        /// Selects the best audio stream available according to options.
        /// </summary>
        /// <param name="vinfo">The list of available streams.</param>
        /// <param name="options">Options for stream selection.</param>
        /// <returns>The audio to download.</returns>
        IAudioStreamInfo? SelectBestAudio(StreamManifest vinfo, DownloadOptions options);
        /// <summary>
        /// Returns the height of specified video stream.
        /// </summary>
        /// <param name="stream">The video stream to get information for.</param>
        /// <returns>The video height.</returns>
        int GetVideoHeight(IStreamInfo stream);
        /// <summary>
        /// Returns the file extension for specified video type.
        /// To avoid conflicting file names, the codec extension must be different than the final extension.
        /// </summary>
        /// <param name="video">The video type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        string GetFinalExtension(IVideoStreamInfo? video, IAudioStreamInfo? audio);
    }
}

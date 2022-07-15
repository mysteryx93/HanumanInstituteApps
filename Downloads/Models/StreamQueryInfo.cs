using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.Downloads;

/// <summary>
/// Contains download stream analysis results. Pass this class to the download method.
/// </summary>
public class StreamQueryInfo
{
    /// <summary>
    /// Gets or sets the video stream info. It may contain both audio and video.
    /// </summary>
    public IVideoStreamInfo? Video { get; set; }
    /// <summary>
    /// Gets or sets the audio stream info. It may contain both audio and video.
    /// </summary>
    public IAudioStreamInfo? Audio { get; set; }

    /// <summary>
    /// Gets or sets whether to download the video.
    /// </summary>
    public bool DownloadVideo { get; set; } = true;
    /// <summary>
    /// Gets or sets whether to download the audio.
    /// </summary>
    public bool DownloadAudio { get; set; } = true;

    /// <summary>
    /// Gets the output video stream info.
    /// </summary>
    public IVideoStreamInfo? OutputVideo => DownloadVideo ? Video ?? Audio as IVideoStreamInfo : null;
    /// <summary>
    /// Gets the output audio stream info.
    /// </summary>
    public IAudioStreamInfo? OutputAudio => DownloadAudio ? Audio ?? Video as IAudioStreamInfo : null;

    /// <summary>
    /// Gets or sets the file extension to save the media file as.
    /// </summary>
    public string FileExtension { get; set; } = string.Empty;
}
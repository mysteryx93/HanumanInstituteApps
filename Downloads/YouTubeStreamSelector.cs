using HanumanInstitute.BassAudio;
using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.Downloads;

/// <inheritdoc />
public sealed class YouTubeStreamSelector : IYouTubeStreamSelector
{
    /// <inheritdoc />
    public StreamQueryInfo SelectStreams(StreamManifest streams, bool downloadVideo, bool downloadAudio, DownloadOptions? options)
    {
        streams.CheckNotNull(nameof(streams));
        options ??= new DownloadOptions();

        var result = new StreamQueryInfo()
        {
            DownloadVideo = downloadVideo,
            DownloadAudio = downloadAudio
        };

        var isMuxed = false;
        if (downloadVideo)
        {
            result.Video = SelectBestVideo(streams, options);
            isMuxed = result.Video is MuxedStreamInfo;
        }
        if (downloadAudio && !isMuxed)
        {
            result.Audio = SelectBestAudio(streams, options);
        }

        result.EncodeAudio = options.EncodeAudio?.Clone();
        result.FileExtension = GetFinalExtension(result.OutputVideo, result.OutputAudio, result.EncodeAudio?.Format);

        return result;
    }

    /// <summary>
    /// Selects the best video stream available according to options.
    /// </summary>
    /// <param name="vInfo">The list of available streams.</param>
    /// <param name="options">Options for stream selection.</param>
    /// <returns>The video to download.</returns>
    private static IVideoStreamInfo? SelectBestVideo(StreamManifest vInfo, DownloadOptions options)
    {
        vInfo.CheckNotNull(nameof(vInfo));
        options.CheckNotNull(nameof(options));

        if (options.PreferredVideo == StreamContainerOption.None)
        {
            return null;
        }

        var orderedList = (from v in vInfo.GetVideoStreams()
            where (options.MaxQuality == 0 || GetVideoHeight(v) <= options.MaxQuality) &&
                options.PreferredVideo == StreamContainerOption.Best || ContainerEquals(options.PreferredVideo, v)
            orderby GetVideoHeight(v) descending,
                v?.VideoQuality.Framerate ?? 0 descending
            select v).ToList();

        if (!orderedList.Any())
        {
            return null;
        }

        var maxRes = GetVideoHeight(orderedList.First());
        var maxResList = orderedList.Where(v => GetVideoHeight(v) == maxRes).ToList();

        return (from v in maxResList
            // WebM VP9 encodes ~30% better. VP8 isn't better than MP4.
            let preference = (int)(v.VideoCodec == VideoEncoding.Vp9 ? v.Size.Bytes * 1.3 : v.Size.Bytes)
            orderby preference descending
            select v).FirstOrDefault();
    }

    /// <summary>
    /// Selects the best audio stream available according to options.
    /// </summary>
    /// <param name="vInfo">The list of available streams.</param>
    /// <param name="options">Options for stream selection.</param>
    /// <returns>The audio to download.</returns>
    private static IAudioStreamInfo? SelectBestAudio(StreamManifest vInfo, DownloadOptions options)
    {
        vInfo.CheckNotNull(nameof(vInfo));
        options.CheckNotNull(nameof(options));

        if (!vInfo.Streams.Any() || options.PreferredAudio == StreamContainerOption.None)
        {
            return null;
        }

        return (from v in vInfo.GetAudioOnlyStreams()
                   // Opus encodes ~20% better, Vorbis ~10% better than AAC
                   let preference = (int)(v.Size.Bytes *
                                          (v.AudioCodec == AudioEncoding.Opus ? 1.2 : v.AudioCodec == AudioEncoding.Vorbis ? 1.1 : 1))
                   where options.PreferredAudio == StreamContainerOption.Best || ContainerEquals(options.PreferredAudio, v)
                   orderby preference descending
                   select v).FirstOrDefault()
               ??
               (from v in vInfo.GetAudioStreams()
                   where v is MuxedStreamInfo
                   where options.PreferredAudio == StreamContainerOption.Best || ContainerEquals(options.PreferredAudio, v)
                   orderby v.Size.Bytes descending
                   select v).FirstOrDefault();
    }

    private static bool ContainerEquals(StreamContainerOption option, IStreamInfo streamInfo)
    {
        return string.Equals(option.ToString(), streamInfo.Container.Name, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Returns the height of specified video stream.
    /// </summary>
    /// <param name="stream">The video stream to get information for.</param>
    /// <returns>The video height.</returns>
    private static int GetVideoHeight(IStreamInfo stream)
    {
        var vInfo = stream as IVideoStreamInfo;
        if (vInfo != null)
        {
            return vInfo.VideoResolution.Height;
        }
        else if (stream is MuxedStreamInfo mInfo)
        {
            return vInfo?.VideoResolution.Height ?? mInfo.VideoQuality.MaxHeight;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Returns the file extension for specified video type.
    /// To avoid conflicting file names, the codec extension must be different than the final extension.
    /// Optional EncodeAudio format is taken into account. 
    /// </summary>
    /// <param name="video">The video type to get file extension for.</param>
    /// <param name="audio">The audio type to get file extension for.</param>
    /// <param name="encodeAudio">If re-encoding the audio, the format being re-encoded to.</param>
    /// <returns>The file extension.</returns>
    private static string GetFinalExtension(IVideoStreamInfo? video, IAudioStreamInfo? audio, EncodeFormat? encodeAudio)
    {
        if (video != null && audio == null)
        {
            // Video-only
            return 1 switch
            {
                _ when video.Container == Container.WebM => "webm",
                _ when video.Container == Container.Mp4 => "mp4",
                _ => "mkv"
            };
        }
        else if (audio != null && video == null)
        {
            // Audio-only
            return 1 switch
            {
                _ when encodeAudio == EncodeFormat.Aac => "m4a",
                _ when encodeAudio != null => encodeAudio.ToStringInvariant().ToLowerInvariant(),
                _ when audio.Container == Container.Mp4 => "m4a",
                _ when audio.AudioCodec == AudioEncoding.Opus => "opus",
                _ when audio.AudioCodec == AudioEncoding.Vorbis => "ogg",
                _ => "mkv"
            };
        }
        else if (audio != null && video != null)
        {
            // Both
            return 1 switch
            {
                _ when video.Container == Container.WebM && (encodeAudio == null ?
                    audio.Container == Container.WebM :
                    encodeAudio is EncodeFormat.Ogg or EncodeFormat.Opus) => "webm",
                _ when video.Container == Container.Mp4 && (encodeAudio == null ?
                    audio.Container == Container.Mp4 :
                    encodeAudio is EncodeFormat.Aac or EncodeFormat.Mp3 or EncodeFormat.Flac or EncodeFormat.Opus) => "mp4",
                _ => "mkv"
            };
        }
        return "mkv";
    }

    private static class VideoEncoding
    {
        public const string Vp9 = "vp9";
        //public const string Vp8 = "vp8";
    }

    private static class AudioEncoding
    {
        public const string Opus = "opus";
        public const string Vorbis = "vorbis";
    }
}

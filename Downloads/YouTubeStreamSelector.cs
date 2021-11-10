using System;
using System.Linq;
using HanumanInstitute.CommonServices;
using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Selects the best streams for YouTube downloads.
    /// </summary>
    public class YouTubeStreamSelector : IYouTubeStreamSelector
    {
        public YouTubeStreamSelector() { }

        /// <summary>
        /// Analyzes download streams and returns the formats to downloads.
        /// </summary>
        /// <param name="streams">The download streams.</param>
        /// <param name="downloadVideo">Whether to download the video.</param>
        /// <param name="downloadAudio">Whether to download the audio.</param>
        /// <param name="options">The download options.</param>
        /// <returns>The analysis results.</returns>
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

            result.FileExtension = GetFinalExtension(result.OutputVideo, result.OutputAudio);

            return result;
        }

        /// <summary>
        /// Selects the best video stream available according to options.
        /// </summary>
        /// <param name="vinfo">The list of available streams.</param>
        /// <param name="options">Options for stream selection.</param>
        /// <returns>The video to download.</returns>
        private static IVideoStreamInfo? SelectBestVideo(StreamManifest vinfo, DownloadOptions options)
        {
            vinfo.CheckNotNull(nameof(vinfo));
            options.CheckNotNull(nameof(options));

            if (options.PreferredVideo == StreamContainerOption.None)
            {
                return null;
            }

            var orderedList = (from v in vinfo.GetVideoStreams()
                               where (options.MaxQuality == 0 || GetVideoHeight(v) <= options.MaxQuality) &&
                                    options.PreferredVideo == StreamContainerOption.Best || ContainerEquals(options.PreferredVideo, v)
                               orderby GetVideoHeight(v) descending
                               orderby (v as IVideoStreamInfo)?.VideoQuality.Framerate ?? 0 descending
                               select v).ToList();

            if (!orderedList.Any())
            {
                return null;
            }

            var maxRes = GetVideoHeight(orderedList.First());
            var maxResList = orderedList.Where(v => GetVideoHeight(v) == maxRes).ToList();

            return (from v in maxResList
                        // WebM VP9 encodes ~30% better. VP8 isn't better than MP4.
                    let Preference = (int)(v.VideoCodec == VideoEncoding.Vp9 ? v.Size.Bytes * 1.3 : v.Size.Bytes)
                    orderby Preference descending
                    select v).FirstOrDefault();
        }

        /// <summary>
        /// Selects the best audio stream available according to options.
        /// </summary>
        /// <param name="vinfo">The list of available streams.</param>
        /// <param name="options">Options for stream selection.</param>
        /// <returns>The audio to download.</returns>
        private static IAudioStreamInfo? SelectBestAudio(StreamManifest vinfo, DownloadOptions options)
        {
            vinfo.CheckNotNull(nameof(vinfo));
            options.CheckNotNull(nameof(options));

            if (!vinfo.Streams.Any() || options.PreferredAudio == StreamContainerOption.None)
            {
                return null;
            }

            return (from v in vinfo.GetAudioOnlyStreams()
                        // Opus encodes ~20% better, Vorbis ~10% better than AAC
                    let Preference = (int)(v.Size.Bytes * (v.AudioCodec == AudioEncoding.Opus ? 1.2 : v.AudioCodec == AudioEncoding.Vorbis ? 1.1 : 1))
                    where options.PreferredAudio == StreamContainerOption.Best || ContainerEquals(options.PreferredAudio, v)
                    orderby Preference descending
                    select v).FirstOrDefault()
                    ??
                    (from v in vinfo.GetAudioStreams()
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
                //var q = vInfo?.VideoQuality ?? mInfo.VideoQuality;
                //if (q == VideoQuality.High4320)
                //{
                //    return 4320;
                //}
                //else if (q == VideoQuality.High3072)
                //{
                //    return 3072;
                //}
                //else if (q == VideoQuality.High2160)
                //{
                //    return 2160;
                //}
                //else if (q == VideoQuality.High1440)
                //{
                //    return 1440;
                //}
                //else if (q == VideoQuality.High1080)
                //{
                //    return 1080;
                //}
                //else if (q == VideoQuality.High720)
                //{
                //    return 720;
                //}
                //else if (q == VideoQuality.Medium480)
                //{
                //    return 480;
                //}
                //else if (q == VideoQuality.Medium360)
                //{
                //    return 360;
                //}
                //else if (q == VideoQuality.Low240)
                //{
                //    return 240;
                //}
                //else if (q == VideoQuality.Low144)
                //{
                //    return 144;
                //}
                //else
                //{
                //    return 0;
                //}
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the file extension for specified video type.
        /// To avoid conflicting file names, the codec extension must be different than the final extension.
        /// </summary>
        /// <param name="video">The video type to get file extension for.</param>
        /// <returns>The file extension.</returns>
        private static string GetFinalExtension(IVideoStreamInfo? video, IAudioStreamInfo? audio)
        {
            if (video != null && audio == null)
            {
                // Video-only
                if (video.Container == Container.WebM)
                {
                    return "webm";
                }
                else if (video.Container == Container.Mp4)
                {
                    return "mp4";
                }
                else if (video.Container == Container.Tgpp)
                {
                    return "mp4"; // ?
                }
            }
            else if (audio != null && video == null)
            {
                // Audio-only
                if (audio.Container == Container.Mp4)
                {
                    return "m4a";
                }
                else if (audio.Container == Container.Tgpp)
                {
                    return "mp4v"; // ?
                }
                else if (audio.AudioCodec == AudioEncoding.Opus)
                {
                    return "opus";
                }
                else if (audio.AudioCodec == AudioEncoding.Vorbis)
                {
                    return "ogg";
                }
            }
            else if (audio != null && video != null)
            {
                // Both
                if (video.Container == Container.WebM && audio.Container == Container.WebM)
                {
                    return "webm";
                }
                else if (video.Container == Container.Mp4 && audio.Container == Container.Mp4)
                {
                    return "mp4";
                }
                else if (video.Container == Container.Tgpp && audio.Container == Container.Tgpp)
                {
                    return "mp4"; // ?
                }
                else
                {
                    return "mkv";
                }
            }
            return "mkv";
        }

        private static class VideoEncoding
        {
            public const string Vp9 = "vp9";
            public const string Vp8 = "vp8";
        }

        private static class AudioEncoding
        {
            public const string Opus = "opus";
            public const string Vorbis = "vorbis";
        }
    }
}

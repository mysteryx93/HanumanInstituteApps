using System;
using System.Linq;
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
        /// Returns the best format from the list in this order of availability: WebM, Mp4 or Flash.
        /// Mp4 will be chosen if WebM is over 35% smaller.
        /// </summary>
        /// <param name="vstream">The list of video streams to chose from.</param>
        /// <param name="options">Options for stream selection.</param>
        /// <returns>The best format available.</returns>
        public BestFormatInfo? SelectBestFormat(StreamManifest vstream, DownloadOptions options)
        {
            vstream.CheckNotNull(nameof(vstream));

            var maxResolutionList = (from v in vstream.GetAudio().Cast<IStreamInfo>().Union(vstream.GetVideo()).Union(vstream.GetMuxed())
                                     where (options.MaxQuality == 0 || GetVideoHeight(v) <= options.MaxQuality)
                                     orderby GetVideoHeight(v) descending
                                     orderby GetVideoFrameRate(v) descending
                                     select v).ToList();

            maxResolutionList = maxResolutionList.Where(v => GetVideoHeight(v) == GetVideoHeight(maxResolutionList.First())).ToList();

            var bestVideo = (from v in maxResolutionList
                                 // WebM VP9 encodes ~30% better. non-DASH is VP8 and isn't better than MP4.
                             let Preference = (int)(GetVideoEncoding(v) == VideoEncoding.Vp9 ? v.Size.TotalBytes * 1.3 : v.Size.TotalBytes)
                             where options.PreferredFormat == SelectStreamFormat.Best ||
                                (options.PreferredFormat == SelectStreamFormat.MP4 && GetVideoEncoding(v) == VideoEncoding.H264) ||
                                (options.PreferredFormat == SelectStreamFormat.VP9 && GetVideoEncoding(v) == VideoEncoding.Vp9)
                             orderby Preference descending
                             select v).FirstOrDefault();

            bestVideo ??= maxResolutionList.FirstOrDefault();

            if (bestVideo != null)
            {
                var result = new BestFormatInfo
                {
                    BestVideo = bestVideo as IVideoStreamInfo,
                    BestAudio = SelectBestAudio(vstream, options)
                };
                return result;
            }
            return null;
        }

        /// <summary>
        /// Selects Opus audio if available, otherwise Vorbis or AAC.
        /// </summary>
        /// <param name="vinfo">The list of available audio streams.</param>
        /// <param name="options">Options for stream selection.</param>
        /// <returns>The audio to download.</returns>
        public IAudioStreamInfo? SelectBestAudio(StreamManifest vinfo, DownloadOptions options)
        {
            if (vinfo == null || !vinfo.GetAudio().Any())
            {
                return null;
            }

            var bestAudio = (from v in vinfo.GetAudio()
                                 // Opus encodes ~20% better, Vorbis ~10% better than AAC
                             let Preference = (int)(v.Size.TotalBytes * (v.AudioCodec == AudioEncoding.Opus ? 1.2 : v.AudioCodec == AudioEncoding.Vorbis ? 1.1 : 1))
                             where options.PreferredAudio == SelectStreamFormat.Best ||
                             (options.PreferredAudio == SelectStreamFormat.MP4 && (v.AudioCodec == AudioEncoding.Aac)) ||
                             (options.PreferredAudio == SelectStreamFormat.VP9 && (v.AudioCodec == AudioEncoding.Opus || v.AudioCodec == AudioEncoding.Vorbis))
                             orderby Preference descending
                             select v).FirstOrDefault();
            return bestAudio;
        }

        /// <summary>
        /// Returns the encoding format of specified download stream.
        /// </summary>
        /// <param name="stream">The download stream for which to get the encoding.</param>
        /// <returns>The video encoding format.</returns>
        public string GetVideoEncoding(IStreamInfo stream)
        {
            var vInfo = stream as IVideoStreamInfo;
            var mInfo = stream as MuxedStreamInfo;
            if (vInfo == null && mInfo == null)
            {
                return VideoEncoding.H264;
            }

            return vInfo?.VideoCodec ?? mInfo?.VideoCodec ?? string.Empty;
        }

        /// <summary>
        /// Returns the height of specified video stream.
        /// </summary>
        /// <param name="stream">The video stream to get information for.</param>
        /// <returns>The video height.</returns>
        public int GetVideoHeight(IStreamInfo stream)
        {
            var vInfo = stream as IVideoStreamInfo;
            if (vInfo != null)
            {
                return vInfo.Resolution.Height;
            }
            else if (stream is MuxedStreamInfo mInfo)
            {
                var q = vInfo?.VideoQuality ?? mInfo.VideoQuality;
                if (q == VideoQuality.High4320)
                {
                    return 4320;
                }
                else if (q == VideoQuality.High3072)
                {
                    return 3072;
                }
                else if (q == VideoQuality.High2160)
                {
                    return 2160;
                }
                else if (q == VideoQuality.High1440)
                {
                    return 1440;
                }
                else if (q == VideoQuality.High1080)
                {
                    return 1080;
                }
                else if (q == VideoQuality.High720)
                {
                    return 720;
                }
                else if (q == VideoQuality.Medium480)
                {
                    return 480;
                }
                else if (q == VideoQuality.Medium360)
                {
                    return 360;
                }
                else if (q == VideoQuality.Low240)
                {
                    return 240;
                }
                else if (q == VideoQuality.Low144)
                {
                    return 144;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the frame rate of specified video stream.
        /// </summary>
        /// <param name="stream">The stream to get information for.</param>
        /// <returns>The video frame rate.</returns>
        public double GetVideoFrameRate(IStreamInfo stream)
        {
            var vInfo = stream as IVideoStreamInfo;
            return vInfo?.Framerate.FramesPerSecond ?? 0;
        }

        private static class VideoEncoding
        {
            public const string H264 = "H264";
            public const string Vp9 = "Vp9";
            public const string Vp8 = "Vp8";
        }

        private static class AudioEncoding
        {
            public const string Aac = "Aac";
            public const string Opus = "Opus";
            public const string Vorbis = "Vorbis";
        }
    }
}

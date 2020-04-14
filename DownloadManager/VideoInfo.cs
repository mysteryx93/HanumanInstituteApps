using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.DownloadManager {
    public class VideoInfo {
        public Video Info { get; set; }
        public StreamManifest Streams { get; set; }

        public VideoInfo() {
        }

        public VideoInfo(Video info, StreamManifest streams) {
            Info = info;
            Streams = streams;
        }
    }
}

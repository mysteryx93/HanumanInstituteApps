
namespace EmergenceGuardian.Downloader {
    public class FileProgress {
        public StreamType Type { get; set; }
        public string Url { get; set; }
        public string Destination { get; set; }
        public object Stream { get; set; }
        public long BytesTotal { get; set; }
        public long BytesDownloaded { get; set; }
        public bool Done { get; set; }
    }
}

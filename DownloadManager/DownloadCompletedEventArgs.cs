
namespace EmergenceGuardian.Downloader {
    public class DownloadCompletedEventArgs {
        public DownloadCompletedEventArgs() {
        }

        public DownloadCompletedEventArgs(DownloadItem downloadInfo) {
            this.DownloadInfo = downloadInfo;
        }

        public DownloadItem DownloadInfo { get; set; }
    }
}

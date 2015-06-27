using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using YoutubeExtractor;

namespace Business {
    [PropertyChanged.ImplementPropertyChanged]
    public class DownloadItem {
        public DownloadItem() {
            this.Files = new List<FileProgress>();
        }

        public DownloadItem(Media request, string destination, string title, int queuePos, bool upgradeAudio, EventHandler<DownloadCompletedEventArgs> callback) {
            this.Status = DownloadStatus.Waiting;
            this.Request = request;
            this.Destination = destination;
            this.Title = title;
            this.Status = DownloadStatus.Waiting;
            UpdateProgress();
            this.QueuePos = queuePos;
            this.UpgradeAudio = upgradeAudio;
            this.Callback = callback;
            this.Files = new List<FileProgress>();
        }

        public Media Request { get; set; }
        public string Destination { get; set; }
        public string Title { get; set; }
        public double ProgressValue { get; set; }
        public List<FileProgress> Files { get; set; }
        public EventHandler<DownloadCompletedEventArgs> Callback { get; set; }
        /// <summary>
        /// Indicate the position in the playlist for autoplay, or -1 to disable playback after complete.
        /// </summary>
        public int QueuePos { get; set; }
        private DownloadStatus status;
        public string Progress { get; set; }
        public bool UpgradeAudio { get; set; }

        public DownloadStatus Status {
            get {
                return status;
            }
            set {
                status = value;
                UpdateProgress();
            }
        }

        public void UpdateProgress() {
            switch (status) {
                case DownloadStatus.Waiting:
                    Progress = "Waiting...";
                    break;
                case DownloadStatus.Initializing:
                    Progress = "Initializing...";
                    break;
                case DownloadStatus.Done:
                    Progress = "Done";
                    break;
                case DownloadStatus.Canceled:
                    Progress = "Canceled";
                    break;
                case DownloadStatus.Failed:
                    Progress = "Failed";
                    break;
                case DownloadStatus.Downloading:
                    int TotalBytes = 0;
                    int Downloaded = 0;
                    bool BytesTotalLoaded = true;
                    foreach (FileProgress item in Files) {
                        if (item.BytesTotal > 0)
                            TotalBytes += item.BytesTotal;
                        else
                            BytesTotalLoaded = false;
                        Downloaded += item.BytesDownloaded;
                    }
                    if (BytesTotalLoaded) {
                        ProgressValue = ((double)Downloaded / TotalBytes) * 100;
                        Progress = Math.Round(ProgressValue, 1).ToString() + "%";
                    }
                    break;
            }
        }

        /// <summary>
        /// Returns whether all files are finished downloading.
        /// </summary>
        public bool IsCompleted {
            get {
                if (Files != null && Files.Count > 0) {
                    bool Result = true;
                    foreach (FileProgress item in Files) {
                        if (item.BytesTotal == 0 || item.BytesDownloaded < item.BytesTotal)
                            Result = false;
                    }
                    return Result;
                } else 
                    return false;
            }
        }

        /// <summary>
        /// Returns whether the download was canceled or failed.
        /// </summary>
        public bool IsCanceled {
            get{
                return (status == DownloadStatus.Canceled || status == DownloadStatus.Failed); 
            }
        }
        
        public class FileProgress {
            public VideoInfo Source { get; set; }
            public string Destination { get; set; }
            public int BytesTotal { get; set; }
            public int BytesDownloaded { get; set; }
            public bool Done { get; set; }
        }
    }
}

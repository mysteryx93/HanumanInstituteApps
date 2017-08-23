using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using YoutubeExplode.Models.MediaStreams;

namespace EmergenceGuardian.Downloader {
    [AddINotifyPropertyChangedInterface]
    public class DownloadItem {
        public DownloadItem() {
            this.Files = new List<FileProgress>();
        }

        public DownloadItem(string url, string destination, string destinationNoExt, string title, DownloadAction action, EventHandler<DownloadCompletedEventArgs> callback, DownloadOptions options, object data) {
            this.Url = url;
            this.Status = DownloadStatus.Waiting;
            this.Destination = destination;
            this.DestinationNoExt = destinationNoExt;
            this.Title = title;
            this.Status = DownloadStatus.Waiting;
            UpdateProgress();
            this.Action = action;
            this.Callback = callback;
            this.Options = options;
            this.Data = data;
            this.Files = new List<FileProgress>();
        }

        public string Url { get; set; }
        public string Destination { get; set; }
        public string DestinationNoExt { get; set; }
        public string Title { get; set; }
        public double ProgressValue { get; set; }
        public List<FileProgress> Files { get; set; }
        public EventHandler<DownloadCompletedEventArgs> Callback { get; set; }
        private DownloadStatus status;
        public string Progress { get; set; }
        public DownloadAction Action { get; set; }
        public DownloadOptions Options { get; set; }
        public object Data { get; set; }

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
                    long TotalBytes = 0;
                    long Downloaded = 0;
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
    }

    public enum StreamType {
        Video,
        Audio
    }
}

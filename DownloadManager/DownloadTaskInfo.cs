using System;
using System.Collections.Generic;
using System.ComponentModel;
using PropertyChanged;

namespace HanumanInstitute.DownloadManager {
    /// <summary>
    /// Represents a media file download task, which can consist of multiple download streams.
    /// This class is responsible to store and manage the task status. All file and stream interactions are done in DownloadManager.
    /// </summary>
    [AddINotifyPropertyChangedInterface()]
    public class DownloadTaskInfo {
        public DownloadTaskInfo() { }

        public DownloadTaskInfo(string url, string destination, string title, bool downloadVideo, bool downloadAudio, DownloadTaskEventHandler callback, DownloadOptions options) {
            this.Url = url;
            this.Destination = destination;
            this.Title = title;
            this.DownloadVideo = downloadVideo;
            this.DownloadAudio = downloadAudio;
            this.Callback = callback;
            this.Options = options;
            UpdateProgress();
        }

        /// <summary>
        /// Gets or sets the list of file streams being downloaded.
        /// </summary>
        public List<DownloadFileInfo> Files { get; set; } = new List<DownloadFileInfo>();
        /// <summary>
        /// Gets or sets the URL to download from.
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Gets or sets the destination path to store the file locally.
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// Gets or sets the title of the downloaded media.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Gets or sets whether to download the video stream.
        /// </summary>
        public bool DownloadVideo { get; set; }
        /// <summary>
        /// Gets or sets whether to download the audio stream.
        /// </summary>
        public bool DownloadAudio { get; set; }
        /// <summary>
        /// Gets or sets the function to be called once download is completed.
        /// </summary>
        public DownloadTaskEventHandler Callback { get; set; }
        /// <summary>
        /// Gets or sets the download options.
        /// </summary>
        public DownloadOptions Options { get; set; }
        /// <summary>
        /// Gets or sets the progress of all streams as percentage.
        /// </summary>
        public double ProgressValue { get; private set; }
        /// <summary>
        /// Gets or sets the progress of all streams as a string representation.
        /// </summary>
        public string Progress { get; private set; }
        private DownloadStatus status = DownloadStatus.Waiting;

        /// <summary>
        /// Gets or sets the download status.
        /// </summary>
        public DownloadStatus Status {
            get => status;
            set {
                status = value;
                UpdateProgress();
            }
        }

        /// <summary>
        /// Indicates that more data has been downloaded and updates Progress and ProgressValue.
        /// </summary>
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
                    foreach (DownloadFileInfo item in Files) {
                        if (item.Length > 0)
                            TotalBytes += item.Length;
                        else
                            BytesTotalLoaded = false;
                        Downloaded += item.Downloaded;
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
                    foreach (DownloadFileInfo item in Files) {
                        if (item.Length == 0 || item.Downloaded < item.Length)
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
        public bool IsCancelled => (status == DownloadStatus.Canceled || status == DownloadStatus.Failed);
    }
}

using System;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Represents the method that will handle a download task event.
    /// </summary>
    /// <param name="e">Information about the download task.</param>
    public delegate void DownloadTaskEventHandler(object sender, DownloadTaskEventArgs e);

    /// <summary>
    /// Contains data for download task event.
    /// </summary>
    public class DownloadTaskEventArgs : EventArgs
    {
        public DownloadTaskEventArgs() { }

        public DownloadTaskEventArgs(DownloadTaskInfo downloadInfo)
        {
            DownloadTask = downloadInfo;
        }

        /// <summary>
        /// Returns information about the file being downloaded.
        /// </summary>
        public DownloadTaskInfo DownloadTask { get; set; }
    }
}

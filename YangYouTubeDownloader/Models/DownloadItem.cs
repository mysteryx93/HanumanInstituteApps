using System;
using HanumanInstitute.Downloads;

namespace HanumanInstitute.YangYouTubeDownloader.Models
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class DownloadItem
    {
        public DownloadItem(IDownloadTask download, string title)
        {
            Title = title;
            Download = download.CheckNotNull(nameof(Download));
            Download.ProgressUpdated += (s, e) =>
            {
                Progress = Download.ProgressText;
            };
        }

        public IDownloadTask Download { get; }
        public string Title { get; set; } = string.Empty;
        public string Progress { get; set; } = string.Empty;
    }
}

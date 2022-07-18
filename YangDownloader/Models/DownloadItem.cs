using HanumanInstitute.Downloads;
using ReactiveUI;

namespace HanumanInstitute.YangDownloader.Models
{
    public class DownloadItem : ReactiveObject
    {
        public DownloadItem(IDownloadTask download, string title)
        {
            Title = title;
            Download = download.CheckNotNull(nameof(Download));
            Download.ProgressUpdated += (_, _) =>
            {
                Progress = Download.ProgressText;
            };
        }

        public IDownloadTask Download { get; }
        
        [Reactive]
        public string Title { get; set; }

        [Reactive]
        public string Progress { get; set; } = string.Empty;
    }
}

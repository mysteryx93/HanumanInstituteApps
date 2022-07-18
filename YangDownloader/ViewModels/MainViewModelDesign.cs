using HanumanInstitute.Downloads;

namespace HanumanInstitute.YangDownloader.ViewModels
{
    public class MainViewModelDesign : MainViewModel
    {
        public MainViewModelDesign(IDownloadManager downloadManager, IYouTubeStreamSelector streamSelector, IDialogService dialogService,
            IFileSystemService fileSystem) : base(downloadManager, streamSelector, dialogService, fileSystem)
        {
            DisplayDownloadInfo = true;
        }
    }
}

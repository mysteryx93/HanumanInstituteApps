using HanumanInstitute.Downloads;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Splat;

namespace HanumanInstitute.YangDownloader.ViewModels;

public class MainViewModelDesign : MainViewModel
{
    public MainViewModelDesign() : base(Locator.Current.GetService<IDownloadManager>()!, new YouTubeStreamSelector(),
        new DialogService(), Locator.Current.GetService<IFileSystemService>()!, Locator.Current.GetService<ISettingsProvider<AppSettingsData>>()!)
    {
        DisplayDownloadInfo = true;
        VideoTitle = "This is a very long title! This is a very long title! This is a very long title! ";
        VideoStreamInfo = "vp9";
        AudioStreamInfo = "opus";
        ErrorMessage = "Error";
    }
}

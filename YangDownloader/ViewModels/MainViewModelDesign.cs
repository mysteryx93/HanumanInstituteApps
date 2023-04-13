using HanumanInstitute.Apps;
using HanumanInstitute.Downloads;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Splat;

namespace HanumanInstitute.YangDownloader.ViewModels;

public class MainViewModelDesign : MainViewModel
{
    public MainViewModelDesign() : base(Locator.Current.GetService<ISettingsProvider<AppSettingsData>>()!,
        Locator.Current.GetService<IAppUpdateService>()!,
        Locator.Current.GetService<IDownloadManager>()!, new YouTubeStreamSelector(null),
        new DialogService(), Locator.Current.GetService<IFileSystemService>()!, null, Application.Current!.Clipboard!, new EnvironmentService())
    {
        DisplayDownloadInfo = true;
        VideoTitle = "This is a very long title! This is a very long title! This is a very long title! ";
        VideoStreamInfo = "vp9";
        AudioStreamInfo = "opus";
        ErrorMessage = "Error";
    }

    public override void OnLoaded()
    {
    }
}

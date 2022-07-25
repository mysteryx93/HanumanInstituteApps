using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.YangDownloader.ViewModels;

public sealed class AboutViewModel : AboutViewModel<AppSettingsData>
{
    public AboutViewModel(IEnvironmentService environment, IProcessService processService,
        ISettingsProvider<AppSettingsData> settings, IUpdateService updateService) :
        base(environment, processService, settings, updateService)
    {
    }

    public override string UpdateFileFormat => "YangDownloader-{0}_Win_x64.zip";

    public override string AppName => "Yang YouTube Downloader";

    public override string AppDescription => "Downloads best-quality audio and video from YouTube.";
}

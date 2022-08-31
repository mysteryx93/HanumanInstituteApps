using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.Player432hz.ViewModels;

public sealed class AboutViewModel : AboutViewModel<AppSettingsData>
{
    public AboutViewModel(IAppInfo appInfo, IEnvironmentService environment, ISettingsProvider<AppSettingsData> settings, IUpdateService updateService) :
        base(appInfo, environment, settings, updateService)
    {
    }
    //
    // public override string UpdateFileFormat => "Player432hz-{0}_Win_x64.zip";
    //
    // public override string AppName => "432hz Player";
    //
    // public override string AppDescription => "Plays music in 432hz";
}

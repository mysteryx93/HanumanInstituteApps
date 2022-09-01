using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.PowerliminalsPlayer.ViewModels;

public sealed class AboutViewModel : AboutViewModel<AppSettingsData>
{
    public AboutViewModel(IAppInfo appInfo, IEnvironmentService environment,
        ISettingsProvider<AppSettingsData> settings, IUpdateService updateService) :
        base(appInfo, environment, settings, updateService)
    {
    }
}

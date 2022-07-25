using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.PowerliminalsPlayer.ViewModels;

public sealed class AboutViewModel : AboutViewModel<AppSettingsData>
{
    public AboutViewModel(IEnvironmentService environment, IProcessService processService,
        ISettingsProvider<AppSettingsData> settings, IUpdateService updateService) :
        base(environment, processService, settings, updateService)
    {
    }

    public override string UpdateFileFormat => "PowerliminalsPlayer-{0}_Win_x64.zip";

    public override string AppName => "Powerliminals Player";

    public override string AppDescription => "Plays multiple audios simultaneously at varying speeds";
}

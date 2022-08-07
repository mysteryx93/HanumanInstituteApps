using FluentAvalonia.Styling;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.Common.Services.Validation;

namespace HanumanInstitute.Player432hz.ViewModels;

public class SettingsViewModel : SettingsViewModel<AppSettingsData>
{
    public SettingsViewModel(ISettingsProvider<AppSettingsData> settingsProvider, IFluentAvaloniaTheme fluentTheme) :
        base(settingsProvider, fluentTheme)
    {
    }

    protected override bool Validate() => Settings.Validate() == null;

    protected override void RestoreDefaultImpl()
    {
        Settings.AntiAlias = false;
        Settings.AntiAliasLength = 32;
        Settings.Speed = 1;
        Settings.AutoDetectPitch = true;
        Settings.PitchFrom = 440;
        Settings.PitchTo = 432;
    }
}

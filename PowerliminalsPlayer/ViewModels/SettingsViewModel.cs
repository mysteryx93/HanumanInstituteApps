using FluentAvalonia.Styling;
using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.PowerliminalsPlayer.ViewModels;

/// <inheritdoc />
public class SettingsViewModel : SettingsViewModel<AppSettingsData>
{
    /// <summary>
    /// Initializes a new instance of the SettingsViewModel class.
    /// </summary>
    public SettingsViewModel(ISettingsProvider<AppSettingsData> settingsProvider, IFluentAvaloniaTheme fluentTheme) :
        base(settingsProvider, fluentTheme)
    {
    }

    /// <inheritdoc />
    protected override void RestoreDefaultImpl()
    {
        Settings.AntiAlias = true;
        Settings.AntiAliasLength = 32;
        Settings.PerformanceQuality = 2;
    }
}

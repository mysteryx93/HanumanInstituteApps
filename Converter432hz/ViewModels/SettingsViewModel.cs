using FluentAvalonia.Styling;
using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.Converter432hz.ViewModels;

/// <inheritdoc />
public class SettingsViewModel : SettingsViewModel<AppSettingsData>
{
    /// <summary>
    /// Initializes a new instance of the SettingsViewModel class.
    /// </summary>
    public SettingsViewModel(ISettingsProvider<AppSettingsData> settingsProvider, FluentAvaloniaTheme fluentTheme) :
        base(settingsProvider, fluentTheme)
    {
    }

    /// <inheritdoc />
    protected override void RestoreDefaultImpl()
    {
    }
}

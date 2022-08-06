using FluentAvalonia.Styling;
using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.YangDownloader.ViewModels;

public class SettingsViewModel : SettingsViewModel<AppSettingsData>
{
    public SettingsViewModel(ISettingsProvider<AppSettingsData> settingsProvider, FluentAvaloniaTheme fluentTheme) :
        base(settingsProvider, fluentTheme)
    {
    }

    protected override void RestoreDefaultImpl()
    {
    }
}

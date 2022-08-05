using FluentAvalonia.Styling;
using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.Player432hz.ViewModels;

public class SettingsViewModel : SettingsViewModel<AppSettingsData>
{
    private readonly IPlaylistPlayer _playlistPlayer;

    public SettingsViewModel(ISettingsProvider<AppSettingsData> settingsProvider, FluentAvaloniaTheme fluentTheme,
        ISerializationService serialization, IPlaylistPlayer playlistPlayer) :
        base(settingsProvider, fluentTheme)
    {
        _playlistPlayer = playlistPlayer;
    }

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

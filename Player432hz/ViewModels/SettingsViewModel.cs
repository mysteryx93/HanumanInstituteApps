using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.Common.Services.Validation;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using ReactiveUI;

namespace HanumanInstitute.Player432hz.ViewModels;

/// <inheritdoc />
public class SettingsViewModel : SettingsViewModel<AppSettingsData>
{
    private readonly IPlaylistPlayer _player;
    public IPlaylistPlayer Player => _player;
    
    public SettingsViewModel(ISettingsProvider<AppSettingsData> settingsProvider, IFluentAvaloniaTheme fluentTheme, IPlaylistPlayer player) :
        base(settingsProvider, fluentTheme)
    {
        _player = player;
    }
    
    /// <inheritdoc />
    protected override bool SaveSettings()
    {
        return base.SaveSettings();
    }

    /// <inheritdoc />
    protected override bool Validate() => Settings.Validate() == null;

    /// <inheritdoc />
    protected override void RestoreDefaultImpl()
    {
        Settings.AntiAlias = false;
        Settings.AntiAliasLength = 32;
        Settings.Speed = 1;
        Settings.AutoDetectPitch = true;
        Settings.PitchFrom = 440;
        Settings.PitchTo = 432;
        Settings.RoundPitch = true;
        Settings.SkipTempo = false;
    }
}

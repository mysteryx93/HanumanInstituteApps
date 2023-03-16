using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.Common.Services.Validation;

namespace HanumanInstitute.Player432Hz.ViewModels;

/// <inheritdoc />
public class SettingsViewModel : SettingsViewModelBase<AppSettingsData>
{
    public SettingsViewModel(ISettingsProvider<AppSettingsData> settingsProvider, IFluentAvaloniaTheme fluentTheme, IPlaylistPlayer player, IEnvironmentService environment) :
        base(settingsProvider, fluentTheme)
    {
        Player = player;
        OutputSampleRateList.SelectedValue = Settings.OutputSampleRate;
        IsDeviceSampleRateVisible = environment.IsLinux;
    }
    
    public IPlaylistPlayer Player { get; }

    /// <inheritdoc />
    protected override bool SaveSettings()
    {
        Settings.OutputSampleRate = OutputSampleRateList.SelectedValue;
        return base.SaveSettings();
    }

    /// <summary>
    /// Gets or sets whether the Device Sample Rate option is visible. On Linux only, as it cannot be auto-detected.
    /// </summary>
    public bool IsDeviceSampleRateVisible { get; }
    
    /// <summary>
    /// Gets the list of device output sample rates for display.
    /// </summary>
    public ListItemCollectionView<int?> OutputSampleRateList { get; } = new()
    {
        { null, "Auto" },
        { 8000, "8000 Hz" },
        { 11025, "11,025 Hz" },
        { 16000, "16,000 Hz" },
        { 22050, "22,050 Hz" },
        { 44100, "44,100 Hz" },
        { 48000, "48,000 Hz" },
        { 88200, "88,200 Hz" },
        { 96000, "96,000 Hz" },
        { 176400, "176,400 Hz" },
        { 192000, "192,000 Hz" },
        { 352800, "352,800 Hz" },
        { 384000, "384,000 Hz" }
    };

    /// <inheritdoc />
    protected override bool Validate() => Settings.Validate() == null;

    /// <inheritdoc />
    protected override void RestoreDefaultImpl()
    {
        CheckForUpdateList.SelectedValue = UpdateInterval.Biweekly;
        Settings.AntiAlias = false;
        Settings.AntiAliasLength = 32;
        Settings.Speed = 1;
        Settings.AutoDetectPitch = true;
        Settings.PitchFrom = 440;
        Settings.PitchTo = 432;
        Settings.RoundPitch = true;
        Settings.SkipTempo = false;
        OutputSampleRateList.SelectedValue = null;
    }
}

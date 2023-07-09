using System.Text.Json.Serialization.Metadata;
using HanumanInstitute.Avalonia;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;

namespace HanumanInstitute.PowerliminalsPlayer.ViewModels;

/// <inheritdoc />
public class SettingsViewModel : SettingsViewModelBase<AppSettingsData>
{
    /// <summary>
    /// Initializes a new instance of the SettingsViewModel class.
    /// </summary>
    public SettingsViewModel(ISettingsProvider<AppSettingsData> settingsProvider, IFluentAvaloniaTheme fluentTheme, IEnvironmentService environment, IJsonTypeInfoResolver? serializerContext) :
        base(settingsProvider, fluentTheme, serializerContext)
    {
        OutputSampleRateList.SelectedValue = Settings.OutputSampleRate;
        IsDeviceSampleRateVisible = environment.IsLinux;
    }

    /// <inheritdoc />
    protected override bool SaveSettings()
    {
        Settings.OutputSampleRate = OutputSampleRateList.SelectedValue;
        BassDevice.Instance.Init(-1, Settings.OutputSampleRate);
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
    protected override void RestoreDefaultImpl()
    {
        CheckForUpdateList.SelectedValue = UpdateInterval.Biweekly;
        Settings.AntiAlias = true;
        Settings.AntiAliasLength = 32;
        Settings.PerformanceQuality = 2;
        OutputSampleRateList.SelectedValue = null;
    }
}

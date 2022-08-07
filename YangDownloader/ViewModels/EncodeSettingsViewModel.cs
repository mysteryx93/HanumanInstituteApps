using HanumanInstitute.BassAudio;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.Common.Services.Validation;
using ReactiveUI;

namespace HanumanInstitute.YangDownloader.ViewModels;

public class EncodeSettingsViewModel : OkCancelViewModel
{
    public EncodeSettingsViewModel()
    {
        this.WhenAnyValue(x => x.Settings.Format)
            .BindTo(this, x => x.FormatsList.SelectedValue);
        this.WhenAnyValue(x => x.FormatsList.SelectedValue)
            .BindTo(this, x => x.Settings.Format);

        this.WhenAnyValue(x => x.Settings.Bitrate)
            .BindTo(this, x => x.BitrateList.SelectedValue);
        this.WhenAnyValue(x => x.BitrateList.SelectedValue)
            .BindTo(this, x => x.Settings.Bitrate);

        this.WhenAnyValue(x => x.Settings.SampleRate)
            .BindTo(this, x => x.SampleRateList.SelectedValue);
        this.WhenAnyValue(x => x.SampleRateList.SelectedValue)
            .BindTo(this, x => x.Settings.SampleRate);

        _isBitrateVisible = this.WhenAnyValue(x => x.FormatsList.SelectedValue, x => x != EncodeFormat.Flac && x != EncodeFormat.Wav)
            .ToProperty(this, x => x.IsBitrateVisible);
        _isSampleRateVisible = this.WhenAnyValue(x => x.FormatsList.SelectedValue, x => x != EncodeFormat.Opus)
            .ToProperty(this, x => x.IsSampleRateVisible);
        _isQualitySpeedVisible = this.WhenAnyValue(x => x.FormatsList.SelectedValue, x => x == EncodeFormat.Mp3 || x == EncodeFormat.Flac)
            .ToProperty(this, x => x.IsQualitySpeedVisible);
    }

    [Reactive]
    public EncodeSettings Settings { get; private set; } = default!;

    private EncodeSettings _source = default!;

    public void SetSettings(EncodeSettings settings)
    {
        _source = settings;
        Settings = Cloning.ShallowClone(settings);
        ShiftPitch = Settings.AutoDetectPitch || Math.Abs(Settings.PitchFrom - Settings.PitchTo) > 0.001;
        this.RaisePropertyChanged(nameof(Settings));
    }

    protected override bool SaveSettings()
    {
        Cloning.CopyAllFields(Settings, _source);
        return true;
    }

    public bool ShiftPitch
    {
        get => _shiftPitch;
        set
        {
            this.RaiseAndSetIfChanged(ref _shiftPitch, value);
            if (value == false)
            {
                Settings.AutoDetectPitch = false;
                Settings.PitchFrom = 440;
                Settings.PitchTo = 440;
            }
        }
    }
    private bool _shiftPitch;

    /// <summary>
    /// Gets whether Bitrate control should be visible.
    /// </summary>
    public bool IsBitrateVisible => _isBitrateVisible.Value;
    private readonly ObservableAsPropertyHelper<bool> _isBitrateVisible;

    /// <summary>
    /// Gets whether SampleRate control should be visible.
    /// </summary>
    public bool IsSampleRateVisible => _isSampleRateVisible.Value;
    private readonly ObservableAsPropertyHelper<bool> _isSampleRateVisible;

    /// <summary>
    /// Gets whether QualitySpeed slider should be visible.
    /// </summary>
    public bool IsQualitySpeedVisible => _isQualitySpeedVisible.Value;
    private readonly ObservableAsPropertyHelper<bool> _isQualitySpeedVisible;

    public ListItemCollectionView<EncodeFormat> FormatsList { get; } = new()
    {
        { EncodeFormat.Mp3, "MP3" },
        { EncodeFormat.Aac, "AAC" },
        { EncodeFormat.Wav, "WAV" },
        { EncodeFormat.Flac, "FLAC" },
        { EncodeFormat.Ogg, "OGG" },
        { EncodeFormat.Opus, "OPUS" }
    };

    public ListItemCollectionView<int> SampleRateList { get; } = new()
    {
        { 0, "Source" },
        { 44100, "44100hz" },
        { 48000, "48000hz" }
    };

    public ListItemCollectionView<int> BitrateList { get; } = new()
    {
        { 0, "Source" },
        { 96, "96 kbps" },
        { 128, "128 kbps" },
        { 192, "192 kbps" },
        { 256, "256 kbps" },
        { 320, "320 kbps" }
    };
    
    /// <summary>
    /// Restores default settings.
    /// </summary>
    public RxCommandUnit RestoreDefault => _restoreDefault ??= ReactiveCommand.Create(RestoreDefaultImpl);
    private RxCommandUnit? _restoreDefault;
    /// <summary>
    /// When overriden in a derived class, restores default settings.
    /// </summary>
    private void RestoreDefaultImpl()
    {
        Settings.Format = EncodeFormat.Mp3;
        Settings.Bitrate = 0;
        Settings.SampleRate = 0;
        Settings.QualityOrSpeed = 5; 
        ShiftPitch = false;
        Settings.AntiAlias = false;
        Settings.AntiAliasLength = 32;
        Settings.AutoDetectPitch = true;
        Settings.PitchFrom = 440;
        Settings.PitchTo = 432;
    }
}

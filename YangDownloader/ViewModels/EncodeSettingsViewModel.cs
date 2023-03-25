using System.Linq.Expressions;
using System.Text.Json.Serialization.Metadata;
using HanumanInstitute.BassAudio;
using HanumanInstitute.Common.Avalonia.App;
using ReactiveUI;

namespace HanumanInstitute.YangDownloader.ViewModels;

public class EncodeSettingsViewModel : OkCancelViewModel
{
    private readonly IAudioEncoder _encoder;
    private readonly IEnvironmentService _environment;
    private readonly ISettingsProvider<AppSettingsData> _appSettings;
    private readonly IJsonTypeInfoResolver? _serializerContext;
    
    public EncodeSettingsViewModel(IAudioEncoder encoder, IEnvironmentService environment, ISettingsProvider<AppSettingsData> appSettings, IJsonTypeInfoResolver serializerContext)
    {
        _encoder = encoder;
        _environment = environment;
        _appSettings = appSettings;
        _serializerContext = serializerContext;
        
        Bind(x => x.Settings.Format, x => x.FormatsList.SelectedValue);
        Bind(x => x.Settings.Bitrate, x => x.BitrateList.SelectedValue);
        Bind(x => x.Settings.BitsPerSample, x => x.BitsPerSampleList.SelectedValue);
        Bind(x => x.Settings.SampleRate, x => x.SampleRateList.SelectedValue);

        _isBitrateVisible = this.WhenAnyValue(x => x.FormatsList.SelectedValue, x => x != EncodeFormat.Flac && x != EncodeFormat.Wav)
            .ToProperty(this, x => x.IsBitrateVisible);
        _isToggleBitrateVisible = this.WhenAnyValue(x => x.FormatsList.SelectedValue, x => x == EncodeFormat.Mp3)
            .ToProperty(this, x => x.IsToggleBitrateVisible);
        _isBitsPerSampleVisible = this.WhenAnyValue(x => x.FormatsList.SelectedValue, x => x == EncodeFormat.Flac || x == EncodeFormat.Wav)
            .ToProperty(this, x => x.IsBitsPerSampleVisible);
        _isQualitySpeedVisible = this.WhenAnyValue(x => x.FormatsList.SelectedValue, x => x == EncodeFormat.Mp3 || x == EncodeFormat.Flac)
            .ToProperty(this, x => x.IsQualitySpeedVisible);
        this.WhenAnyValue(x => x.Settings.Format).Subscribe(FillSampleRateList);
    }
    
    private void Bind<T1, T2>(Expression<Func<EncodeSettingsViewModel, T1?>> expr1, Expression<Func<EncodeSettingsViewModel, T2?>> expr2)
    {
        this.WhenAnyValue(expr1)
            .BindTo(this, expr2);
        this.WhenAnyValue(expr2)
            .BindTo(this, expr1);
    }

    [Reactive]
    public EncodeSettings Settings { get; private set; } = default!;

    public AppSettingsData AppSettings => _appSettings.Value;

    private EncodeSettings _source = default!;

    public void SetSettings(EncodeSettings settings)
    {
        _source = settings;
        Settings = Cloning.DeepClone(settings, _serializerContext);
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
            if (value != _shiftPitch)
            {
                Settings.AutoDetectPitch = value;
                Settings.PitchFrom = 440;
                Settings.PitchTo = value ? 432 : 440;
            }
            this.RaiseAndSetIfChanged(ref _shiftPitch, value);
        }
    }
    private bool _shiftPitch;

    /// <summary>
    /// Gets whether Bitrate control should be visible.
    /// </summary>
    public bool IsBitrateVisible => _isBitrateVisible.Value;
    private readonly ObservableAsPropertyHelper<bool> _isBitrateVisible;
    
    /// <summary>
    /// Gets or sets whether Toggle Bitrate button should be visible. 
    /// </summary>
    public bool IsToggleBitrateVisible => _isToggleBitrateVisible.Value;
    private readonly ObservableAsPropertyHelper<bool> _isToggleBitrateVisible;

    /// <summary>
    /// Gets whether Bits Per Sample control should be visible.
    /// </summary>
    public bool IsBitsPerSampleVisible => _isBitsPerSampleVisible.Value;
    private readonly ObservableAsPropertyHelper<bool> _isBitsPerSampleVisible;

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

    public ListItemCollectionView<int> SampleRateList { get; } = new();

    private void FillSampleRateList(EncodeFormat format)
    {
        var selection = SampleRateList.SelectedValue;
        List<ListItem<int>> list = new() { { 0, "Source" } };
        foreach (var sampleRate in _encoder.GetSupportedSampleRates(format))
        {
            list.Add(sampleRate, string.Format(_environment.CurrentCulture, "{0} Hz", sampleRate));
        }
        
        SampleRateList.Source.Clear();
        SampleRateList.AddRange(list);
        SampleRateList.SelectedValue = SampleRateList.Source.Any(x => x.Value == selection) ? selection : 48000;
    }

    public ListItemCollectionView<int> BitrateList { get; } = new()
    {
        { 0, "Source" },
        { 96, "96 kbps" },
        { 128, "128 kbps" },
        { 192, "192 kbps" },
        { 256, "256 kbps" },
        { 320, "320 kbps" }
    };
    
    public ListItemCollectionView<int> BitsPerSampleList { get; } = new()
    {
        { 8, "8-bits" },
        { 16, "16-bits" },
        { 24, "24-bits" },
        { 32, "32-bits" }
    };
    
    /// <summary>
    /// Enables or disables the Fixed Bitrate option.
    /// </summary>
    public RxCommandUnit ToggleFixedBitrate => _toggleFixedBitrate ??= ReactiveCommand.Create(ToggleFixedBitrateImpl);
    private RxCommandUnit? _toggleFixedBitrate;
    private void ToggleFixedBitrateImpl()
    {
        Settings.FixedBitrate = !Settings.FixedBitrate;
    }
    
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

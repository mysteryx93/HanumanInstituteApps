using System.Linq;
using HanumanInstitute.BassAudio;
using HanumanInstitute.Common.Avalonia.App;
using ReactiveUI;

namespace HanumanInstitute.Player432Hz.Business;

/// <inheritdoc cref="IPlaylistPlayer" />
public class PlaylistPlayer : BaseWithSettings<AppSettingsData>, IPlaylistPlayer
{
    private readonly IPitchDetector _pitchDetector;
    private readonly IFileSystemService _fileSystem;

    public PlaylistPlayer(IPitchDetectorWithCache pitchDetector, IFileSystemService fileSystem, ISettingsProvider<AppSettingsData> settings) :
        base(settings)
    {
        _pitchDetector = pitchDetector;
        _fileSystem = fileSystem;
    }

    /// <inheritdoc />
    public IList<string> Files { get; } = new List<string>();

    /// <inheritdoc />
    [Reactive]
    public string NowPlaying { get; set; } = string.Empty;

    /// <inheritdoc />
    [Reactive]
    public string NowPlayingTitle { get; set; } = string.Empty;

    /// <inheritdoc />
    [Reactive]
    public double PitchFrom { get; private set; } = 440;

    /// <inheritdoc />
    [Reactive]
    public double Pitch { get; private set; } = 1;

    /// <inheritdoc />
    public double? PitchError
    {
        get => _pitchError;
        set
        {
            this.RaiseAndSetIfChanged(ref _pitchError, value);
            this.RaisePropertyChanged(nameof(PitchErrorHz));
        }
    }
    private double? _pitchError;

    /// <inheritdoc />
    public double? PitchErrorHz => PitchError * PitchFrom;

    private readonly Random _random = new Random();

    /// <inheritdoc />
    public async Task PlayAsync(IEnumerable<string>? list, string? current)
    {
        Files.Clear();
        if (list != null)
        {
            Files.AddRange(list);
        }
        NowPlaying = string.Empty;
        if (current.HasValue())
        {
            await CalcPitchAsync(current).ConfigureAwait(false);
            NowPlaying = current;
        }
        SetTitle();
        if (string.IsNullOrEmpty(current))
        {
            await PlayNextAsync().ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task PlayNextAsync()
    {
        if (Files.Any())
        {
            var pos = _random.Next(Files.Count);
            if (Files[pos] == NowPlaying)
            {
                pos = _random.Next(Files.Count);
            }

            await CalcPitchAsync(Files[pos]).ConfigureAwait(false);

            NowPlaying = string.Empty;
            NowPlaying = Files[pos];
            SetTitle();
        }
    }

    private async Task CalcPitchAsync(string filePath)
    {
        PitchFrom = _settings.Value.AutoDetectPitch ? await _pitchDetector.GetPitchAsync(filePath).ConfigureAwait(false) : _settings.Value.PitchFrom;
        SetTitle();
        Pitch = _settings.Value.PitchTo / PitchFrom;
    }

    private void SetTitle()
    {
        NowPlayingTitle = string.IsNullOrEmpty(NowPlaying) ?
            string.Empty :
            (Settings.AutoDetectPitch ? $"[{PitchFrom:F1}] " : "") +
            _fileSystem.Path.GetFileName(NowPlaying);
    }

    protected override async void ApplySettings()
    {
        if (NowPlaying.HasValue())
        {
            await CalcPitchAsync(NowPlaying);
        }
    }
    

    // /// <inheritdoc />
    // public void ApplySettings()
    // {
    //     if (NowPlaying.HasValue())
    //     {
    //         CalcPitch(NowPlaying);
    //     }
    //     this.RaisePropertyChanged(nameof(Settings));
    // }
}

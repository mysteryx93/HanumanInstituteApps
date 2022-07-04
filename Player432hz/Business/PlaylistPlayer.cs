using System.Linq;
using HanumanInstitute.BassAudio;
using ReactiveUI;

namespace HanumanInstitute.Player432hz.Business;

/// <summary>
/// Manages the playback of a list of media files.
/// </summary>
public class PlaylistPlayer : ReactiveObject, IPlaylistPlayer
{
    private readonly IPitchDetector _pitchDetector;
    private readonly IFileSystemService _fileSystem;

    public PlaylistPlayer(IPitchDetector pitchDetector, IFileSystemService fileSystem)
    {
        _pitchDetector = pitchDetector;
        _fileSystem = fileSystem;
    }

    /// <summary>
    /// Gets the list of files currently playing.
    /// </summary>
    public IList<string> Files { get; } = new List<string>();

    /// <summary>
    /// Gets the path of the file currently playing.
    /// </summary>
    [Reactive]
    public string NowPlaying { get; set; } = string.Empty;

    /// <summary>
    /// Gets the display title of the file currently playing.
    /// </summary>
    [Reactive]
    public string NowPlayingTitle { get; set; } = string.Empty;

    public double Pitch => 432.0 / PitchFrom;

    public double PitchFrom
    {
        get => _pitchFrom;
        set
        {
            this.RaiseAndSetIfChanged(ref _pitchFrom, value);
            this.RaisePropertyChanged(nameof(Pitch));
        }
    }
    private double _pitchFrom = 440.0;

    private readonly Random _random = new Random();

    /// <summary>
    /// Starts the playback of specified list of media files.
    /// </summary>
    /// <param name="list">The list of file paths to play.</param>
    /// <param name="current">If specified, playback will start with specified file.</param>
    public void Play(IEnumerable<string>? list, string? current)
    {
        Files.Clear();
        if (list != null)
        {
            Files.AddRange(list);
        }
        NowPlaying = string.Empty;
        NowPlaying = current ?? string.Empty;
        SetTitle();
        if (string.IsNullOrEmpty(current))
        {
            PlayNext();
        }
    }

    /// <summary>
    /// Starts playing the next media file from the list.
    /// </summary>
    public void PlayNext()
    {
        if (Files.Any())
        {
            var pos = _random.Next(Files.Count);
            if (Files[pos] == NowPlaying)
            {
                pos = _random.Next(Files.Count);
            }

            PitchFrom = _pitchDetector.GetPitch(Files[pos]);
            
            NowPlaying = string.Empty;
            NowPlaying = Files[pos];
            SetTitle();
        }
    }

    private void SetTitle()
    {
        NowPlayingTitle = !string.IsNullOrEmpty(NowPlaying) ? $"[{PitchFrom:F1}] " + _fileSystem.Path.GetFileName(NowPlaying) : string.Empty;
    }
}

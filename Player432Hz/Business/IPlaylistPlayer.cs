using System.ComponentModel;

namespace HanumanInstitute.Player432Hz.Business;

/// <summary>
/// Manages the playback of a list of media files.
/// </summary>
public interface IPlaylistPlayer : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the list of files currently playing.
    /// </summary>
    IList<string> Files { get; }
    /// <summary>
    /// Gets the path of the file currently playing.
    /// </summary>
    string NowPlaying { get; set; }
    /// <summary>
    /// Gets the display title of the file currently playing.
    /// </summary>
    string NowPlayingTitle { get; set; }
    /// <summary>
    /// Gets the media player pitch.
    /// </summary>
    double Pitch { get; }
    /// <summary>
    /// Gets or sets the pitch of the currently-playing file. Used to calculate the Pitch property.
    /// </summary>
    double PitchFrom { get; }
    /// <summary>
    /// Gets the pitch rounding error when TempoCompensation = Optimized.
    /// </summary>
    double? PitchError { get; set; }
    /// <summary>
    /// Gets the pitch rounding error in Hz. 
    /// </summary>
    double? PitchErrorHz { get; }
    /// <summary>
    /// Starts the playback of specified list of media files.
    /// </summary>
    /// <param name="list">The list of file paths to play.</param>
    /// <param name="current">If specified, playback will start with specified file.</param>
    Task PlayAsync(IEnumerable<string>? list, string? current);
    /// <summary>
    /// Starts playing the next media file from the list.
    /// </summary>
    Task PlayNextAsync();
    // /// <summary>
    // /// Applies settings to the active player.
    // /// </summary>
    // void ApplySettings();
    /// <summary>
    /// Gets application settings.
    /// </summary>
    public AppSettingsData Settings { get; }
}

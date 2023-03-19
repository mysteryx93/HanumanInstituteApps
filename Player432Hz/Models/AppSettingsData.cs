using System.ComponentModel.DataAnnotations;
using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.Player432Hz.Models;

/// <summary>
/// Contains the application settings and configured playlists.
/// </summary>
public class AppSettingsData : SettingsDataBase
{
    /// <summary>
    /// Gets or sets the list of configured playlists.
    /// </summary>
    [ValidateObject]
    [Reactive]
    public List<SettingsPlaylistItem> Playlists { get; set; } = new();

    /// <summary>
    /// Gets or sets the playback volume.
    /// </summary>
    [Range(0, 100)]
    [Reactive]
    public int Volume { get; set; } = 100;

    /// <summary>
    /// Gets or sets whether to enable the Anti-Alias filter.
    /// </summary>
    [Reactive]
    public bool AntiAlias { get; set; }

    /// <summary>
    /// Gets or sets the Anti-Alias filter length. 
    /// </summary>
    [Reactive]
    [Range(minimum: 8, maximum: 128)]
    public int AntiAliasLength { get; set; } = 32;

    /// <summary>
    /// Gets or sets the speed multiplier. Default=1.
    /// </summary>
    [Reactive]
    [RangeClusive(min: 0, minInclusive: false)]
    public double Speed { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether to auto-detect music pitch.
    /// </summary>
    [Reactive]
    public bool AutoDetectPitch { get; set; } = true;

    /// <summary>
    /// Gets or sets the pitch of the source audio. 
    /// </summary>
    [Reactive]
    [Range(1, 10000)]
    public double PitchFrom { get; set; } = 440;

    /// <summary>
    /// Gets or sets the pitch to shift to.
    /// </summary>
    [Reactive]
    [Range(1, 10000)]
    public double PitchTo { get; set; } = 432;

    /// <summary>
    /// Gets or sets whether to round the pitch to the nearest fraction when pitch-shifting for enhanced quality. 
    /// </summary>
    [Reactive]
    public bool RoundPitch { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to skip tempo adjustment for maximum audio quality. 
    /// </summary>
    [Reactive]
    public bool SkipTempo { get; set; }
    
    /// <summary>
    /// On Linux, sets the device output sample rate. On other platforms, null = auto-detected.
    /// </summary>
    [Reactive]
    public int? OutputSampleRate { get; set; }

    /// <inheritdoc />
    public override void SetFreeLicenseDefaults()
    {
        AntiAlias = false;
        AntiAliasLength = 32;
        Speed = 1;
        AutoDetectPitch = true;
        PitchFrom = 440;
        PitchTo = 432;
        RoundPitch = true;
        SkipTempo = false;
    }
}

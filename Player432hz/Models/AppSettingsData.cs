using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using HanumanInstitute.Common.Avalonia.App;
using ReactiveUI;

namespace HanumanInstitute.Player432hz.Models;

/// <summary>
/// Contains the application settings and configured playlists.
/// </summary>
[Serializable]
[XmlRoot("Player432hz")]
public class AppSettingsData : SettingsDataBase
{
    /// <summary>
    /// Gets or sets the list of configured playlists.
    /// </summary>
    [ValidateObject]
    [XmlElement("Playlist")]
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
    /// Creates a copy of this instance.
    /// </summary>
    /// <returns>The cloned data.</returns>
    public AppSettingsData Clone()
    {
        var obj = new AppSettingsData();
        ExtensionMethods.CopyAllFields(this, obj);
        return obj;
    }
}

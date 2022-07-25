using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using HanumanInstitute.Common.Avalonia.App;

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
}

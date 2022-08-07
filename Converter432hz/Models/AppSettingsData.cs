using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.Converter432hz.Models;

/// <summary>
/// Contains the application settings and configured playlists.
/// </summary>
[Serializable]
[XmlRoot("Converter432hz")]
public class AppSettingsData : SettingsDataBase
{
    [Reactive]
    public EncodeSettings Encode { get; set; } = new();

    /// <summary>
    /// Gets or sets the maximum amount of simultaneous encodes.
    /// </summary>
    [Reactive]
    [Range(1, 64)]
    public int MaxThreads { get; set; } = 1;
}

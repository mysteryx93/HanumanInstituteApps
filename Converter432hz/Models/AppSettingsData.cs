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
    /// <summary>
    /// Encoding settings.
    /// </summary>
    [Reactive]
    public EncodeSettings Encode { get; set; } = new();

    /// <summary>
    /// Gets or sets the destination folder where to encode the files.
    /// </summary>
    [Reactive]
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the action to take when the destination file already exists.
    /// </summary>
    [Reactive]
    public FileExistsAction FileExistsAction { get; set; } = FileExistsAction.Ask;

    /// <summary>
    /// Gets or sets the maximum amount of simultaneous encodes.
    /// </summary>
    [Reactive]
    [Range(1, 64)]
    public int MaxThreads { get; set; } = 1;
}

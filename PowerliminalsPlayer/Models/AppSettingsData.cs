using System.Xml.Serialization;
using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.PowerliminalsPlayer.Models;

/// <summary>
/// Contains the PowerliminalsPlayer application settings.
/// </summary>
[XmlRoot("PowerliminalsPlayer")]
public class AppSettingsData : SettingsDataBase
{
    /// <summary>
    /// Gets or sets the list of folders in which to look for audio files.
    /// </summary>
    [XmlElement("Folder")]
    public ObservableCollection<string> Folders { get; } = new ObservableCollection<string>();

    /// <summary>
    /// Gets or sets the list of saved presets.
    /// </summary>
    //[XmlElement("Preset")]
    public ObservableCollection<PresetItem> Presets { get; } = new ObservableCollection<PresetItem>();

    /// <summary>
    /// Gets or sets whether the folders section is expanded.
    /// </summary>
    [Reactive]
    public bool FoldersExpanded { get; set; } = true;

    /// <summary>
    /// Creates a copy of the SettingsFile class.
    /// </summary>
    /// <returns>The copied object.</returns>
    public AppSettingsData Clone() => (AppSettingsData)MemberwiseClone();


}

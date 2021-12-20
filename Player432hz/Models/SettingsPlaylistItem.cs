using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace HanumanInstitute.Player432hz.Models;

/// <summary>
/// Contains information on a playlist.
/// </summary>
[Serializable]
public class SettingsPlaylistItem
{
    /// <summary>
    /// Gets or sets the name of the playlist.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the list of folders included in the playlist.
    /// </summary>
    [XmlElement("Folder")]
    public List<string> Folders { get; } = new List<string>();

    public SettingsPlaylistItem() { }

    public SettingsPlaylistItem(string name, IList<string>? folders = null)
    {
        this.Name = name;
        if (folders != null)
        {
            Folders.AddRange(folders);
        }
    }
}

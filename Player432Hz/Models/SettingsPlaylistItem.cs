namespace HanumanInstitute.Player432Hz.Models;

/// <summary>
/// Contains information on a playlist.
/// </summary>
public class SettingsPlaylistItem
{
    /// <summary>
    /// Gets or sets the name of the playlist.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the list of folders included in the playlist.
    /// </summary>
    public List<string> Folders { get; set; } = new();

    public SettingsPlaylistItem() { }

    public SettingsPlaylistItem(string name, IList<string>? folders = null)
    {
        Name = name;
        if (folders != null)
        {
            Folders.AddRange(folders);
        }
    }
}

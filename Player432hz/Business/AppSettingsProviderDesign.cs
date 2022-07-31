namespace HanumanInstitute.Player432hz.Business;

/// <summary>
/// Application settings for design view.
/// </summary>
public sealed class AppSettingsProviderDesign : ISettingsProvider<AppSettingsData>
{
    public AppSettingsProviderDesign()
    {
        Value = new AppSettingsData
        {
            Width = 560,
            Height = 350,
            Playlists = new List<SettingsPlaylistItem>()
            {
                new("Item 1", new List<string>(new[] { "Folder1" })),
                new("Item 2")
            }
        };
    }

    public AppSettingsData Value { get; set; }

#pragma warning disable 67
    public event EventHandler? Loaded;
    public event EventHandler? Saved;
#pragma warning restore
        
    /// <inheritdoc />
    public AppSettingsData Load() => Value;

    /// <inheritdoc />
    public AppSettingsData Load(string path) => Value;

    /// <inheritdoc />
    public void Save()
    {
    }

    /// <inheritdoc />
    public void Save(string path)
    {
    }
}

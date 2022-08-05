namespace HanumanInstitute.Player432hz.Business;

/// <summary>
/// Application settings for design view.
/// </summary>
public sealed class AppSettingsProviderDesign : SettingsProviderDesign<AppSettingsData>
{
    public AppSettingsProviderDesign() : base(
        new AppSettingsData
        {
            Width = 560,
            Height = 350,
            Playlists = new List<SettingsPlaylistItem>()
            {
                new("Item 1", new List<string>(new[] { "Folder1" })),
                new("Item 2")
            }
        })
    {
    }
}

namespace HanumanInstitute.Converter432hz.Business;

/// <summary>
/// Application settings for design view.
/// </summary>
public sealed class AppSettingsProviderDesign : SettingsProviderDesign<AppSettingsData>
{
    public AppSettingsProviderDesign() : base(
        new AppSettingsData
        {
            Width = 600,
            Height = 400
        })
    {
    }
}

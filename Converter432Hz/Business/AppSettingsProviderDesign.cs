using HanumanInstitute.Services;

namespace HanumanInstitute.Converter432Hz.Business;

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

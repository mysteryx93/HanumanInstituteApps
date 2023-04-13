using HanumanInstitute.Services;

namespace HanumanInstitute.YangDownloader.Business;

/// <summary>
/// Application settings for design view.
/// </summary>
public sealed class AppSettingsProviderDesign : SettingsProviderDesign<AppSettingsData>
{
    public AppSettingsProviderDesign() : base(
        new AppSettingsData
        {
            Width = 540,
            Height = 400
        })
    {
    }
}

using HanumanInstitute.Services;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

/// <summary>
/// Application settings for design view.
/// </summary>
public sealed class AppSettingsProviderDesign : SettingsProviderDesign<AppSettingsData>
{
    public AppSettingsProviderDesign() : base(
        new AppSettingsData
        {
            Width = 680,
            Height = 380
        })
    {
    }
}

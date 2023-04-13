using HanumanInstitute.Apps.AdRotator;

namespace HanumanInstitute.Apps;

/// <summary>
/// Provides access to server API functions.
/// </summary>
public interface IHanumanInstituteHttpClient
{
    /// <summary>
    /// Returns the latest app version available for download, and latest ads date.
    /// </summary>
    /// <returns>An object containing version info.</returns>
    Task<HanumanInstituteHttpClient.AppVersionQuery?> QueryVersionAsync();
    /// <summary>
    /// Returns updated ads from the server.
    /// </summary>
    /// <returns>An object containing ads data.</returns>
    Task<AdInfo?> GetAdsAsync();
    /// <summary>
    /// Returns the ad link tracker URL for specified ad. App and OS will be filled automatically.
    /// </summary>
    /// <param name="ad">The ID of the ad to open.</param>
    /// <returns>The constructed query URL.</returns>
    string GetLinkTrackerUrl(int ad);
}

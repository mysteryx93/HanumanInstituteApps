namespace HanumanInstitute.Apps;

/// <summary>
/// Version information returned by HanumanInstituteHttpClient.QueryVersion.
/// </summary>
public class AppVersionQuery
{
    /// <summary>
    /// The latest app version available for download.
    /// </summary>
    public Version LatestVersion { get; set; } = default!;
    /// <summary>
    /// The date ads were last updated on the server, in UTC time.
    /// </summary>
    public DateTime AdsLastUpdated { get; set; }
}

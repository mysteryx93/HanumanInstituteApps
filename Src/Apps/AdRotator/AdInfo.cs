namespace HanumanInstitute.Apps.AdRotator;

/// <summary>
/// Contains information about ads for the AdRotator.
/// </summary>
public class AdInfo
{
    /// <summary>
    /// The date ads were last updated. This is used to check for new ads data to download.
    /// </summary>
    public DateTime LastUpdated { get; set; }
    /// <summary>
    /// The list of ads to display.
    /// </summary>
    public List<AdItem> Ads { get; set; } = new();
}

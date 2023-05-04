namespace HanumanInstitute.Apps.AdRotator;

/// <summary>
/// Contains information about an ad for the AdRotator.
/// </summary>
public class AdItem
{
    /// <summary>
    /// A unique ID to identify the ad.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The apps for which the ad applies, or null for all.
    /// </summary>
    public int[]? Apps { get; set; }

    /// <summary>
    /// The date from which to show the ad, or null.
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// The date until when to show the ad, or null.
    /// </summary>
    public DateTime? End { get; set; }
    
    /// <summary>
    /// The ad to display on desktop, in markdown format.
    /// </summary>
    public string Markdown { get; set; } = string.Empty;

    /// <summary>
    /// The ad to display on mobile, in markdown format.
    /// </summary>
    public string MarkdownMobile { get; set; } = string.Empty;

    /// <summary>
    /// The path to navigate to when clicking the ad.
    /// </summary>
    public string NavigateUrl { get; set; } = "https://www.spiritualselftransformation.com/";
}

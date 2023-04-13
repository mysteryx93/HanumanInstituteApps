using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HanumanInstitute.Apps.AdRotator;

/// <summary>
/// Contains information about an ad for the AdRotator.
/// </summary>
public class AdItem : ReactiveObject
{
    /// <summary>
    /// A unique ID to identify the ad.
    /// </summary>
    [Reactive]
    public int Id { get; set; }
    
    /// <summary>
    /// The ad to display on desktop, in markdown format.
    /// </summary>
    [Reactive]
    public string Markdown { get; set; } = string.Empty;

    /// <summary>
    /// The ad to display on mobile, in markdown format.
    /// </summary>
    [Reactive]
    public string MarkdownMobile { get; set; } = string.Empty;

    /// <summary>
    /// The path to navigate to when clicking the ad.
    /// </summary>
    [Reactive]
    public string NavigateUrl { get; set; } = "https://www.spiritualselftransformation.com/";
}

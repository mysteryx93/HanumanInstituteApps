namespace HanumanInstitute.Apps.AdRotator;

/// <summary>
/// The ViewModel for the AdRotator UserControl.
/// </summary>
public interface IAdRotatorViewModel
{
    /// <summary>
    /// Gets or sets the application being run.
    /// </summary>
    public AppIdentifier AppId { get; set; }
    /// <summary>
    /// Gets or sets whether the AdRotator is enabled.
    /// </summary>
    bool Enabled { get; set; }
    /// <summary>
    /// The information about all available ads.
    /// </summary>
    AdInfo AdInfo { get; }
    /// <summary>
    /// Gets or sets the currently displayed ad.
    /// </summary>
    AdItem? Current { get; set; }
    /// <summary>
    /// Opens the currently displayed ad into the web browser.
    /// </summary>
    RxCommandUnit OpenLink { get; }
    /// <summary>
    /// Updates ads information from the server and save it locally.
    /// </summary>
    Task LoadFromServerAsync();
}

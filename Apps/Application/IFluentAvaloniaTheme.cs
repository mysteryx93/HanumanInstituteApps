namespace HanumanInstitute.Apps;

/// <summary>
/// Provides an interface for FluentAvaloniaTheme allowing to mock it.
/// </summary>
public interface IFluentAvaloniaTheme
{
    /// <summary>
    /// Gets or sets the desired theme mode (Light, Dark, or HighContrast) for the app
    /// </summary>
    public string RequestedTheme { get; set; }
}

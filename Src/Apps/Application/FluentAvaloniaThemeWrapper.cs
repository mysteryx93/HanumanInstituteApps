using Avalonia;
using Avalonia.Styling;

namespace HanumanInstitute.Apps;

/// <inheritdoc />
public class FluentAvaloniaThemeWrapper : IFluentAvaloniaTheme
{
    public FluentAvaloniaThemeWrapper()
    {
    }

    /// <inheritdoc />
    public string RequestedTheme
    {
        get => Application.Current!.RequestedThemeVariant?.Key.ToString() ?? string.Empty;
        set => Application.Current!.RequestedThemeVariant = value == "Dark" ? ThemeVariant.Dark : ThemeVariant.Light;
    }
}

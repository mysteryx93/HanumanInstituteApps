using FluentAvalonia.Styling;

namespace HanumanInstitute.Apps;

/// <inheritdoc />
public class FluentAvaloniaThemeWrapper : IFluentAvaloniaTheme
{
    private readonly FluentAvaloniaTheme _theme;
    
    public FluentAvaloniaThemeWrapper(FluentAvaloniaTheme theme)
    {
        _theme = theme;
    }

    /// <inheritdoc />
    public string RequestedTheme
    {
        get => _theme.RequestedTheme;
        set => _theme.RequestedTheme = value;
    }
}

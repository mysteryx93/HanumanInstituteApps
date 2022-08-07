using FluentAvalonia.Styling;

namespace HanumanInstitute.Common.Avalonia.App;

/// <inheritdoc />
public class FluentAvaloniaThemeWrapper : IFluentAvaloniaTheme
{
    private FluentAvaloniaTheme _theme;
    
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

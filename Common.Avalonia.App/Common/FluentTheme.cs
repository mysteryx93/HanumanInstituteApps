using System.Linq;
using Avalonia;
using Avalonia.Media;
using FluentAvalonia.Styling;

namespace HanumanInstitute.Common.Avalonia.App;

public static class FluentTheme
{
    public static void Init(AppTheme theme)
    {
        var style = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>()!;
        
        //style.PreferUserAccentColor = false;
        style.RequestedTheme = theme.ToString();
        //global::Avalonia.Application.Current!.Resources["ButtonBackground"] = "ButtonBackground";
        //style.CustomAccentColor = Colors.Orange;
        
        // style.RequestedThemeChanged += Style_RequestedThemeChanged;
    }

    private static void Style_RequestedThemeChanged(FluentAvaloniaTheme sender, RequestedThemeChangedEventArgs args)
    {
    }
}

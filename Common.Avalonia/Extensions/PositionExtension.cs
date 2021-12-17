using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using HanumanInstitute.Common.Services;

namespace HanumanInstitute.Common.Avalonia;

public class PositionExtension : AvaloniaObject
{
    static PositionExtension()
    {
        
    }
    
    // Position (Window.Position doesn't support binding by default) 
    public static readonly AttachedProperty<PixelPoint> PositionProperty =
        AvaloniaProperty.RegisterAttached<PositionExtension, Window, PixelPoint>("Position"); 
    public static PixelPoint GetPosition(AvaloniaObject d) => d.CheckNotNull(nameof(d)).GetValue(PositionProperty);
    public static void SetPosition(AvaloniaObject d, PixelPoint value) => d.CheckNotNull(nameof(d)).SetValue(PositionProperty, value);
    private static void OnPositionChanged(AvaloniaPropertyChangedEventArgs<PixelPoint> e)
    {
        if (e.Sender is Window p)
        {
            p.Position = e.NewValue.Value;
        }
    }
}

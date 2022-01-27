using System;
using Avalonia;
using Avalonia.Controls;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.Common.Avalonia;

public class PositionExtension : AvaloniaObject
{
    static PositionExtension()
    {
        PositionProperty.Changed.Subscribe(OnPositionChanged);
    }
    
    // Position with binding
    // Note: Window.Position doesn't support binding by default 
    // https://github.com/AvaloniaUI/Avalonia/pull/3521#issuecomment-582243868
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

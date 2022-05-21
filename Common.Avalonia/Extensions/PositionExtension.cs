using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.Common.Avalonia;

public class PositionExtension : AvaloniaObject
{
    static PositionExtension()
    {
        EnabledProperty.Changed.Subscribe(OnEnabledChanged);
        PositionProperty.Changed.Subscribe(OnPositionChanged);
    }
    
    // Allows binding the window position.
    public static readonly AttachedProperty<bool> EnabledProperty =
        AvaloniaProperty.RegisterAttached<PositionExtension, Layoutable, bool>("Enabled"); 
    public static bool GetEnabled(AvaloniaObject d) => d.CheckNotNull(nameof(d)).GetValue(EnabledProperty);
    public static void SetEnabled(AvaloniaObject d, bool value) => d.CheckNotNull(nameof(d)).SetValue(EnabledProperty, value);
    private static void OnEnabledChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is Window p)
        {
            p.Position = GetPosition(p).ToPixelPoint();
            if (e.NewValue.Value)
            {
                p.PositionChanged += Window_PositionChanged;
                p.Opened += Window_Opened;
            }
            else
            {
                p.PositionChanged -= Window_PositionChanged;
                p.Initialized -= Window_Opened;
            }
        }
    }
    
    private static void Window_Opened(object? sender, EventArgs e)
    {
        if (sender is CommonWindow p)
        {
            p.SetManualStartupPosition(GetPosition(p).ToPixelPoint());
        }
        
    }
    
    private static void Window_PositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (sender is Window p)
        {
            SetPosition(p, new PositionPoint(e.Point));
        }
    }

    public static readonly AttachedProperty<PositionPoint> PositionProperty =
        AvaloniaProperty.RegisterAttached<PositionExtension, Window, PositionPoint>("Position"); 
    public static PositionPoint GetPosition(AvaloniaObject d) => d.CheckNotNull(nameof(d)).GetValue(PositionProperty);
    public static void SetPosition(AvaloniaObject d, PositionPoint value) => d.CheckNotNull(nameof(d)).SetValue(PositionProperty, value);
    private static void OnPositionChanged(AvaloniaPropertyChangedEventArgs<PositionPoint> e)
    {
        if (e.Sender is Window p)
        {
            p.Position = e.NewValue.Value.ToPixelPoint();
        }
    }
    
    public struct PositionPoint
    {
        public PositionPoint() : this(0, 0) { }

        public PositionPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public PositionPoint(PixelPoint point)
        {
            X = point.X;
            Y = point.Y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public PixelPoint ToPixelPoint() => new PixelPoint(X, Y);
    }
    
    // Note: Window.Position doesn't support binding by default 
    // https://github.com/AvaloniaUI/Avalonia/pull/3521#issuecomment-582243868
}

using Avalonia;
using Avalonia.Layout;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.Common.Avalonia;

public class SizeExtension : AvaloniaObject
{
    static SizeExtension()
    {
        TrackSizeProperty.Changed.Subscribe(OnTrackSizeChanged);
        WidthProperty.Changed.Subscribe(OnWidthChanged);
        HeightProperty.Changed.Subscribe(OnHeightChanged);
    }
    
    // Allows binding the window size.
    public static readonly AttachedProperty<bool> TrackSizeProperty =
        AvaloniaProperty.RegisterAttached<SizeExtension, Layoutable, bool>("TrackSize"); 
    public static bool GetTrackSize(AvaloniaObject d) => d.CheckNotNull(nameof(d)).GetValue(TrackSizeProperty);
    public static void SetTrackSize(AvaloniaObject d, bool value) => d.CheckNotNull(nameof(d)).SetValue(TrackSizeProperty, value);
    private static void OnTrackSizeChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is Layoutable p)
        {
            p.Width = GetWidth(p);
            p.Height = GetHeight(p);
            p.GetObservable(Layoutable.WidthProperty).Subscribe((newValue) => SetWidth(p, newValue));
            p.GetObservable(Layoutable.HeightProperty).Subscribe((newValue) => SetHeight(p, newValue));
        }
    }
    
    public static readonly AttachedProperty<double> WidthProperty =
        AvaloniaProperty.RegisterAttached<SizeExtension, Layoutable, double>("Width"); 
    public static double GetWidth(AvaloniaObject d) => d.CheckNotNull(nameof(d)).GetValue(WidthProperty);
    public static void SetWidth(AvaloniaObject d, double value) => d.CheckNotNull(nameof(d)).SetValue(WidthProperty, value);
    private static void OnWidthChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        if (e.Sender is Layoutable p)
        {
            p.Width = e.NewValue.Value;
        }
    }
    
    public static readonly AttachedProperty<double> HeightProperty =
        AvaloniaProperty.RegisterAttached<SizeExtension, Layoutable, double>("Height"); 
    public static double GetHeight(AvaloniaObject d) => d.CheckNotNull(nameof(d)).GetValue(HeightProperty);
    public static void SetHeight(AvaloniaObject d, double value) => d.CheckNotNull(nameof(d)).SetValue(HeightProperty, value);
    private static void OnHeightChanged(AvaloniaPropertyChangedEventArgs<double> e)
    {
        if (e.Sender is Layoutable p)
        {
            p.Height = e.NewValue.Value;
        }
    }
    
    
    // Note: Window.Position doesn't support binding by default 
    // https://github.com/AvaloniaUI/Avalonia/pull/3521#issuecomment-582243868
}

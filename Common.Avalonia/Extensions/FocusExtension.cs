using System;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace HanumanInstitute.Common.Avalonia;

/// <summary>
/// Allows setting focus via binding.
/// </summary>
public class FocusExtensions : AvaloniaObject
{
    static FocusExtensions()
    {
        IsFocusedProperty.Changed.Subscribe(OnIsFocusedChanged);
        FocusFirstProperty.Changed.Subscribe(OnFocusFirstPropertyChanged);
        FocusOnLoadedProperty.Changed.Subscribe(OnFocusOnLoadedChanged);
        FocusOnHoverProperty.Changed.Subscribe(OnFocusOnHoverChanged);
        SelectAllOnFocusProperty.Changed.Subscribe(OnSelectAllOnFocusChanged);
    }

    // IsFocused
    public static readonly AttachedProperty<bool> IsFocusedProperty =
        AvaloniaProperty.RegisterAttached<FocusExtensions, InputElement, bool>("IsFocused");
    public static bool GetIsFocused(AvaloniaObject d) => d.CheckNotNull(nameof(d)).GetValue(IsFocusedProperty);
    public static void SetIsFocused(AvaloniaObject d, bool value) => d.CheckNotNull(nameof(d)).SetValue(IsFocusedProperty, value);
    private static void OnIsFocusedChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is InputElement p)
        {
            if (e.NewValue.Value)
            {
                // To set false value to get focus on control. if we don't set value to False then we have to set all binding
                //property to first False then True to set focus on control.
                SetIsFocused(p, false);
                p.Focus(); // Don't care about false values.
            }
        }
    }

    // FocusFirst, activates the first control when window loads.
    public static readonly AttachedProperty<bool> FocusFirstProperty =
        AvaloniaProperty.RegisterAttached<FocusExtensions, TopLevel, bool>("FocusFirst");
    public static bool GetFocusFirst(AvaloniaObject control) => control.CheckNotNull(nameof(control)).GetValue(FocusFirstProperty);
    public static void SetFocusFirst(AvaloniaObject control, bool value) => control.CheckNotNull(nameof(control)).SetValue(FocusFirstProperty, value);
    private static void OnFocusFirstPropertyChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is not TopLevel control) { return; }

        if (e.NewValue.Value)
        {
            control.Opened += (_, _) =>
            {
                IInputElement? next = control;
                next = KeyboardNavigationHandler.GetNext(next, NavigationDirection.Next);
                if (next != null)
                {
                    FocusManager.Instance?.Focus(next, NavigationMethod.Directional);
                }
            };
        }
    }

    // FocusOnLoaded
    public static readonly AttachedProperty<bool> FocusOnLoadedProperty =
        AvaloniaProperty.RegisterAttached<FocusExtensions, InputElement, bool>("FocusOnLoaded");
    public static bool GetFocusOnLoaded(AvaloniaObject d) => d.CheckNotNull(nameof(d)).GetValue(FocusOnLoadedProperty);
    public static void SetFocusOnLoaded(AvaloniaObject d, bool value) => d.CheckNotNull(nameof(d)).SetValue(FocusOnLoadedProperty, value);
    private static void OnFocusOnLoadedChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is InputElement element && e.NewValue.Value)
        {
            element.AttachedToVisualTree += (_, _) => element.Focus();
        }
    }

    // FocusOnHover
    public static readonly AttachedProperty<bool> FocusOnHoverProperty =
        AvaloniaProperty.RegisterAttached<FocusExtensions, InputElement, bool>("FocusOnHover");
    public static bool GetFocusOnHover(AvaloniaObject d) => d.CheckNotNull(nameof(d)).GetValue(FocusOnHoverProperty);
    public static void SetFocusOnHover(AvaloniaObject d, bool value) => d.CheckNotNull(nameof(d)).SetValue(FocusOnHoverProperty, value);
    private static void OnFocusOnHoverChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is InputElement element && e.NewValue.Value)
        {
            element.Focus();
        }
    }

    // SelectAllOnFocus
    public static readonly AttachedProperty<bool> SelectAllOnFocusProperty =
        AvaloniaProperty.RegisterAttached<FocusExtensions, InputElement, bool>("SelectAllOnFocus");
    public static bool GetSelectAllOnFocus(AvaloniaObject d) => d.CheckNotNull(nameof(d)).GetValue(SelectAllOnFocusProperty);
    public static void SetSelectAllOnFocus(AvaloniaObject d, bool value) => d.CheckNotNull(nameof(d)).SetValue(SelectAllOnFocusProperty, value);
    private static void OnSelectAllOnFocusChanged(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (e.Sender is InputElement element && e.NewValue.Value)
        {
            element.GotFocus += (sender, _) =>
            {
                if (sender is TextBox txt)
                {
                    txt.SelectAll();
                }
            };
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using HanumanInstitute.CommonServices;

// https://stackoverflow.com/a/31272370/3960200
// License: Attribution-ShareAlike 3.0 Unported https://creativecommons.org/licenses/by-sa/3.0/
// Did some code layout refactoring.
namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Allows setting focus via binding.
    /// </summary>
    public static class FocusExtensions
    {
        // IsFocused
        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached("IsFocused", typeof(bool),
            typeof(FocusExtensions), new UIPropertyMetadata(false, OnIsFocusedPropertyChanged));
        public static bool GetIsFocused(DependencyObject d) => (bool)d.CheckNotNull(nameof(d)).GetValue(IsFocusedProperty);
        public static void SetIsFocused(DependencyObject d, bool value) => d.CheckNotNull(nameof(d)).SetValue(IsFocusedProperty, value);
        private static void OnIsFocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control p)
            {
                if ((bool)e.NewValue)
                {
                    // To set false value to get focus on control. if we don't set value to False then we have to set all binding
                    //property to first False then True to set focus on control.
                    SetIsFocused(p, false);
                    p.Focus(); // Don't care about false values.
                }
            }
        }

        // FocusFirst, activates the first control when window loads.
        public static readonly DependencyProperty FocusFirstProperty = DependencyProperty.RegisterAttached("FocusFirst", typeof(bool),
            typeof(FocusExtensions), new PropertyMetadata(false, OnFocusFirstPropertyChanged));
        public static bool GetFocusFirst(Control control) => (bool)control.CheckNotNull(nameof(control)).GetValue(FocusFirstProperty);
        public static void SetFocusFirst(Control control, bool value) => control.CheckNotNull(nameof(control)).SetValue(FocusFirstProperty, value);
        static void OnFocusFirstPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (!(d is Control control) || !(args.NewValue is bool)) { return; }

            if ((bool)args.NewValue)
            {
                control.Loaded += (sender, e) =>
                    control.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }


        public static readonly DependencyProperty FocusOnLoadedProperty = DependencyProperty.RegisterAttached(
            "FocusOnLoaded", typeof(bool), typeof(FocusExtensions), new PropertyMetadata(false, OnFocusOnLoadedChanged));
        public static bool GetFocusOnLoaded(DependencyObject d) => (bool)d.CheckNotNull(nameof(d)).GetValue(FocusOnLoadedProperty);
        public static void SetFocusOnLoaded(DependencyObject d, bool value) => d.CheckNotNull(nameof(d)).SetValue(FocusOnLoadedProperty, value);
        private static void OnFocusOnLoadedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element && (bool)e.NewValue == true)
            {
                element.Loaded += (s, a) => element.Focus();
            }
        }


        public static readonly DependencyProperty FocusOnHoverProperty = DependencyProperty.RegisterAttached(
            "FocusOnHover", typeof(bool), typeof(FocusExtensions), new PropertyMetadata(false, OnFocusOnHoverChanged));

        public static bool GetFocusOnHover(DependencyObject d) => (bool)d.CheckNotNull(nameof(d)).GetValue(FocusOnHoverProperty);
        public static void SetFocusOnHover(DependencyObject d, bool value) => d.CheckNotNull(nameof(d)).SetValue(FocusOnHoverProperty, value);
        private static void OnFocusOnHoverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element && (bool)e.NewValue == true)
            {
                element.Focus();
            }
        }


        public static readonly DependencyProperty SelectOnHoverProperty = DependencyProperty.RegisterAttached(
            "SelectOnHover", typeof(bool), typeof(FocusExtensions), new PropertyMetadata(false, OnSelectOnHoverChanged));
        public static bool GetSelectOnHover(DependencyObject d) => (bool)d.CheckNotNull(nameof(d)).GetValue(SelectOnHoverProperty);
        public static void SetSelectOnHover(DependencyObject d, bool value) => d.CheckNotNull(nameof(d)).SetValue(SelectOnHoverProperty, value);
        private static void OnSelectOnHoverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element && (bool)e.NewValue == true)
            {
                Selector.SetIsSelected(element, true);
            }
        }


        public static readonly DependencyProperty SelectAllOnFocusProperty = DependencyProperty.RegisterAttached(
            "SelectAllOnFocus", typeof(bool), typeof(FocusExtensions), new PropertyMetadata(false, OnSelectAllOnFocusChanged));
        public static bool GetSelectAllOnFocus(DependencyObject d) => (bool)d.CheckNotNull(nameof(d)).GetValue(SelectOnHoverProperty);
        public static void SetSelectAllOnFocus(DependencyObject d, bool value) => d.CheckNotNull(nameof(d)).SetValue(SelectOnHoverProperty, value);
        private static void OnSelectAllOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element && (bool)e.NewValue == true)
            {
                element.GotFocus += (sender, args) =>
                {
                    if (sender is TextBox)
                    {
                        ((TextBox)sender).SelectAll();
                    }
                    else if (sender is PasswordBox)
                    {
                        ((PasswordBox)sender).SelectAll();
                    }
                };
            }
        }
    }
}

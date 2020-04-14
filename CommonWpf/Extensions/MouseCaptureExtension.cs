using System.Windows;
using System.Windows.Controls;

// Author: Emergence Guardian
// License: Attribution-ShareAlike 3.0 Unported https://creativecommons.org/licenses/by-sa/3.0/

namespace HanumanInstitute.CommonWpf {
    /// <summary>
    /// Captures the mouse causing it to lose focus when clicking anywhere outside the control.
    /// </summary>
    public static class MouseCaptureExtension {
        // HasCapture
        public static readonly DependencyProperty HasCaptureProperty = DependencyProperty.RegisterAttached("HasCapture", typeof(bool), 
            typeof(MouseCaptureExtension),  new UIPropertyMetadata(false, OnHasCaptureChanged));
        public static bool GetHasCapture(DependencyObject obj) => (bool)obj.GetValue(HasCaptureProperty);
        public static void SetHasCapture(DependencyObject obj, bool value) => obj.SetValue(HasCaptureProperty, value);
        private static void OnHasCaptureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (!(d is Control P))
                return;

            if ((bool)e.NewValue == true) {
                P.CaptureMouse();
                P.LostFocus += Control_LostFocus;
            } else {
                P.ReleaseMouseCapture();
                P.LostFocus -= Control_LostFocus;
            }
        }

        private static void Control_LostFocus(object sender, RoutedEventArgs e) {
            SetHasCapture(sender as Control, false);
        }
    }
}

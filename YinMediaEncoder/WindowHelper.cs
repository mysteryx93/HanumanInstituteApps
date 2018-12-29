using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using EmergenceGuardian.WpfCommon;

namespace YinMediaEncoder {
    public class WindowHelper {
        private static ImageSource Icon { get; set; }
        private Window window;

        public WindowHelper(Window window) {
            this.window = window;

            // All windows will get the icon of the first window.
            if (Icon == null)
                Icon = window.Icon;
            else {
                // Set the icon.
                window.Icon = Icon;
                window.UseLayoutRounding = true;
                TextOptions.SetTextFormattingMode(window, TextFormattingMode.Display);
                TextOptions.SetTextRenderingMode(window, TextRenderingMode.ClearType);
            }

            // Set vertical content align for all textboxes
            window.Loaded += delegate (object sender, RoutedEventArgs e) {
                foreach (TextBox item in window.FindVisualChildren<TextBox>()) {
                    if (item.VerticalScrollBarVisibility != ScrollBarVisibility.Visible)
                        item.VerticalContentAlignment = VerticalAlignment.Center;
                }
            };

            // Set the focus to the first control.
            window.SourceInitialized += (sender, e) => {
                window.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            };
        }
    }
}

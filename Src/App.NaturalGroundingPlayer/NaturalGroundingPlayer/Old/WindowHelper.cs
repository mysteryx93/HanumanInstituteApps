using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Business;
using System.Windows.Controls;

namespace NaturalGroundingPlayer {
    public class WindowHelper {
        private Window window;
        private double currentZoom;

        public WindowHelper(Window window) {
            this.window = window;

            // Set the icon.
            window.Icon = SessionCore.Instance.Icon;
            window.UseLayoutRounding = true;

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
                currentZoom = 1;
                if (Settings.Zoom != 1)
                    Settings_Changed(null, null);
            };

            Settings.SettingsChanged += Settings_Changed;
        }

        void Settings_Changed(object sender, EventArgs e) {
            if (Settings.Zoom != currentZoom) {
                var titleHeight = SystemParameters.WindowCaptionHeight + SystemParameters.ResizeFrameHorizontalBorderHeight;
                var verticalBorderWidth = SystemParameters.ResizeFrameVerticalBorderWidth;

                // Set the zoom.
                Panel TopControl = window.Content as Panel;
                window.Width = window.Width + TopControl.ActualWidth * (Settings.Zoom - currentZoom);
                window.Height = window.Height + TopControl.ActualHeight * (Settings.Zoom - currentZoom);
                SetScale(TopControl);

                // Set the zoom for ContextMenu objects.
                foreach (var item in window.FindVisualChildren<Button>()) {
                    if (item.ContextMenu != null)
                        SetScale(item.ContextMenu);
                }

                currentZoom = Settings.Zoom;
            }
        }

        public static void SetScale(FrameworkElement obj) {
            obj.LayoutTransform = new ScaleTransform(Settings.Zoom, Settings.Zoom);
        }
    }
}

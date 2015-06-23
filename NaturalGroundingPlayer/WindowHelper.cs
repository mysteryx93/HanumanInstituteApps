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
                foreach (var item in FindVisualChildren<Button>(window)) {
                    if (item.ContextMenu != null)
                        SetScale(item.ContextMenu);
                }

                currentZoom = Settings.Zoom;
            }
        }

        /// <summary>
        /// Returns the list of child objects of specified type.
        /// </summary>
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject {
            if (depObj != null) {
                foreach (var child in LogicalTreeHelper.GetChildren(depObj)) {
                    if (child != null && child is T) {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child as DependencyObject)) {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static void SetScale(FrameworkElement obj) {
            obj.LayoutTransform = new ScaleTransform(Settings.Zoom, Settings.Zoom);
        }
    }
}

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LinqToVisualTree;

// https://stackoverflow.com/a/40069088/3960200
// License: Attribution-ShareAlike 3.0 Unported https://creativecommons.org/licenses/by-sa/3.0/

namespace HanumanInstitute.CommonWpf {
    /// <summary>
    /// Adds a MaxZoomFactor attached property to the Viewbox.
    /// </summary>
    public static class ViewboxExtensions {
        public static readonly DependencyProperty MaxZoomFactorProperty =
            DependencyProperty.RegisterAttached("MaxZoomFactor", typeof(double), typeof(ViewboxExtensions), new PropertyMetadata(1.0, OnMaxZoomFactorChanged));

        private static void OnMaxZoomFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var viewbox = d as Viewbox;
            if (viewbox == null)
                return;
            viewbox.Loaded += OnLoaded;
        }

        private static void OnLoaded(object sender, RoutedEventArgs e) {
            var viewbox = sender as Viewbox;
            var child = viewbox?.Child as FrameworkElement;
            if (child == null)
                return;

            child.SizeChanged += (o, args) => CalculateMaxSize(viewbox);
            CalculateMaxSize(viewbox);
        }

        private static void CalculateMaxSize(Viewbox viewbox) {
            var child = viewbox.Child as FrameworkElement;
            if (child == null)
                return;
            viewbox.MaxWidth = child.ActualWidth * GetMaxZoomFactor(viewbox);
            viewbox.MaxHeight = child.ActualHeight * GetMaxZoomFactor(viewbox);
        }

        public static void SetMaxZoomFactor(DependencyObject d, double value) {
            d.SetValue(MaxZoomFactorProperty, value);
        }

        public static double GetMaxZoomFactor(DependencyObject d) {
            return (double)d.GetValue(MaxZoomFactorProperty);
        }
    }
}

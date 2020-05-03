using System.Windows;
using System.Windows.Controls;
using HanumanInstitute.CommonServices;

// https://stackoverflow.com/a/40069088/3960200
// License: Attribution-ShareAlike 3.0 Unported https://creativecommons.org/licenses/by-sa/3.0/

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Adds a MaxZoomFactor attached property to the Viewbox.
    /// </summary>
    public static class ViewboxExtensions
    {
        public static readonly DependencyProperty MaxZoomFactorProperty = DependencyProperty.RegisterAttached("MaxZoomFactor", typeof(double), typeof(ViewboxExtensions),
            new PropertyMetadata(1.0, OnMaxZoomFactorChanged));
        public static void SetMaxZoomFactor(DependencyObject d, double value) => d.CheckNotNull(nameof(d)).SetValue(MaxZoomFactorProperty, value);
        public static double GetMaxZoomFactor(DependencyObject d) => (double)d.CheckNotNull(nameof(d)).GetValue(MaxZoomFactorProperty);
        private static void OnMaxZoomFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Viewbox viewbox)
            {
                viewbox.Loaded += OnLoaded;
            }
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var viewbox = (Viewbox)sender;
            if (viewbox.Child is FrameworkElement child)
            {
                child.SizeChanged += (o, args) => CalculateMaxSize(viewbox);
                CalculateMaxSize(viewbox);
            }
        }

        private static void CalculateMaxSize(Viewbox viewbox)
        {
            if (viewbox.Child is FrameworkElement child)
            {
                viewbox.MaxWidth = child.ActualWidth * GetMaxZoomFactor(viewbox);
                viewbox.MaxHeight = child.ActualHeight * GetMaxZoomFactor(viewbox);
            }
        }
    }
}

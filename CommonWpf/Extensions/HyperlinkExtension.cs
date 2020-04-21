using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Opens NavigateUri in a browser on click.
    /// </summary>
    public static class HyperlinkExtensions
    {
#pragma warning disable CA1062 // Validate arguments of public methods
        public static readonly DependencyProperty NavigateInBrowserProperty =
            DependencyProperty.RegisterAttached("NavigateInBrowser", typeof(bool), typeof(HyperlinkExtensions), new PropertyMetadata(false, NavigateInBrowserChanged));
        public static bool GetNavigateInBrowser(DependencyObject d) => (bool)d.GetValue(NavigateInBrowserProperty);
        public static void SetNavigateInBrowser(DependencyObject d, bool value) => d.SetValue(NavigateInBrowserProperty, value);
#pragma warning restore CA1062 // Validate arguments of public methods

        private static void NavigateInBrowserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Hyperlink link)
            {
                if ((bool)e.NewValue == true)
                {
                    link.RequestNavigate += OnRequestNavigate;
                }
                else
                {
                    link.RequestNavigate -= OnRequestNavigate;
                }
            }
        }

        private static void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Uri.AbsoluteUri))
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            }
        }
    }
}

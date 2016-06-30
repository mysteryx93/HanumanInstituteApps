using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Business;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for MediaEncoderDeshakerWindow.xaml
    /// </summary>
    public partial class MediaEncoderDeshakerWindow : Window {
        public static void Instance(MediaEncoderBusiness business, MediaEncoderSettings settings) {
            MediaEncoderDeshakerWindow NewForm = new MediaEncoderDeshakerWindow();
            NewForm.business = business;
            NewForm.encodeSettings = settings;
            SessionCore.Instance.Windows.ShowDialog(NewForm);
        }

        private WindowHelper helper;
        MediaEncoderBusiness business;
        MediaEncoderSettings encodeSettings;
        MediaEncoderDeshakerSettings bindingSettings;

        public MediaEncoderDeshakerWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Dictionary<EdgeCompensationMethods, string> EdgeList = new Dictionary<EdgeCompensationMethods, string>();
            EdgeList.Add(EdgeCompensationMethods.None, "None (large borders)");
            EdgeList.Add(EdgeCompensationMethods.AdaptiveZoomAverage, "Adaptive zoom average (some borders)");
            EdgeList.Add(EdgeCompensationMethods.AdaptiveZoomFull, "Adaptive zoom full (no borders)");
            EdgeList.Add(EdgeCompensationMethods.FixedZoom, "Fixed zoom (no borders)");
            EdgeList.Add(EdgeCompensationMethods.AdaptiveZoomAverageFixedZoom, "Adaptive zoom average + fixed zoom (no borders)");
            EdgeCompensationCombo.ItemsSource = EdgeList;

            if (encodeSettings.DeshakerSettings == null)
                encodeSettings.DeshakerSettings = new MediaEncoderDeshakerSettings();
            // We'll edit a copy so that we can detect changes and cancel changes
            bindingSettings = encodeSettings.DeshakerSettings.Clone();
            
            this.DataContext = bindingSettings;
        }

        private async void OkButton_Click(object sender, RoutedEventArgs e) {
            encodeSettings.DeshakerSettings = bindingSettings;
            Close();
            if (MessageBox.Show("Would you like to prescan the video now?", "Prescan", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                await business.GenerateDeshakerLog(encodeSettings, business.GetPreviewSourceFile(encodeSettings));
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void HelpLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using NaturalGroundingPlayer;
using Business;

namespace YinMediaEncoder {
    /// <summary>
    /// Interaction logic for DeshakerWindow.xaml
    /// </summary>
    public partial class DeshakerAdvancedWindow : Window {
        public static void Instance(MediaEncoderBusiness business, MediaEncoderDeshakerSegmentSettings settings) {
            DeshakerAdvancedWindow NewForm = new DeshakerAdvancedWindow();
            NewForm.business = business;
            NewForm.encodeSettings = settings;
            SessionCore.Instance.Windows.ShowDialog(NewForm);
        }

        private WindowHelper helper;
        MediaEncoderBusiness business;
        MediaEncoderDeshakerSegmentSettings encodeSettings;
        MediaEncoderDeshakerSegmentSettings bindingSettings;

        public DeshakerAdvancedWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Dictionary<DeshakerScales, string> ScaleList = new Dictionary<DeshakerScales, string>();
            ScaleList.Add(DeshakerScales.Full, "Full (most precise)");
            ScaleList.Add(DeshakerScales.Half, "Half");
            ScaleList.Add(DeshakerScales.Quarter, "Quarter (fastest)");
            ScaleCombo.ItemsSource = ScaleList;

            Dictionary<DeshakerUsePixels, string> UsePixelsList = new Dictionary<DeshakerUsePixels, string>();
            UsePixelsList.Add(DeshakerUsePixels.All, "All (most robust)");
            UsePixelsList.Add(DeshakerUsePixels.Every4, "Every 4th");
            UsePixelsList.Add(DeshakerUsePixels.Every9, "Every 9th");
            UsePixelsList.Add(DeshakerUsePixels.Every16, "Every 16th (fastest)");
            UsePixelsCombo.ItemsSource = UsePixelsList;

            // We'll edit a copy so that we can detect changes and cancel changes
            bindingSettings = encodeSettings.Clone();
            
            this.DataContext = bindingSettings;
        }

        private void HelpLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            if (Validate()) {
                bindingSettings.CopyTo(encodeSettings);
                Close();
            }
        }

        private bool Validate() {
            bool Error = !this.IsValid();
            if (Error)
                MessageBox.Show(this, "You must enter valid settings.", "Validation Error");
            return !Error;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}

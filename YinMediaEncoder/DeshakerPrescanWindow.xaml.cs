using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Business;
using NaturalGroundingPlayer;
using EmergenceGuardian.WpfCommon;

namespace YinMediaEncoder {
    /// <summary>
    /// Interaction logic for DeshakerWindow.xaml
    /// </summary>
    public partial class DeshakerPrescanWindow : Window {
        public static bool Instance(MediaEncoderBusiness business, MediaEncoderSettings settings) {
            DeshakerPrescanWindow NewForm = new DeshakerPrescanWindow();
            NewForm.business = business;
            NewForm.encodeSettings = settings;
            SessionCore.Instance.Windows.ShowDialog(NewForm);
            return NewForm.Result;
        }

        private WindowHelper helper;
        private MediaEncoderBusiness business;
        private MediaEncoderSettings encodeSettings;
        private MediaEncoderDeshakerSettings bindingSettings;
        internal bool Result = false;

        public DeshakerPrescanWindow() {
            InitializeComponent();
            helper = new WindowHelper(this);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            bindingSettings = encodeSettings.DeshakerSettings.Clone();
            this.DataContext = bindingSettings;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            OkButton.Focus();
            if (Validate()) {
                encodeSettings.DeshakerSettings = bindingSettings;
                Result = true;
                Close();
            }
        }

        private bool Validate() {
            bool Error = !this.IsValid();
            if (Error)
                MessageBox.Show(this, "You must enter valid settings.", "Validation Error");
            if (bindingSettings.PrescanAction == PrescanType.Preview && bindingSettings.PrescanStart.HasValue && bindingSettings.PrescanEnd.HasValue && bindingSettings.PrescanEnd < bindingSettings.PrescanStart) {
                Error = true;
                MessageBox.Show(this, "End must be greater than Start.", "Validation Error");
            }
            return !Error;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}

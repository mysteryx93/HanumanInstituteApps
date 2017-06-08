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
    public partial class DeshakerWindow : Window {
        public static bool Instance(MediaEncoderBusiness business, MediaEncoderSettings settings) {
            DeshakerWindow NewForm = new DeshakerWindow();
            NewForm.business = business;
            NewForm.encodeSettings = settings;
            SessionCore.Instance.Windows.ShowDialog(NewForm);
            return NewForm.Result;
        }

        private WindowHelper helper;
        private MediaEncoderBusiness business;
        private MediaEncoderSettings encodeSettings;
        private MediaEncoderDeshakerSettings bindingSettings;
        private MediaEncoderDeshakerSegmentSettings bindingSegmentSettings;
        public int StartFrameTextBoxValue { get; set; } = 0;
        public bool Result = false;

        public DeshakerWindow() {
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

            // We'll edit a copy so that we can detect changes and cancel changes
            bindingSettings = encodeSettings.DeshakerSettings.Clone();
            bindingSegmentSettings = bindingSettings.Segments[0];
            Pass1Grid.DataContext = bindingSegmentSettings;
            // Pass 2 will always be bound to the first item
            Pass2Grid.DataContext = bindingSettings;

            StartFramesList.ItemsSource = bindingSettings.Segments;
            StartFramesList.DisplayMemberPath = "FrameStart";
            StartFramesList.SelectedValuePath = "FrameStart";
            StartFramesList.SelectedValue = 0;
        }

        private void HelpLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
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
            return !Error;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        /// <summary>
        /// This gets called when the Deshaker process has started. Show the status window.
        /// </summary>
        //private void ExecutingDeshakePass(object sender, ExecutingProcessEventArgs e) {
        //    Application.Current.Dispatcher.Invoke(() => DeshakerPassWindow.Instance(business, encodeSettings, e.Process));
        //}

        private void AdvancedSettingsButton_Click(object sender, RoutedEventArgs e) {
            DeshakerAdvancedWindow.Instance(business, bindingSegmentSettings);
        }

        private void StartFrameChangeButton_Click(object sender, RoutedEventArgs e) {
            if (bindingSegmentSettings.FrameStart != 0 && !bindingSettings.Segments.Where(b => b.FrameStart == StartFrameTextBoxValue).Any())
                bindingSegmentSettings.FrameStart = StartFrameTextBoxValue;
        }

        private void StartFrameAddButton_Click(object sender, RoutedEventArgs e) {
            if (!bindingSettings.Segments.Where(b => b.FrameStart == StartFrameTextBoxValue).Any()) {
                bindingSegmentSettings = bindingSegmentSettings.Clone();
                bindingSegmentSettings.FrameStart = StartFrameTextBoxValue;
                bindingSettings.Segments.Add(bindingSegmentSettings);
                StartFramesList.SelectedValue = StartFrameTextBoxValue;
            }
        }

        private void StartFrameRemoveButton_Click(object sender, RoutedEventArgs e) {
            if (StartFrameTextBoxValue != 0) {
                var ItemToDelete = bindingSettings.Segments.Where(b => b.FrameStart == StartFrameTextBoxValue).FirstOrDefault();
                if (ItemToDelete != null) {
                    if (ItemToDelete == bindingSegmentSettings) {
                        StartFrameTextBoxValue = 0;
                        StartFramesList.SelectedValue = 0;
                    }
                    bindingSettings.Segments.Remove(ItemToDelete);
                }
            }
        }

        private void StartFramesList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            bindingSegmentSettings = (MediaEncoderDeshakerSegmentSettings)StartFramesList.SelectedItem;
            Pass1Grid.DataContext = bindingSegmentSettings;
            StartFrameTextBox.Text = bindingSegmentSettings.FrameStart.ToString();
        }
    }
}

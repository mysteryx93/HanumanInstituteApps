using System;
using System.Windows;

namespace EmergenceGuardian.WpfCommon {
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class FFmpegErrorWindow : Window {
        public static void Instance(string displayTitle, string log) {
            FFmpegErrorWindow F = new FFmpegErrorWindow();
            F.Title = "Failed: " + displayTitle;
            F.OutputText.Text = log;
            F.Show();
        }

        public FFmpegErrorWindow() {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}

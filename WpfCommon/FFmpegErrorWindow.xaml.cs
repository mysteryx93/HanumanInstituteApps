using System;
using System.Text;
using System.Windows;
using EmergenceGuardian.FFmpeg;

namespace EmergenceGuardian.WpfCommon {
    /// <summary>
    /// Interaction logic for ErrorWindow.xaml
    /// </summary>
    public partial class FFmpegErrorWindow : Window {
        public static void Instance(FFmpegProcess host) {
            FFmpegErrorWindow F = new FFmpegErrorWindow();
            F.Title = "Failed: " + host.Options.DisplayTitle;
            F.OutputText.Text = host.CommandWithArgs + Environment.NewLine + Environment.NewLine + host.Output;
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

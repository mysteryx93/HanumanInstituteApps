using System;
using System.Windows;
using EmergenceGuardian.FFmpeg;

namespace EmergenceGuardian.WpfCommon {
    public class FFmpegUserInterfaceManager : IUserInterfaceManager {
        public void CreateInstance(FFmpegProcess host) {
            Application.Current.Dispatcher.Invoke(() => FFmpegWindow.Instance(host));
        }

        public void DisplayError(FFmpegProcess host) {
            Application.Current.Dispatcher.Invoke(() => FFmpegErrorWindow.Instance(host));
        }
    }
}
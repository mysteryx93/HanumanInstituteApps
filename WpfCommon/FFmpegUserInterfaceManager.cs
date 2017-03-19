using System;
using System.Windows;
using EmergenceGuardian.FFmpeg;

namespace EmergenceGuardian.WpfCommon {
    public class FFmpegUserInterfaceManager : UserInterfaceManagerBase {
        private Window parent;

        public FFmpegUserInterfaceManager(Window parent) {
            this.parent = parent;
        }

        public override IUserInterface CreateUI(string title, bool autoClose) {
            return Application.Current.Dispatcher.Invoke(() => FFmpegWindow.Instance(parent, title, autoClose));
        }

        public override void DisplayError(FFmpegProcess host) {
            Application.Current.Dispatcher.Invoke(() => FFmpegErrorWindow.Instance(parent, host));
        }
    }
}
using System;
using System.IO;
using System.Windows;
using EmergenceGuardian.FFmpeg;
using EmergenceGuardian.WpfCommon;

namespace EmergenceGuardian.YinMediaEncoder.Business {
    public class AppPaths : IAppPaths {
        private readonly string[] videoExtensions = new string[] { ".mp4", ".webm", ".avi", ".flv", ".mpg", ".mkv", ".wmv", ".tp", ".ts", ".mov", ".avs", ".m2v", ".vob" };
        private readonly string[] audioExtensions = new string[] { ".mp3", ".mp2", ".aac", ".wav", ".wma", ".m4a", ".flac" };
        private readonly string[] imageExtensions = new string[] { ".gif", ".jpg", ".png", ".bmp", ".tiff" };
        public string[] VideoExtensions => videoExtensions;
        public string[] AudioExtensions => audioExtensions;
        public string[] ImageExtensions => imageExtensions;

        public string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Yin Media Encoder\Settings.xml");
        public string UnhandledExceptionLogPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Yin Media Encoder\Log.txt");
        //public string AviSynthPluginsPath => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Encoder\");

        public string FFmpegPath => @"Encoder\ffmpeg.exe";
        public string X264Path => @"Encoder\x264-10bit.exe";
        public string Avs2yuvPath => @"Encoder\avs2yuv.exe";

        public void ConfigureFFmpegPaths(Window main) {
            FFmpegConfig.FFmpegPath = FFmpegPath;
            FFmpegConfig.Avs2yuvPath = Avs2yuvPath;
            FFmpegConfig.UserInterfaceManager = new FFmpegUserInterfaceManager(main);
            FFmpegConfig.CloseProcess += FFmpegUserInterfaceManager.CloseProcess;
        }
    }
}

using System;
using System.IO;
using System.Windows;
using EmergenceGuardian.FFmpeg;
using EmergenceGuardian.WpfCommon;

namespace EmergenceGuardian.MediaEncoder {
    public static class AppPaths {
        public static readonly string[] VideoExtensions = new string[] { ".mp4", ".webm", ".avi", ".flv", ".mpg", ".mkv", ".wmv", ".tp", ".ts", ".mov", ".avs", ".m2v", ".vob" };
        public static readonly string[] AudioExtensions = new string[] { ".mp3", ".mp2", ".aac", ".wav", ".wma", ".m4a", ".flac" };
        public static readonly string[] ImageExtensions = new string[] { ".gif", ".jpg", ".png", ".bmp", ".tiff" };

        //public static string[] GetMediaTypeExtensions(MediaType mediaType) {
        //    if (mediaType == MediaType.Video)
        //        return VideoExtensions;
        //    else if (mediaType == MediaType.Audio)
        //        return AudioExtensions;
        //    else if (mediaType == MediaType.Image)
        //        return ImageExtensions;
        //    else
        //        return null;
        //}

        public static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Natural Grounding Player\Settings.xml");
        public static readonly string UnhandledExceptionLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Natural Grounding Player\Log.txt");
        public static readonly string DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Natural Grounding Player\NaturalGroundingVideos.db");
        public static readonly string InitialDatabasePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "InitialDatabase.db");
        //public static readonly string AviSynthPluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Encoder\");
        public static readonly string Player432hzScriptFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Natural Grounding Player\432hzPlaying.avs");
        public static readonly string Player432hzConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Natural Grounding Player\432hzConfig.xml");
        public static readonly string PowerliminalsPlayerConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Natural Grounding Player\PowerliminalsConfig.xml");

        public static readonly string FFmpegPath = @"Encoder\ffmpeg.exe";
        public static readonly string X264Path = @"Encoder\x264-10bit.exe";
        public static readonly string Avs2yuvPath = @"Encoder\avs2yuv.exe";

        public static void ConfigureFFmpegPaths(Window main) {
            FFmpegConfig.FFmpegPath = FFmpegPath;
            FFmpegConfig.Avs2yuvPath = Avs2yuvPath;
            FFmpegConfig.UserInterfaceManager = new FFmpegUserInterfaceManager(main);
            FFmpegConfig.CloseProcess += FFmpegUserInterfaceManager.CloseProcess;
        }
    }
}

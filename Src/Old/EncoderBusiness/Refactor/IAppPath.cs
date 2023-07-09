using System;
using System.IO;
using System.Windows;
using EmergenceGuardian.FFmpeg;
using EmergenceGuardian.WpfCommon;

namespace EmergenceGuardian.YinMediaEncoder.Business {
    public interface IAppPaths {
        string[] VideoExtensions { get; }
        string[] AudioExtensions { get; }
        string[] ImageExtensions { get; }

        string SettingsPath { get; }
        string UnhandledExceptionLogPath { get; }
        //readonly string AviSynthPluginsPath { get; }

        string FFmpegPath { get; }
        string X264Path { get; }
        string Avs2yuvPath { get; }

        void ConfigureFFmpegPaths(Window main);
    }
}

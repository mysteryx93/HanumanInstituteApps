using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace Business {
    public static class Settings {
        static Settings() {
            // Change ConnectionString's DataDirectory before initiating any database connection.
            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.Personal));

            // Load settings file if present.
            try {
                SavedFile = SettingsFile.Load();
            }
            catch { }
        }

        public static SettingsFile SavedFile { get; set; }

        public static event EventHandler SettingsChanged;
        public static void RaiseSettingsChanged() {
            if (SettingsChanged != null)
                SettingsChanged(null, new EventArgs());
        }

        public static double Zoom {
            get {
                if (SavedFile != null) {
                    return SavedFile.Zoom;
                } else
                    return 1;
            }
        }

        public static string NaturalGroundingFolder {
            get { return SavedFile.NaturalGroundingFolder; }
        }

        public static string TempFilesPath {
            get { return SavedFile.NaturalGroundingFolder + @"Temp\"; }
        }

        public static string AutoPitchFile {
            get { return SavedFile.NaturalGroundingFolder + @"Player.avs"; }
        }

        //public static string DownloadsFolder = "Downloads";
        public static int SimultaneousDownloads = 2;

        public static readonly string[] VideoExtensions = new string[] { ".mp4", ".webm", ".avi", ".flv", ".mpg", ".mkv", ".wmv", ".tp", ".ts", ".mov", ".avs", ".m2v", ".vob" };
        public static readonly string[] AudioExtensions = new string[] { ".mp3", ".mp2", ".aac", ".wav", ".wma", ".m4a" };
        public static readonly string[] ImageExtensions = new string[] { ".gif", ".jpg", ".png", ".bmp", ".tiff" };

        public static readonly string SettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"Natural Grounding Player\Settings.xml");
        public static readonly string UnhandledExceptionLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"Natural Grounding Player\Log.txt");
        public static readonly string DatabasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"Natural Grounding Player\NaturalGroundingVideos.db");
        public static readonly string InitialDatabasePath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "InitialDatabase.db");
        public static readonly string AviSynthPluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Encoder\");
        public static readonly string Player432hzScriptFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"Natural Grounding Player\432hzPlaying.avs");
        public static readonly string Player432hzConfigFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"Natural Grounding Player\432hzConfig.xml");

        public static string[] GetMediaTypeExtensions(MediaType mediaType) {
            if (mediaType == MediaType.Video)
                return VideoExtensions;
            else if (mediaType == MediaType.Audio)
                return AudioExtensions;
            else if (mediaType == MediaType.Image)
                return ImageExtensions;
            else
                return null;
        }
    }
}

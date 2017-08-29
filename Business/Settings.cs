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
            AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

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

        //public static string AutoPitchFile {
        //    get { return SavedFile.NaturalGroundingFolder + @"Player.avs"; }
        //}

        //public static string DownloadsFolder = "Downloads";
        public static int SimultaneousDownloads = 2;
    }
}

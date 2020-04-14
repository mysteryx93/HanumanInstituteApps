using System;
using EmergenceGuardian.DownloadManager;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {
    /// <summary>
    /// Contains the application settings configurable by the user.
    /// </summary>
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class SettingsFile {
        public SettingsFile() { }

        /// <summary>
        /// Gets or sets the path where Natural Grounding media files are stored.
        /// </summary>
        public string NaturalGroundingFolder { get; set; }
        /// <summary>
        /// Gets or sets the path where temporary files are stored.
        /// </summary>
        public string TempPath { get; set; }
        /// <summary>
        /// Gets or sets the path where SVP is installed.
        /// </summary>
        public string SvpPath { get; set; }
        /// <summary>
        /// Gets or sets the path where MPC-HC is installed.
        /// </summary>
        public string MpcPath { get; set; }
        /// <summary>
        /// Gets or sets whether to auto-download media files.
        /// </summary>
        public bool AutoDownload { get; set; } = true;
        /// <summary>
        /// Gets or sets whether to use SVP during video playback.
        /// </summary>
        public bool EnableSvp { get; set; } = true;
        /// <summary>
        /// Gets or sets whether to use MadVR during video playback.
        /// </summary>
        public bool EnableMadVR { get; set; } = false;
        /// <summary>
        /// Gets or sets whether to change the pitch from 440hz to 432hz.
        /// </summary>
        public bool ChangeAudioPitch { get; set; } = false;
        /// <summary>
        /// Gets or sets whether to display videos in widescreen format.
        /// </summary>
        public bool Widescreen { get; set; } = false;
        /// <summary>
        /// Gets or sets which media player to use during playback.
        /// </summary>
        public MediaPlayerEnum MediaPlayerApp { get; set; } = MediaPlayerEnum.MpcHc;
        /// <summary>
        /// Gets or sets download options.
        /// </summary>
        public DownloadOptions Download { get; set; } = new DownloadOptions();
        /// <summary>
        /// Gets or sets the zoom factor to enlarge the UI.
        /// </summary>
        public double Zoom { get; set; } = 1;


        // Hidden properly allowing to change KNLMeans.device_id when encoding videos. It can only be set by editing Settings.xml
        //public int GraphicDeviceId { get; set; }


        /// <summary>
        /// Creates a copy of the SettingsFile class.
        /// </summary>
        /// <returns>The copied object.</returns>
        public SettingsFile Clone() {
            SettingsFile Result = (SettingsFile)MemberwiseClone();
            Result.Download = Download.Clone();
            return Result;
        }
    }
}

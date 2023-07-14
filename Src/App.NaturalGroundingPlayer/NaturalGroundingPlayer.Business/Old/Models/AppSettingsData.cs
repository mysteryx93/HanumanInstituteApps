using System;
using System.ComponentModel.DataAnnotations;
using HanumanInstitute.CommonServices;
using HanumanInstitute.Downloads;

namespace HanumanInstitute.NaturalGroundingPlayer.Models
{
    /// <summary>
    /// Contains the application settings configurable by the user.
    /// </summary>
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    [CustomValidation(typeof(AppSettingsData), nameof(AppSettingsData.Validate))]
    public class AppSettingsData
    {
        public AppSettingsData() { }

        /// <summary>
        /// Gets or sets the path where Natural Grounding media files are stored.
        /// </summary>
        public string NaturalGroundingFolder { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the path where temporary files are stored.
        /// </summary>
        public string TempPath { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the path where SVP is installed.
        /// </summary>
        public string SvpPath { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the path where MPC-HC is installed.
        /// </summary>
        public string MpcPath { get; set; } = string.Empty;
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
        //public MediaPlayerEnum MediaPlayerApp { get; set; } = MediaPlayerEnum.MpcHc;
        /// <summary>
        /// Gets or sets download options.
        /// </summary>
        public DownloadOptions Download { get; set; } = new DownloadOptions();
        /// <summary>
        /// Gets or sets the zoom factor to enlarge the UI.
        /// </summary>
        public double Zoom { get; set; } = 1;

        public static bool Validate(AppSettingsData data, ValidationContext context, ValidationResult result)
        {
            string? error = null;

            var fileSystem = (IFileSystemService)context.GetService(typeof(IFileSystemService));
            var environment = (IEnvironmentService)context.GetService(typeof(IEnvironmentService));

            data.NaturalGroundingFolder = data.NaturalGroundingFolder.Trim();
            data.SvpPath = data.SvpPath.Trim();
            data.MpcPath = data.MpcPath.Trim();

            if (!data.NaturalGroundingFolder.EndsWith(environment.DirectorySeparatorChar.ToString()))
            {
                data.NaturalGroundingFolder += environment.DirectorySeparatorChar;
            }

            if (data.SvpPath.Length == 0)
            {
                data.EnableSvp = false;
            }

            if (!fileSystem.Path.IsPathRooted(data.NaturalGroundingFolder))
            {
                error = "Invalid Natural Grounding folder";
            }

            //if (Value.MediaPlayerApp == MediaPlayerEnum.MpcHc)
            //{
            //    if (!_fileSystem.File.Exists(Value.MpcPath))
            //        return "Invalid MPC-HC path";
            //    if (Value.SvpPath.Length > 0 && !_fileSystem.File.Exists(Value.SvpPath))
            //        return "Invalid SVP path";
            //}

            if (error != null)
            {
                result.ErrorMessage = error;
                return false;
            }
            return true;
        }


        // Hidden properly allowing to change KNLMeans.device_id when encoding videos. It can only be set by editing Settings.xml
        //public int GraphicDeviceId { get; set; }


        /// <summary>
        /// Creates a copy of the SettingsFile class.
        /// </summary>
        /// <returns>The copied object.</returns>
        public AppSettingsData Clone()
        {
            var result = (AppSettingsData)MemberwiseClone();
            result.Download = Download.Clone();
            return result;
        }
    }
}

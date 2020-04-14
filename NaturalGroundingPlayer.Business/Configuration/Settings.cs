using System;
using EmergenceGuardian.CommonServices;

// ************************** NEED TO MOVE **************************
//public void ConfigureFFmpegPaths(Window main) {
//    FFmpegConfig.FFmpegPath = FFmpegPath;
//    //FFmpegConfig.Avs2yuvPath = Avs2yuvPath;
//    FFmpegConfig.UserInterfaceManager = new FFmpegUserInterfaceManager(main);
//    FFmpegConfig.CloseProcess += FFmpegUserInterfaceManager.CloseProcess;
//}

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    #region Interface

    public interface ISettings {
        /// <summary>
        /// Occurs when settings are changed and saved.
        /// </summary>
        event EventHandler SettingsSaved;
        /// <summary>
        /// Returns an object containing settings data.
        /// </summary>
        SettingsFile Data { get; set; }
        /// <summary>
        /// Loads settings file if present, or creates a new object with default values.
        /// </summary>
        void Load();
        /// <summary>
        /// Saves settings into an XML file.
        /// </summary>
        void Save();
        /// <summary>
        /// Returns Data.Zoom
        /// </summary>
        double Zoom { get; }
        /// <summary>
        /// Returns Data.NaturalGroundingFolder
        /// </summary>
        string NaturalGroundingFolder { get; }
        /// <summary>
        /// Validates settings and returns the error message, or null if successful.
        /// </summary>
        /// <returns>The error message, or null.</returns>
        string Validate();
    }

    #endregion

    public class Settings : ISettings {

        #region Declarations / Constructors

        /// <summary>
        /// Occurs when settings are changed and saved.
        /// </summary>
        public event EventHandler SettingsSaved;
        /// <summary>
        /// Returns an object containing settings data.
        /// </summary>
        public SettingsFile Data { get; set; }

        protected readonly IEnvironmentService environment;
        protected readonly IAppPathService appPath;
        protected readonly IRegistryService registry;
        protected readonly IFileSystemService fileSystem;
        protected readonly ISerializationService serialization;

        public Settings() : this(new EnvironmentService(), new AppPathService(), new RegistryService(), new FileSystemService(), new SerializationService()) { }

        public Settings(IEnvironmentService environmentService, IAppPathService appPathService, IRegistryService registryService, IFileSystemService fileSystemService, ISerializationService serializationService) {
            this.environment = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
            this.appPath = appPathService ?? throw new ArgumentNullException(nameof(appPathService));
            this.registry = registryService ?? throw new ArgumentNullException(nameof(registryService));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            this.serialization = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
        }

        #endregion

        /// <summary>
        /// Loads settings file if present, or creates a new object with default values.
        /// </summary>
        public void Load() {
            try {
                Data = serialization.DeserializeFromFile<SettingsFile>(appPath.SettingsPath);
                if (Data.Zoom < 1)
                    Data.Zoom = 1;
                else if (Data.Zoom > 1.5)
                    Data.Zoom = 1.5;
            } catch {
                Data = new SettingsFile();
                SetDefaultValues();
            }
        }

        /// <summary>
        /// Saves settings into an XML file.
        /// </summary>
        public void Save() {
            if (Validate() != null)
                throw new Exception("Cannot save settings because of validation errors.");

            serialization.SerializeToFile<SettingsFile>(Data, appPath.SettingsPath);
            SettingsSaved?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Returns Data.Zoom
        /// </summary>
        public double Zoom => Data?.Zoom ?? 1;

        /// <summary>
        /// Returns Data.NaturalGroundingFolder
        /// </summary>
        public string NaturalGroundingFolder => Data?.NaturalGroundingFolder;

        /// <summary>
        /// Validates settings and returns the error message, or null if successful.
        /// </summary>
        /// <returns>The error message, or null.</returns>
        public string Validate() {
            Data.NaturalGroundingFolder = Data.NaturalGroundingFolder.Trim();
            Data.SvpPath = Data.SvpPath.Trim();
            Data.MpcPath = Data.MpcPath.Trim();

            if (!Data.NaturalGroundingFolder.EndsWith(environment.DirectorySeparatorChar.ToString()))
                Data.NaturalGroundingFolder += environment.DirectorySeparatorChar;
            if (Data.SvpPath.Length == 0)
                Data.EnableSvp = false;
            if (!fileSystem.Path.IsPathRooted(NaturalGroundingFolder))
                return "Invalid Natural Grounding folder";

            if (Data.MediaPlayerApp == MediaPlayerEnum.MpcHc) {
                if (!fileSystem.File.Exists(Data.MpcPath))
                    return "Invalid MPC-HC path";
                if (Data.SvpPath.Length > 0 && !fileSystem.File.Exists(Data.SvpPath))
                    return "Invalid SVP path";
            }

            return null;
        }

        /// <summary>
        /// Sets default values for this computer.
        /// </summary>
        private void SetDefaultValues() {
            Data.NaturalGroundingFolder = appPath.DefaultNaturalGroundingFolder;
            // Auto-detect SVP path.
            Data.SvpPath = appPath.GetDefaultSvpPath();
            // Auto-detect MPC-HC path.
            Data.MpcPath = registry.MpcExePath;
        }
    }
}

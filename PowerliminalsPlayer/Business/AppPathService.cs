using System;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.PowerliminalsPlayer.Business
{
    /// <summary>
    /// Manages the file system paths used by the application.
    /// </summary>
    public class AppPathService : IAppPathService
    {

        protected readonly IEnvironmentService environment;
        protected readonly IFileSystemService fileSystem;

        // public AppPathService() : this(new EnvironmentService(), new FileSystemService()) { }

        public AppPathService(IEnvironmentService environmentService, IFileSystemService fileSystemService)
        {
            this.environment = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        /// <summary>
        /// Returns all valid audio extensions
        /// </summary>
        public string[] AudioExtensions => audioExtensions ?? (audioExtensions = new string[] { ".mp3", ".mp2", ".aac", ".wav", ".wma", ".m4a", ".flac" });
        private string[] audioExtensions;

        /// <summary>
        /// Returns the path where the Powerliminals Player settings file is stored.
        /// </summary>
        public string SettingsPath => fileSystem.Path.Combine(environment.CommonApplicationDataPath, @"Natural Grounding Player\PowerliminalsConfig.xml");
        /// <summary>
        /// Returns the path where unhandled exceptions are logged.
        /// </summary>
        public string UnhandledExceptionLogPath => fileSystem.Path.Combine(environment.CommonApplicationDataPath, @"Natural Grounding Player\Log.txt");
    }
}

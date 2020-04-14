using System;
using System.Threading.Tasks;
using EmergenceGuardian.CommonServices;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    #region Interface

    /// <summary>
    /// Ensures database is available and up-to-date.
    /// </summary>
    public interface IDatabaseInitializer {
        /// <summary>
        /// Checks server and database availability. 
        /// If there is no database file, copy and attach initial database.
        /// Update database if it is outdated.
        /// </summary>
        /// <param name="upgradeConfirm">This delegate will be called to display a confirmation before upgrade.</param>
        /// <remarks>If connection fails, the exception must be handled by the caller.</remarks>
        Task EnsureAvailableAsync(DisplayMessageDelegate upgradeConfirm);

        /// <summary>
        /// Writes exception details into a log file.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        void LogException(Exception ex);
    }

    #endregion

    /// <summary>
    /// Ensures database is available and up-to-date.
    /// </summary>
    public class DatabaseInitializer : IDatabaseInitializer {

        #region Declarations / Constructors

        private bool isDatabaseRecreated;

        protected readonly IEnvironmentService environment;
        protected readonly IAppPathService appPath;
        protected readonly IFileSystemService fileSystem;
        protected readonly IProcessService process;
        protected readonly IVersionAccess versionAccess;

        public DatabaseInitializer() : this(new EnvironmentService(), new AppPathService(), new FileSystemService(), new ProcessService(), new VersionAccess()) { }

        public DatabaseInitializer(IEnvironmentService environmentService, IAppPathService appPathService, IFileSystemService fileSystemService, IProcessService processService, IVersionAccess versionAccess) {
            this.environment = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
            this.appPath = appPathService ?? throw new ArgumentNullException(nameof(appPathService));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            this.process = processService ?? throw new ArgumentNullException(nameof(processService));
            this.versionAccess = versionAccess ?? throw new ArgumentNullException(nameof(versionAccess));
        }

        #endregion

        /// <summary>
        /// Checks server and database availability. 
        /// If there is no database file, copy and attach initial database.
        /// Update database if it is outdated.
        /// </summary>
        /// <param name="upgradeConfirm">This delegate will be called to display a confirmation before upgrade.</param>
        /// <remarks>If connection fails, the exception must be handled by the caller.</remarks>
        public async Task EnsureAvailableAsync(DisplayMessageDelegate upgradeConfirm) {
            Version CurrentVersion = null;
            try {
                // Test query.
                CurrentVersion = await Task.Run(() => versionAccess.GetVersionInfo());
            } catch {
            }

            if (CurrentVersion == null) {
                // Check if database exists. If it doesn't, create blank database.
                if (!fileSystem.File.Exists(appPath.DatabasePath) || fileSystem.FileInfo.FromFileName(appPath.DatabasePath).Length == 0)
                    await CreateNewDatabaseAsync();

                await TryUntilTimeout(() => versionAccess.GetVersionInfo(), 10000);
            }

            // If database connection is successfull, ensure database file is up to date.
            await UpdateDatabaseAsync(upgradeConfirm);
        }

        /// <summary>
        /// Updates database if needed.
        /// </summary>
        /// <param name="upgradeConfirm">This delegate will be called to display a confirmation before upgrade.</param>
        private async Task UpdateDatabaseAsync(DisplayMessageDelegate upgradeConfirm) {
            Version databaseVersion = versionAccess.GetVersionInfo();

            if (databaseVersion < new Version(1, 4, 0, 0)) {
                if (isDatabaseRecreated)
                    throw new Exception(string.Format("InitialDatabase version {0} is outdated.", databaseVersion.ToString()));

                string Msg = string.Format("Database v{0} is outdated. Do you wish to delete it and recreate an updated database? All your personal data will be lost.", databaseVersion.ToString(3));
                
                // Ask for confirmation before upgrade.
                if (upgradeConfirm != null && upgradeConfirm.Invoke("Database Update", Msg) != true)
                //if (dialog.ShowMessageBox(ownerViewModel, Msg, "Database Update", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    throw new Exception(string.Format("Database is outdated."));

                GC.Collect();
                await TryUntilTimeout(() => fileSystem.MoveToRecycleBin(appPath.DatabasePath), 10000);
                isDatabaseRecreated = true;
                await EnsureAvailableAsync(upgradeConfirm);

                // UpdateDatabaseToVersion(backupBeforeUpdate, 0, 9, 3, 0);
            }
        }

        /// <summary>
        /// Updates database by running a script, after making an optional backup.
        /// </summary>
        /// <param name="backupBeforeUpdate">Wether to backup database.</param>
        /// <param name="major">The Major version number.</param>
        /// <param name="minor">The Minor version number.</param>
        /// <param name="build">The Build version number.</param>
        /// <param name="revision">The Revision version number.</param>
        private void UpdateDatabaseToVersion(bool backupBeforeUpdate, int major, int minor, int build, int revision) {
            // Update database.
            RunScript(string.Format("{0}.{1}.{2}.{3}.sql", major, minor, build, revision));
            versionAccess.UpdateVersionInfo(new Version(major, minor, build, revision));
        }

        /// <summary>
        /// Creates a new database.
        /// </summary>
        private async Task CreateNewDatabaseAsync() {
            // Restore initial database into Data folder.
            await Task.Run(() => fileSystem.File.Copy(appPath.InitialDatabasePath, appPath.DatabasePath));
        }

        private async Task TryUntilTimeout(Action action, int timeout) {
            DateTime StartTime = environment.Now;
            while (true) {
                try {
                    await Task.Run(action);
                    return;
                } catch {
                    if ((environment.Now - StartTime).TotalMilliseconds > timeout)
                        throw;
                }
                await Task.Delay(500);
            }
        }

        /// <summary>
        /// Runs the specified database script.
        /// </summary>
        /// <param name="scriptFile">The name of the script to execute, which must be an embedded resource in UpdateScripts folder.</param>
        private void RunScript(string scriptFile) {
            string SqlScript = GetUpdateScript("UpdateScripts." + scriptFile);
            //SqlServerManager.RunScript(SqlScript);
        }

        /// <summary>
        /// Returns the update script with specified name.
        /// </summary>
        private static string GetUpdateScript(string name) {
            return null;
            //using (Stream input = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("NaturalGroundingPlayer.Business." + name)) {
            //    using (StreamReader reader = new StreamReader(input)) {
            //        string Result = reader.ReadToEnd();
            //        return Result;
            //    }
            //}
        }

        /// <summary>
        /// Writes exception details into a log file.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        public void LogException(Exception ex) {
            fileSystem.File.WriteAllText(appPath.UnhandledExceptionLogPath, ex.ToString());
            process.StartNotePad(appPath.UnhandledExceptionLogPath);
        }
    }
}
using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using DataAccess;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Business {
    /// <summary>
    /// Ensures server and database are up and running.
    /// </summary>
    public sealed class DatabaseHandler {
        public DatabaseHandler(Window owner) {
            this.owner = owner;
        }

        private bool isDatabaseRecreated;
        private Window owner;

        /// <summary>
        /// Checks server and database availability. 
        /// If there is no database file, copy and attach initial database.
        /// Update database if it is outdated.
        /// </summary>
        /// <remarks>If connection fails, the exception must be handled by the caller.</remarks>
        public async Task EnsureAvailableAsync() {
            Version CurrentVersion = null;
            try {
                // Test query.
                CurrentVersion = await Task.Run(() => VersionAccess.GetVersionInfo());
            } catch {
            }

            if (CurrentVersion == null) {
                // Check if database exists. If it doesn't, create blank database.
                if (!File.Exists(Settings.DatabasePath) || new FileInfo(Settings.DatabasePath).Length == 0)
                    await CreateNewDatabaseAsync();

                await TryUntilTimeout(() => VersionAccess.GetVersionInfo(), 10000);
            }

            // If database connection is successfull, ensure database file is up to date.
            await UpdateDatabaseAsync();
        }

        /// <summary>
        /// Updates database if needed.
        /// </summary>
        /// <param name="backupBeforeUpdate">If true, backups database before udpating.</param>
        public async Task UpdateDatabaseAsync() {
            Version databaseVersion = VersionAccess.GetVersionInfo();

            if (databaseVersion < new Version(1, 3, 0, 0)) {
                if (isDatabaseRecreated)
                    throw new Exception(string.Format("InitialDatabase version {0} is outdated.", databaseVersion.ToString()));

                string Msg = string.Format("Database v{0} is outdated. Do you wish to delete it and recreate an updated database? All your personal data will be lost.", databaseVersion.ToString(3));
                if (MessageBox.Show(owner, Msg, "Database Update", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    throw new Exception(string.Format("Database is outdated."));

                GC.Collect();
                await TryUntilTimeout(() => FileOperationAPIWrapper.MoveToRecycleBin(Settings.DatabasePath), 10000);
                isDatabaseRecreated = true;
                await EnsureAvailableAsync();

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
            VersionAccess.UpdateVersionInfo(new Version(major, minor, build, revision));
        }

        /// <summary>
        /// Creates a new database.
        /// </summary>
        private async Task CreateNewDatabaseAsync() {
            // Restore initial database into Data folder.
            Directory.CreateDirectory(Path.GetDirectoryName(Settings.DatabasePath));
            await CopyFileAsync(Settings.InitialDatabasePath, Settings.DatabasePath);
        }

        public async Task CopyFileAsync(string sourcePath, string destinationPath) {
            using (Stream source = File.Open(sourcePath, FileMode.Open, FileAccess.Read)) {
                using (Stream destination = File.Create(destinationPath)) {
                    await source.CopyToAsync(destination);
                }
            }
        }

        private async Task TryUntilTimeout(Action action, int timeout) {
            DateTime StartTime = DateTime.Now;
            while (true) {
                try {
                    await Task.Run(action);
                    return;
                } catch {
                    if ((DateTime.Now - StartTime).TotalMilliseconds > timeout)
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
            using (Stream input = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Business." + name)) {
                using (StreamReader reader = new StreamReader(input)) {
                    string Result = reader.ReadToEnd();
                    return Result;
                }
            }
        }

        /// <summary>
        /// Writes exception details into a log file.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        public void LogException(Exception ex) {
            File.WriteAllText(Settings.UnhandledExceptionLogPath, ex.ToString());
            Process.Start("notepad.exe", Settings.UnhandledExceptionLogPath);
        }
    }
}
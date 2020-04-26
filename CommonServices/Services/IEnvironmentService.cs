using System;
using System.Collections.Generic;

namespace HanumanInstitute.CommonServices
{
    /// <summary>
    /// Provides information about the application, environment and operating system.
    /// </summary>
    public interface IEnvironmentService
    {
        /// <summary>
        /// Returns a string array containing the command-line arguments for the current process.
        /// </summary>
        IEnumerable<string> CommandLineArguments { get; }
        /// <summary>
        /// Returns the version of executing assembly.
        /// </summary>
        Version AppVersion { get; }
        /// <summary>
        /// Returns the friendly name of this application.
        /// </summary>
        string AppFriendlyName { get; }
        /// <summary>
        /// Returns the path of the system special folder CommonApplicationData.
        /// </summary>
        string CommonApplicationDataPath { get; }
        /// <summary>
        /// Returns the directory from which the application is run.
        /// </summary>
        string AppDirectory { get; }
        /// <summary>
        /// Returns the root of the drive where the operating system is installed.
        /// </summary>
        string SystemRootDirectory { get; }
        /// <summary>
        /// Returns the path where x86 Program Files are installed.
        /// </summary>
        string ProgramFilesX86 { get; }
        /// <summary>
        /// Returns the character used to separate directory levels in the file system.
        /// </summary>
        char DirectorySeparatorChar { get; }
        /// <summary>
        /// Returns the current date and time on this computer expressed as local time.
        /// </summary>
        DateTime Now { get; }
        /// <summary>
        /// Returns the current date and time on this computer expressed as UTC.
        /// </summary>
        DateTime UtcNow { get; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HanumanInstitute.Common.Services;

/// <summary>
/// Provides information about the application, environment and operating system.
/// </summary>
public class EnvironmentService : IEnvironmentService
{
    /// <summary>
    /// Returns a string array containing the command-line arguments for the current process.
    /// </summary>
    public IEnumerable<string> CommandLineArguments => Environment.GetCommandLineArgs();
    /// <summary>
    /// Returns the version of executing assembly.
    /// </summary>
    public Version AppVersion => Assembly.GetEntryAssembly()?.GetName()?.Version ?? new Version();
    /// <summary>
    /// Returns the friendly name of this application.
    /// </summary>
    public string AppFriendlyName => System.AppDomain.CurrentDomain.FriendlyName;
    /// <summary>
    /// Returns the path of the system special folder CommonApplicationData.
    /// </summary>
    public string CommonApplicationDataPath => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    /// <summary>
    /// Returns the directory from which the application is run.
    /// </summary>
    public string AppDirectory => System.AppDomain.CurrentDomain?.BaseDirectory ?? string.Empty;
    /// <summary>
    /// Returns the root of the drive where the operating system is installed.
    /// </summary>
    public string SystemRootDirectory => Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)) ?? string.Empty;
    /// <summary>
    /// Returns the path where x86 Program Files are installed.
    /// </summary>
    public string ProgramFilesX86 => Environment.GetFolderPath(Environment.Is64BitOperatingSystem ? Environment.SpecialFolder.ProgramFilesX86 : Environment.SpecialFolder.ProgramFiles);
    /// <summary>
    /// Returns the character used to separate directory levels in the file system.
    /// </summary>
    public char DirectorySeparatorChar => Path.DirectorySeparatorChar;
    /// <summary>
    /// Returns the current date and time on this computer expressed as local time.
    /// </summary>
    public DateTime Now => DateTime.Now;
    /// <summary>
    /// Returns the current date and time on this computer expressed as UTC.
    /// </summary>
    public DateTime UtcNow => DateTime.UtcNow;
}
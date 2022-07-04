
// ReSharper disable InconsistentNaming

namespace HanumanInstitute.Converter432hz.Business;

/// <summary>
/// Manages the file system paths used by the application.
/// </summary>
public interface IAppPathService
{
    /// <summary>
    /// Returns all valid audio extensions
    /// </summary>
    IReadOnlyList<string> AudioExtensions { get; }
    /// <summary>
    /// Returns the path where unhandled exceptions are logged.
    /// </summary>
    string UnhandledExceptionLogPath { get; }
    /// <summary>
    /// Returns the path where the application settings file is stored.
    /// </summary>
    string ConfigFile { get; }
    /// <summary>
    /// If a settings file is found at an old location, it will be moved to the new location. This is for compatibility with previous version.
    /// </summary>
    string OldConfigFile { get; }
}

namespace HanumanInstitute.Apps;

/// <summary>
/// Manages the file system paths used by the application.
/// </summary>
public interface IAppPathServiceBase
{
    /// <summary>
    /// Returns the Uri where unhandled exceptions are logged.
    /// </summary>
    string UnhandledExceptionLogPath { get; }
    /// <summary>
    /// Returns the Uri where AdRotator ads data are stored.
    /// </summary>
    string AdInfoPath { get; }
    /// <summary>
    /// Returns the path where application data is stored.
    /// </summary>
    string StorageFolder { get; }
    /// <summary>
    /// Returns an Uri pointing to specified file name within <see cref="StorageFolder"/>.
    /// </summary>
    string GetStoragePath(string fileName);
}


// ReSharper disable InconsistentNaming

namespace HanumanInstitute.Converter432hz.Business;

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
    /// Returns the path where the 432hz Player settings file is stored.
    /// </summary>
    string Player432hzConfigFile { get; }
}

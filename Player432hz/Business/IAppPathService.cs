using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace HanumanInstitute.Player432hz.Business;

public interface IAppPathService
{
    /// <summary>
    /// Returns all valid video extensions.
    /// </summary>
    IList<string> VideoExtensions { get; }
    /// <summary>
    /// Returns all valid audio extensions
    /// </summary>
    IList<string> AudioExtensions { get; }
    /// <summary>
    /// Returns all valid image extensions.
    /// </summary>
    IList<string> ImageExtensions { get; }
    /// <summary>
    /// Returns the path where unhandled exceptions are logged.
    /// </summary>
    string UnhandledExceptionLogPath { get; }
    /// <summary>
    /// Returns the path where the 432hz Player settings file is stored.
    /// </summary>
    string Player432hzConfigFile { get; }
}

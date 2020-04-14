using System;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.PowerliminalsPlayer.Business
{
    /// <summary>
    /// Manages the file system paths used by the application.
    /// </summary>
    public interface IAppPathService
    {
        /// <summary>
        /// Returns all valid audio extensions
        /// </summary>
        string[] AudioExtensions { get; }
        /// <summary>
        /// Returns the path where the Powerliminals Player settings file is stored.
        /// </summary>
        string SettingsPath { get; }
        /// <summary>
        /// Returns the path where unhandled exceptions are logged.
        /// </summary>
        string UnhandledExceptionLogPath { get; }
    }
}

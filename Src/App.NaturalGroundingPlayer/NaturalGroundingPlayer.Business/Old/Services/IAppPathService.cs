using System;
using System.Collections.Generic;
using HanumanInstitute.NaturalGroundingPlayer.Models;

namespace HanumanInstitute.NaturalGroundingPlayer.Services
{
    /// <summary>
    /// Manages the file system paths used by the application.
    /// </summary>
    public interface IAppPathService
    {
        /// <summary>
        /// Returns all valid video extensions.
        /// </summary>
        IEnumerable<string> VideoExtensions { get; }
        /// <summary>
        /// Returns all valid audio extensions
        /// </summary>
        IEnumerable<string> AudioExtensions { get; }
        /// <summary>
        /// Returns all valid image extensions.
        /// </summary>
        IEnumerable<string> ImageExtensions { get; }
        /// <summary>
        /// Returns all extensions for specified media type.
        /// </summary>
        /// <param name="mediaType">The type of media for which to get extensions.</param>
        /// <returns>A string array of file extensions.</returns>
        IEnumerable<string> GetMediaTypeExtensions(MediaType mediaType);
        /// <summary>
        /// Returns the path where the settings file is stored.
        /// </summary>
        string SettingsPath { get; }
        /// <summary>
        /// Returns the path where unhandled exceptions are logged.
        /// </summary>
        string UnhandledExceptionLogPath { get; }
        /// <summary>
        /// Returns the path where the database file is stored.
        /// </summary>
        string DatabasePath { get; }
        /// <summary>
        /// Returns the path where the blank initial database is stored.
        /// </summary>
        string InitialDatabasePath { get; }
        /// <summary>
        /// Returns the path where AviSynth plugins are located.
        /// </summary>
        string AvisynthPluginsPath { get; }
        /// <summary>
        /// Returns the path where the 432hz Player is storing its Avisynth script during playback.
        /// </summary>
        string Player432hzScriptFile { get; }
        /// <summary>
        /// Returns the path where the 432hz Player settings file is stored.
        /// </summary>
        string Player432hzConfigFile { get; }
        /// <summary>
        /// Returns the path where the Powerlimnals Player settings file is stored.
        /// </summary>
        string PowerliminalsPlayerConfigFile { get; }
        /// <summary>
        /// Returns the relative path to access FFmpeg.
        /// </summary>
        string FFmpegPath { get; }
        /// <summary>
        /// Returns the relative path to access the temp folder within the Natural Grounding folder.
        /// </summary>
        string LocalTempPath { get; }
        /// <summary>
        /// Returns the system temp folder.
        /// </summary>
        string SystemTempPath { get; }
        /// <summary>
        /// Returns the temp path for the media downloader.
        /// </summary>
        string DownloaderTempPath { get; }
        /// <summary>
        /// Returns the default Natural Grounding folder.
        /// </summary>
        string DefaultNaturalGroundingFolder { get; }
        /// <summary>
        /// Auto-detects SVP path.
        /// </summary>
        /// <returns>The default SVP path.</returns>
        string GetDefaultSvpPath();
        // void WriteLogFile(string content);
    }
}

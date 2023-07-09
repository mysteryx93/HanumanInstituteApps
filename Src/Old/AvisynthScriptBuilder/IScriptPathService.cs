using System;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Manages the paths used by the Avisynth or Vapoursynth scripts.
    /// </summary>
    public interface IScriptPathService
    {
        /// <summary>
        /// Gets or sets the path where Avisynth plugins are stored.
        /// </summary>
        string PluginsPath { get; set; }
        /// <summary>
        /// Returns a list of all valid audio extensions.
        /// </summary>
        string[] AudioExtensions { get; set; }
        /// <summary>
        /// Gets the newline string defined for this environment.
        /// </summary>
        string NewLine { get; }
        /// <summary>
        /// Returns the next available file name to avoid overriding an existing file.
        /// </summary>
        /// <param name="dest">The attempted destination.</param>
        /// <returns>The next available file name.</returns>
        string GetNextAvailableFileName(string dest);
        /// <summary>
        /// Returns a unique temporary file with given extension.
        /// </summary>
        /// <param name="extension">The extension of the temporary file to create.</param>
        /// <returns></returns>
        string GetTempFile(string extension);
    }
}

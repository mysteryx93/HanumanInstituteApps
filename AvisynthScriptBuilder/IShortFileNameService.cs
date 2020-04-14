using System;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Gives access to DOS short path names.
    /// </summary>
    public interface IShortFileNameService
    {
        /// <summary>
        /// Returns whether specified value contains only ASCII characters.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>True if value contains only ASCII characters, otherwise false.</returns>
        bool IsASCII(string value);
        /// <summary>
        /// Returns the short path name (DOS) for specified path.
        /// </summary>
        /// <param name="path">The path for which to get the short path.</param>
        /// <returns>The short path.</returns>
        string GetAsciiPath(string path);
    }
}

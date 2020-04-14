using System;
using System.Collections.Generic;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Facilitates the creation of Avisynth or Vapoursynth scripts.
    /// </summary>
    public interface IScriptBuilder
    {
        /// <summary>
        /// Gets or sets the script contained in this class.
        /// </summary>
        string Script { get; set; }
        /// <summary>
        /// Returns the script value.
        /// </summary>
        string ToString();
        /// <summary>
        /// Returns whether the script is empty.
        /// </summary>
        bool IsEmpty { get; }
        /// <summary>
        /// Appends a line break to the script.
        /// </summary>
        void AppendLine();
        /// <summary>
        /// Appends a line to the script following specified format. Line break is automatically added.
        /// </summary>
        /// <param name="value">The value or format to append.</param>
        /// <param name="args">If adding a format, the list of arguments.</param>
        void AppendLine(string value, params object[] args);
        /// <summary>
        /// Appends a line to the script following specified format. Line break is automatically added.
        /// </summary>
        /// <param name="culture">The culture to use while formatting.</param>
        /// <param name="value">The value or format to append.</param>
        /// <param name="args">If adding a format, the list of arguments.</param>
        void AppendLine(IFormatProvider culture, string value, params object[] args);
        /// <summary>
        /// Appends specified lines to the script.
        /// </summary>
        /// <param name="lines">The lines to append.</param>
        void AppendLine(IEnumerable<string> lines);
        /// <summary>
        /// Replaces all instances of oldValue with newValue.
        /// </summary>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace with.</param>
        void Replace(string oldValue, string newValue);
        /// <summary>
        /// Returns whether the script contains any line beginning with the specified values. The search is case-invariant.
        /// </summary>
        /// <param name="values">The list of values to search for.</param>
        /// <returns>True if any of the value was found, otherwise false.</returns>
        bool ContainsAny(string[] values);
        /// <summary>
        /// Writes the content of script into specified file path.
        /// </summary>
        /// <param name="path">The path of the file to write.</param>
        void WriteToFile(string fileName);
    }
}

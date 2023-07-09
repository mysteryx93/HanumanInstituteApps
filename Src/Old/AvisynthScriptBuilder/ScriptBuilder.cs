using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.Text;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Facilitates the creation of Avisynth or Vapoursynth scripts.
    /// </summary>
    public class ScriptBuilder : IScriptBuilder
    {
        private readonly IScriptPathService _scriptPath;
        private readonly IFileSystem _fileSystem;

        public ScriptBuilder() : this(new ScriptPathService(), new FileSystem()) { }

        public ScriptBuilder(IScriptPathService scriptPathService, IFileSystem fileSystemService)
        {
            _scriptPath = scriptPathService;
            _fileSystem = fileSystemService;
        }

        private StringBuilder _script = new StringBuilder();

        /// <summary>
        /// Gets or sets the script contained in this class.
        /// </summary>
        public string Script
        {
            get { return _script.ToString(); }
            set { _script = new StringBuilder(value); }
        }

        /// <summary>
        /// Returns the script value.
        /// </summary>
        public override string ToString()
        {
            return Script;
        }

        /// <summary>
        /// Returns whether the script is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return _script.Length == 0; }
        }

        /// <summary>
        /// Appends specified value to the script, followed by a line break.
        /// </summary>
        /// <param name="value">The value to append.</param>
        public void AppendLine(string value)
        {
            _script.AppendLine(value);
        }

        /// <summary>
        /// Appends a line break to the script.
        /// </summary>
        public void AppendLine()
        {
            _script.AppendLine();
        }

        /// <summary>
        /// Appends a line to the script following specified format. Line break is automatically added.
        /// </summary>
        /// <param name="value">The value or format to append.</param>
        /// <param name="args">If adding a format, the list of arguments.</param>
        public void AppendLine(string value, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                _script.AppendFormat(value, args).AppendLine();
            }
            else
            {
                _script.AppendLine(value);
            }
        }

        /// <summary>
        /// Appends a line to the script following specified format. Line break is automatically added.
        /// </summary>
        /// <param name="culture">The culture to use while formatting.</param>
        /// <param name="value">The value or format to append.</param>
        /// <param name="args">If adding a format, the list of arguments.</param>
        public void AppendLine(IFormatProvider culture, string value, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                _script.AppendFormat(culture, value, args).AppendLine();
            }
            else
            {
                _script.AppendLine(value);
            }
        }

        /// <summary>
        /// Appends specified lines to the script.
        /// </summary>
        /// <param name="lines">The lines to append.</param>
        public void AppendLine(IEnumerable<string>? lines)
        {
            if (lines != null)
            {
                foreach (var item in lines)
                {
                    _script.AppendLine(item);
                }
            }
        }

        /// <summary>
        /// Appends a line to the script following specified format using invariant culture. Line break is automatically added.
        /// </summary>
        /// <param name="value">The value or format to append.</param>
        /// <param name="args">If adding a format, the list of arguments.</param>
        public void AppendLineInvariant(string value, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                _script.AppendFormat(CultureInfo.InvariantCulture, value, args).AppendLine();
            }
            else
            {
                _script.AppendLine(value);
            }
        }

        /// <summary>
        /// Replaces all instances of oldValue with newValue.
        /// </summary>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace with.</param>
        public void Replace(string oldValue, string newValue)
        {
            _script.Replace(oldValue, newValue);
        }

        /// <summary>
        /// Returns whether the script contains any line beginning with the specified values. The search is case-invariant.
        /// </summary>
        /// <param name="values">The list of values to search for.</param>
        /// <returns>True if any of the value was found, otherwise false.</returns>
        public bool ContainsAny(string[] values)
        {
            values.CheckNotNull(nameof(values));

            var strScript = _script.ToString();
            foreach (var item in values)
            {
                if (strScript.IndexOf(_scriptPath.NewLine + item, StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Writes the content of script into specified file path.
        /// </summary>
        /// <param name="path">The path of the file to write.</param>
        public void WriteToFile(string path)
        {
            var folder = _fileSystem.Path.GetDirectoryName(path);
            if (!_fileSystem.Directory.Exists(folder))
            {
                _fileSystem.Directory.CreateDirectory(folder);
            }

            _fileSystem.File.WriteAllText(path, _script.ToString(), Encoding.ASCII);
        }
    }
}

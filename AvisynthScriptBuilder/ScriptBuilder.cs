using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Text;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Facilitates the creation of Avisynth or Vapoursynth scripts.
    /// </summary>
    public class ScriptBuilder : IScriptBuilder
    {
        private readonly IScriptPathService scriptPath;
        private readonly IFileSystem fileSystem;

        public ScriptBuilder() : this(new ScriptPathService(), new FileSystem()) { }

        public ScriptBuilder(IScriptPathService scriptPathService, IFileSystem fileSystemService)
        {
            this.scriptPath = scriptPathService ?? throw new ArgumentNullException(nameof(scriptPathService));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        protected StringBuilder script = new StringBuilder();

        /// <summary>
        /// Gets or sets the script contained in this class.
        /// </summary>
        public string Script {
            get { return script.ToString(); }
            set { script = new StringBuilder(value); }
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
        public bool IsEmpty {
            get { return script.Length == 0; }
        }

        /// <summary>
        /// Appends a line break to the script.
        /// </summary>
        public void AppendLine()
        {
            script.AppendLine();
        }

        /// <summary>
        /// Appends a line to the script following specified format. Line break is automatically added.
        /// </summary>
        /// <param name="value">The value or format to append.</param>
        /// <param name="args">If adding a format, the list of arguments.</param>
        public void AppendLine(string value, params object[] args)
        {
            if (args != null && args.Length > 0)
                script.AppendFormat(value, args).AppendLine();
            else
                script.AppendLine(value);
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
                script.AppendFormat(culture, value, args).AppendLine();
            else
                script.AppendLine(value);
        }

        /// <summary>
        /// Appends specified lines to the script.
        /// </summary>
        /// <param name="lines">The lines to append.</param>
        public void AppendLine(IEnumerable<string> lines)
        {
            foreach (string item in lines)
            {
                script.AppendLine(item);
            }
        }

        /// <summary>
        /// Replaces all instances of oldValue with newValue.
        /// </summary>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace with.</param>
        public void Replace(string oldValue, string newValue)
        {
            script.Replace(oldValue, newValue);
        }

        /// <summary>
        /// Returns whether the script contains any line beginning with the specified values. The search is case-invariant.
        /// </summary>
        /// <param name="values">The list of values to search for.</param>
        /// <returns>True if any of the value was found, otherwise false.</returns>
        public bool ContainsAny(string[] values)
        {
            string StrScript = script.ToString();
            foreach (string item in values)
            {
                if (StrScript.IndexOf(scriptPath.NewLine + item, StringComparison.InvariantCultureIgnoreCase) > -1)
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
            string Folder = fileSystem.Path.GetDirectoryName(path);
            if (!fileSystem.Directory.Exists(Folder))
                fileSystem.Directory.CreateDirectory(Folder);

            fileSystem.File.WriteAllText(path, script.ToString(), Encoding.ASCII);
        }
    }
}

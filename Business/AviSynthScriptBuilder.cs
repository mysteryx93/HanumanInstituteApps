using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Business {
    /// <summary>
    /// Facilitates the creation of AviSynth scripts.
    /// </summary>
    public class AviSynthScriptBuilder {
        private StringBuilder script = new StringBuilder();

        public AviSynthScriptBuilder() {
        }

        public AviSynthScriptBuilder(string script) {
            Script = script;
        }

        /// <summary>
        /// Gets or sets the script contained in this class.
        /// </summary>
        public string Script {
            get { return script.ToString(); }
            set { script = new StringBuilder(value); }
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
        public void AppendLine() {
            script.AppendLine();
        }

        /// <summary>
        /// Appends a line to the script following specified format. Line break is automatically added.
        /// </summary>
        /// <param name="value">The value or format to append.</param>
        /// <param name="args">If adding a format, the list of arguments.</param>
        public void AppendLine(string value, params object[] args) {
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
        public void AppendLine(IFormatProvider culture, string value, params object[] args) {
            if (args != null && args.Length > 0)
                script.AppendFormat(culture, value, args).AppendLine();
            else
                script.AppendLine(value);
        }

        /// <summary>
        /// Replaces all instances of oldValue with newValue.
        /// </summary>
        /// <param name="oldValue">The string to be replaced.</param>
        /// <param name="newValue">The string to replace with.</param>
        public void Replace(string oldValue, string newValue) {
            script.Replace(oldValue, newValue);
        }

        /// <summary>
        /// Adds a line containing the plugin path. Must be called before loading any other plugin.
        /// </summary>
        public void AddPluginPath() {
            AppendLine(@"PluginPath=""{0}""", GetAsciiPath(Settings.AviSynthPluginsPath));
        }

        /// <summary>
        /// Moves all LoadPlugin and Import commands to the beginning to make the script more readable.
        /// </summary>
        public void Cleanup() {
            string[] Lines = script.ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            string[] CommandsToComment = new string[] { "PluginPath", "LoadPlugin", "Import" };
            var NewScriptLines = Lines.Where(l => CommandsToComment.Any(c => l.StartsWith(c))).Concat(Lines.Where(l => !CommandsToComment.Any(c => l.StartsWith(c))));
            script = new StringBuilder(string.Join(Environment.NewLine, NewScriptLines));
        }

        /// <summary>
        /// Removes commands from the script to allow displaying it as a preview.
        /// </summary>
        public void ConvertForPreview() {
            string[] Lines = script.ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            string[] CommandsToComment = new string[] { "ColorMatrix", "SetMTMode", "Prefetch" };
            for (int i = 0; i < Lines.Length; i++) {
                if (CommandsToComment.Any(c => Lines[i].StartsWith(c)))
                    Lines[i] = "# " + Lines[i];
            }
            script = new StringBuilder(string.Join(Environment.NewLine, Lines) + Environment.NewLine + "ConvertToRGB32()");
        }

        public void LoadPluginDll(string fileName) {
            script.AppendFormat(@"LoadPlugin(PluginPath+""{0}"")", fileName).AppendLine();
        }

        public void LoadPluginAvsi(string fileName) {
            script.AppendFormat(@"Import(PluginPath+""{0}"")", fileName).AppendLine();
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string path, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath, int shortPathLength);

        private string GetShortPath(string path) {
            const int MAX_PATH = 255;
            var shortPath = new StringBuilder(MAX_PATH);
            GetShortPathName(path, shortPath, MAX_PATH);
            return shortPath.ToString();
        }

        private bool IsASCII(string value) {
            // ASCII encoding replaces non-ascii with question marks, so we use UTF8 to see if multi-byte sequences are there
            return Encoding.UTF8.GetByteCount(value) == value.Length;
        }

        public string GetAsciiPath(string path) {
            if (!IsASCII(path))
                return GetShortPath(path);
            else
                return path;
        }

        public void WriteToFile(string fileName) {
            string Folder = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            File.WriteAllText(fileName, script.ToString(), Encoding.ASCII);
        }


    }
}

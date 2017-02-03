using System;
using System.Collections.Generic;
using System.Globalization;
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
        // Store the list of loaded plugins to avoid adding duplicate.
        private List<string> loadedPluginsDll = new List<string>();
        private List<string> loadedPluginsAvsi = new List<string>();

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

        public override string ToString() {
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
        /// Appends specified lines to the script.
        /// </summary>
        /// <param name="lines">The lines to append.</param>
        public void AppendLine(IEnumerable<string> lines) {
            foreach (string item in lines) {
                script.AppendLine(item);
            }
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
        /// Returns whether the script contains any line beginning with the specified values. The search is case-invariant.
        /// </summary>
        /// <param name="values">The list of values to search for.</param>
        /// <returns>True if any of the value was found, otherwise false.</returns>
        public bool ContainsAny(string[] values) {
            string StrScript = script.ToString();
            foreach (string item in values) {
                if (StrScript.IndexOf(Environment.NewLine + item, StringComparison.InvariantCultureIgnoreCase) > -1) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds a line containing the plugin path. Must be called before loading any other plugin.
        /// </summary>
        public void AddPluginPath() {
            AppendLine(@"P=""{0}""", GetAsciiPath(Settings.AviSynthPluginsPath));
            //AppendLine(@"AddAutoloadDir(P, true)");
            //AppendLine(@"AddAutoloadDir(P+""Shaders\"", true)");
        }

        /// <summary>
        /// Moves all LoadPlugin and Import commands to the beginning to make the script more readable.
        /// </summary>
        public void Cleanup() {
            string[] Lines = script.ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            string[] CommandsToMove = new string[] { "AddAutoloadDir", "P=", "LoadPlugin", "Import", "LoadVirtualDubPlugin" };
            var NewScriptLines = Lines.Where(l => CommandsToMove.Any(c => l.StartsWith(c, StringComparison.InvariantCultureIgnoreCase))).Concat(Lines.Where(l => !CommandsToMove.Any(c => l.StartsWith(c, StringComparison.InvariantCultureIgnoreCase))));
            script = new StringBuilder(string.Join(Environment.NewLine, NewScriptLines));
        }

        /// <summary>
        /// Rewrites the script to run through MP_Pipeline.
        /// </summary>
        public void ConvertToMultiProcesses(double sourceFrameRate) {
            string[] Lines = script.ToString().TrimEnd('\r','\n').Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            script = new StringBuilder();
            AddPluginPath();
            LoadPluginDll("MP_Pipeline.dll");
            AppendLine("SetMemoryMax(1)");
            AppendLine();
            AppendLine("MP_Pipeline(\"\"\"");
            string[] CommandsToMove = new string[] { "P=", "LoadPlugin", "Import", "LoadVirtualDubPlugin" };
            string[] CommandsToIgnore = new string[] { "P=", "LoadPlugin", "Import", "LoadVirtualDubPlugin", "file=", "AudioDub(" };
            var LoadPluginLines = Lines.Where(l => CommandsToMove.Any(c => l.StartsWith(c, StringComparison.InvariantCultureIgnoreCase)));
            string FileNameLine = Lines.Where(l => l.StartsWith("file=", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            string AudioDubLine = Lines.Where(l => l.StartsWith("AudioDub(", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            string AviSourceLine = Lines.Where(l => l.StartsWith("AviSource(", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            string TrimLine = Lines.Where(l => l.StartsWith("Trim(", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            string AssumeFpsLine = Lines.Where(l => l.StartsWith("AssumeFps(", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            var OtherLines = Lines.Where(l => !CommandsToIgnore.Any(c => l.StartsWith(c, StringComparison.InvariantCultureIgnoreCase)));
            AppendLine("### inherit start ###");
            AppendLine(LoadPluginLines);
            AppendLine(FileNameLine);
            AppendLine("### inherit end ###");
            AppendLine(OtherLines);
            if (OtherLines.Last() != "### ###") {
                AppendLine("### prefetch: 3, 3"); // Keep 3 frames before and after for temporal filters.
                AppendLine("### ###");
            }
            AppendLine("\"\"\")");
            AppendLine();
            if (!string.IsNullOrEmpty(AudioDubLine) && string.IsNullOrEmpty(TrimLine) && string.IsNullOrEmpty(AssumeFpsLine)) {
                AppendLine(AudioDubLine);
            } else {
                if (string.IsNullOrEmpty(AudioDubLine))
                    AppendLine("A=" + AviSourceLine);
                else {
                    // We must create a clip with the original frame count and frame rate so that Trim and AssumeFps functions perform what they are meant to.
                    AppendLine("SourceFps=" + sourceFrameRate.ToString(CultureInfo.InvariantCulture));
                    AppendLine("A=BlankClip(1,1,1)." + AudioDubLine);
                    AppendLine(@"Blank=BlankClip(int(A.AudioDuration*SourceFps), 4, 4, ""YV12"", SourceFps)");
                    AppendLine("A=AudioDub(Blank, A)");
                }
                if (!string.IsNullOrEmpty(TrimLine))
                    AppendLine("A=A." + TrimLine);
                if (!string.IsNullOrEmpty(AssumeFpsLine))
                    AppendLine("A=A." + AssumeFpsLine);
                AppendLine("AudioDub(last, A)");
            }
        }

        /// <summary>
        /// Removes MultiThreading commands from script.
        /// </summary>
        public void RemoveMT() {
            script = script.Replace(string.Format("Cores={0},", Environment.ProcessorCount), "Cores=1,");
            string[] Lines = script.ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            string[] CommandsToComment = new string[] { "SetMTMode", "SetFilterMTMode", "Prefetch", @"Import(P+""AviSynthMT.avsi"")" };
            string[] NewLines = Lines.Where(l => !CommandsToComment.Any(c => l.StartsWith(c))).ToArray();
            script = new StringBuilder(string.Join(Environment.NewLine, NewLines));
        }

        /// <summary>
        /// If script ends with DitherPost(), replace it with Dither_out() when rawOutput is true. If rawOutput is false, it does the reverse.
        /// </summary>
        /// <param name="rawOutput">True to replace DitherPost with Dither_out, false to replace Dither_out with DitherPost.</param>
        //public void DitherOut(bool rawOutput) {
        //    string StrPost = "DitherPost()";
        //    string StrOut = "Dither_out()";
        //    string Val = Script.TrimEnd('\r', '\n', ' ');
        //    if (rawOutput && Val.EndsWith(StrPost)) {
        //        int Pos = Val.LastIndexOf(StrPost);
        //        script.Replace(StrPost, StrOut, Pos, script.Length - Pos);
        //    } else if (!rawOutput && Val.EndsWith(StrOut)) {
        //        int Pos = Val.LastIndexOf(StrOut);
        //        script.Replace(StrOut, StrPost, Pos, script.Length - Pos);
        //    }
        //}

        public void LoadPluginDll(string fileName) {
            if (!loadedPluginsDll.Contains(fileName)) {
                AppendLine(@"LoadPlugin(P+""{0}"")", fileName);
                loadedPluginsDll.Add(fileName);
            }
        }

        public void LoadPluginAvsi(string fileName) {
            if (!loadedPluginsAvsi.Contains(fileName)) {
                AppendLine(@"Import(P+""{0}"")", fileName);
                loadedPluginsAvsi.Add(fileName);
            }
        }

        public void OpenAvi(string fileName, bool audio) {
            AppendLine(@"file=""{0}""", GetAsciiPath(fileName));
            AppendLine(@"AviSource(file, audio={0}, pixel_type=""YV12"")", audio);
        }

        public void OpenDirect(string fileName, bool audio) {
            OpenDirect(fileName, audio, true);
        }

        public void OpenDirect(string fileName, bool audio, bool video) {
            // Create cache file when file size is over 500MB
            bool Cache = new FileInfo(fileName).Length >= 500 * 1024 * 1024;
            LoadPluginDll("LSMASHSource.dll");
            AppendLine(@"file=""{0}""", GetAsciiPath(fileName));
            if (video)
                AppendLine("LWLibavVideoSource(file, cache={0})", Cache);
            if (audio) {
                string AudioScript = string.Format("LWLibavAudioSource(file, cache={0})", Cache);
                if (video)
                    AppendLine("AudioDub({0})", AudioScript);
                else
                    AppendLine(AudioScript);
            }                
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

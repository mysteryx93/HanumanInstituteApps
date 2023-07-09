using System;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Facilitates the creation of AviSynth scripts.
    /// </summary>
    public class ScriptBuilderAvisynth : ScriptBuilder, IScriptBuilderAvisynth
    {
        // Store the list of loaded plugins to avoid adding duplicate.
        //private List<string> loadedPluginsDll = new List<string>();
        //private List<string> loadedPluginsAvsi = new List<string>();

        private readonly IScriptPathService scriptPath;
        private readonly IFileSystem fileSystem;
        private readonly IShortFileNameService shortFileName;

        public ScriptBuilderAvisynth() : this(new ScriptPathService(), new FileSystem(), new ShortFileNameService()) { }

        public ScriptBuilderAvisynth(IScriptPathService scriptPathService, IFileSystem fileSystemService, IShortFileNameService shortFileNameService)
        {
            this.scriptPath = scriptPathService ?? throw new ArgumentNullException(nameof(scriptPathService));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            this.shortFileName = shortFileNameService ?? throw new ArgumentNullException(nameof(shortFileNameService));
        }

        /// <summary>
        /// Adds a line containing the plugin path. Must be called before loading any other plugin.
        /// </summary>
        public void AddPluginPath()
        {
            AppendLine(@"P=""{0}""", shortFileName.GetAsciiPath(scriptPath.PluginsPath));
            AppendLine(@"AddAutoloadDir(P, true)");
            //AppendLine(@"AddAutoloadDir(P+""Shaders\"", true)");
        }

        /// <summary>
        /// Moves all LoadPlugin and Import commands to the beginning to make the script more readable.
        /// </summary>
        public void Cleanup()
        {
            string[] Lines = _script.ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            string[] CommandsToMove = new string[] { "AddAutoloadDir", "P=", "LoadPlugin", "Import", "LoadVirtualDubPlugin" };
            var NewScriptLines = Lines.Where(l => CommandsToMove.Any(c => l.StartsWith(c, StringComparison.InvariantCultureIgnoreCase))).Concat(Lines.Where(l => !CommandsToMove.Any(c => l.StartsWith(c, StringComparison.InvariantCultureIgnoreCase))));
            _script = new StringBuilder(string.Join(scriptPath.NewLine, NewScriptLines));
        }

        /// <summary>
        /// Rewrites the script to run through MP_Pipeline.
        /// </summary>
        public void ConvertToMultiProcesses(double sourceFrameRate)
        {
            string[] Lines = _script.ToString().TrimEnd('\r', '\n').Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            _script = new StringBuilder();
            AddPluginPath();
            LoadPluginAvsi("AviSynthMT.avsi");
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
            if (OtherLines.Last() != "### ###")
            {
                AppendLine("### prefetch: 3, 3"); // Keep 3 frames before and after for temporal filters.
                AppendLine("### ###");
            }
            AppendLine("\"\"\")");
            if (!string.IsNullOrEmpty(AudioDubLine) || !string.IsNullOrEmpty(AviSourceLine))
            {
                AppendLine();
                if (!string.IsNullOrEmpty(AudioDubLine) && string.IsNullOrEmpty(TrimLine) && string.IsNullOrEmpty(AssumeFpsLine))
                {
                    AppendLine(AudioDubLine);
                }
                else
                {
                    if (string.IsNullOrEmpty(AudioDubLine))
                        AppendLine("A=" + AviSourceLine);
                    else
                    {
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
        }

        /// <summary>
        /// Removes MultiThreading commands from script.
        /// </summary>
        public void RemoveMT()
        {
            _script = _script.Replace(string.Format("Cores={0},", Environment.ProcessorCount), "Cores=1,");
            string[] Lines = _script.ToString().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            string[] CommandsToComment = new string[] { "SetMTMode", "SetFilterMTMode", "Prefetch", @"Import(P+""AviSynthMT.avsi"")" };
            string[] NewLines = Lines.Where(l => !CommandsToComment.Any(c => l.StartsWith(c))).ToArray();
            _script = new StringBuilder(string.Join(scriptPath.NewLine, NewLines));
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

        public void LoadPluginDll(string fileName)
        {
            //if (!loadedPluginsDll.Contains(fileName)) {
            //    AppendLine(@"LoadPlugin(P+""{0}"")", fileName);
            //    loadedPluginsDll.Add(fileName);
            //}
        }

        public void LoadPluginAvsi(string fileName)
        {
            //if (!loadedPluginsAvsi.Contains(fileName)) {
            //    AppendLine(@"Import(P+""{0}"")", fileName);
            //    loadedPluginsAvsi.Add(fileName);
            //}
        }

        public void OpenAvi(string fileName, bool audio)
        {
            AppendLine(@"file=""{0}""", shortFileName.GetAsciiPath(fileName));
            AppendLine(@"AviSource(file, audio={0}, pixel_type=""YV12"")", audio);
        }

        public void OpenDirect(string fileName, bool audio)
        {
            OpenDirect(fileName, audio, true);
        }

        public void OpenDirect(string fileName, bool audio, bool video)
        {
            // Create cache file when file size is over 500MB
            bool Cache = fileSystem.FileInfo.FromFileName(fileName).Length >= 500 * 1024 * 1024;
            LoadPluginDll("LSMASHSource.dll");
            AppendLine(@"file=""{0}""", shortFileName.GetAsciiPath(fileName));
            if (video)
                AppendLine("LWLibavVideoSource(file, cache={0})", Cache);
            if (audio)
            {
                string AudioScript = string.Format("LWLibavAudioSource(file, cache={0})", Cache);
                if (video)
                    AppendLine("AudioDub({0})", AudioScript);
                else
                    AppendLine(AudioScript);
            }
        }
    }
}

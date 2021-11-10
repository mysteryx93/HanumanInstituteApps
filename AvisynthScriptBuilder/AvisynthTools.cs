using System;
using System.IO.Abstractions;
using HanumanInstitute.CommonServices;
using HanumanInstitute.FFmpeg;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Provides tools to get information about videos.
    /// </summary>
    public class AvisynthTools : IAvisynthTools
    {
        private readonly IScriptPathService _scriptPath;
        private readonly IScriptFactory _scriptFactory;
        private readonly IFileSystem _fileSystem;
        private readonly IProcessWorkerFactory _ffmpegFactory;
        private readonly IMediaScript _mediaScript;

        public AvisynthTools() { }

        public AvisynthTools(IScriptPathService scriptPathService, IScriptFactory scriptFactory, IFileSystem fileSystemService, IProcessWorkerFactory ffmpegFactory, IMediaScript mediaScript)
        {
            _scriptPath = scriptPathService ?? throw new ArgumentNullException(nameof(scriptPathService));
            _scriptFactory = scriptFactory ?? throw new ArgumentNullException(nameof(scriptFactory));
            _fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            _ffmpegFactory = ffmpegFactory ?? throw new ArgumentNullException(nameof(ffmpegFactory));
            _mediaScript = mediaScript ?? throw new ArgumentNullException(nameof(mediaScript));
        }

        /// <summary>
        /// Gets an AviSynth clip information by running a script that outputs the frame count to a file.
        /// </summary>
        /// <param name="source">The AviSynth script to get information about.</param>
        /// <param name="options">The options that control the behaviors of the process.</param>
        /// <returns>The frame count.</returns>
        public long GetFrameCount(string source, ProcessOptionsEncoder options)
        {
            source.CheckNotNullOrEmpty(nameof(source));
            if (!_fileSystem.File.Exists(source))
            {
                return 0;
            }

            var TempScriptBase = _fileSystem.Path.ChangeExtension(source, null);
            var TempScript = _scriptPath.GetTempFile("avs");
            var TempResult = _fileSystem.Path.ChangeExtension(TempScript, "txt");

            IScriptBuilderAvisynth Script;
            if (source.EndsWith(".avs", StringComparison.InvariantCultureIgnoreCase))
            {
                // Read source script and remove MT. Also remove Deshaker if present.
                var FileContent = _fileSystem.File.ReadAllText(source);
                FileContent = FileContent.Replace(_scriptPath.NewLine + "Deshaker", _scriptPath.NewLine + "#Deshaker", StringComparison.InvariantCulture);
                Script = _scriptFactory.CreateAvisynthScript();
                Script.Script = FileContent;
                Script.RemoveMT();
            }
            else
            {
                // Generate script to read media file.
                Script = _scriptFactory.CreateAvisynthScript();
                Script.AddPluginPath();
                Script.OpenDirect(source, false);
            }
            // Get frame count.
            Script.AppendLine();
            Script.AppendLineInvariant(@"WriteFileStart(""{0}"", ""FrameRate""{1}""Framecount"")", TempResult, @", """""" "","" """""", ");
            Script.AppendLine("Trim(0,-1)");
            Script.WriteToFile(TempScript);

            // Run script.
            _mediaScript.RunAvisynth(TempScript, options);

            // Read frame count
            long Result = 0;
            if (_fileSystem.File.Exists(TempResult))
            {
                var FileString = _fileSystem.File.ReadAllText(TempResult);
                var FileValues = FileString.Split(',');
                try
                {
                    //Result.FrameRate = float.Parse(FileValues[0], CultureInfo.InvariantCulture);
                    Result = int.Parse(FileValues[1]);
                }
                catch
                {
                }
            }

            // Delete temp files.
            _fileSystem.File.Delete(TempScript);
            _fileSystem.File.Delete(TempResult);

            return Result;
        }

        /// <summary>
        /// Returns the audio gain that can be applied to an audio file.
        /// </summary>
        /// <param name="settings">The source file to analyze.</param>
        /// <param name="options">The options that control the behaviors of the process.</param>
        /// <returns>A float value representing the audio gain that can be applied, or null if it failed.</returns>
        public float? GetAudioGain(string filePath, ProcessOptionsEncoder options)
        {
            var Worker = _ffmpegFactory.CreateEncoder(options);
            var Args = string.Format(@"-i ""{0}"" -af ""volumedetect"" -f null NUL", filePath);
            Worker.RunEncoder(Args, EncoderApp.FFmpeg);
            float? Result = null;
            var FileString = Worker.Output;
            // Find max_volume.
            var SearchVal = "max_volume: ";
            var Pos1 = FileString.IndexOf(SearchVal);
            if (Pos1 >= 0)
            {
                Pos1 += SearchVal.Length;
                // Find end of line.
                var Pos2 = FileString.IndexOf('\r', Pos1);
                if (Pos2 >= 0)
                {
                    var MaxVolString = FileString.Substring(Pos1, Pos2 - Pos1);
                    if (MaxVolString.Length > 3)
                    {
                        // Remove ' dB'
                        MaxVolString = MaxVolString.Substring(0, MaxVolString.Length - 3);
                        if (float.TryParse(MaxVolString, out var MaxVol))
                        {
                            Result = Math.Abs(MaxVol);
                        }
                    }
                }
            }
            return Result;
        }
    }
}

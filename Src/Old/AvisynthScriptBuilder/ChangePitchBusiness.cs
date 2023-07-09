using System;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using HanumanInstitute.FFmpeg;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Handles 432hz auto-pitch features, which makes the music in better harmony with your heart.
    /// </summary>
    public class ChangePitchBusiness : IChangePitchBusiness
    {
        private readonly IScriptPathService scriptPath;
        private readonly IScriptFactory scriptFactory;
        private readonly IFileSystem fileSystem;

        public ChangePitchBusiness(IScriptPathService scriptPathService, IScriptFactory scriptFactory, IFileSystem fileSystemService)
        {
            scriptPath = scriptPathService ?? throw new ArgumentNullException(nameof(scriptPathService));
            this.scriptFactory = scriptFactory ?? throw new ArgumentNullException(nameof(scriptFactory));
            fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        }

        /// <summary>
        /// Generates an AviSynth script that will auto-pitch the audio to 432hz. You then open this script file in the video player instead of directly opening the file.
        /// </summary>
        /// <param name="inputFile">The video to play.</param>
        /// <param name="fileInfo">An object containing media file information.</param>
        public void GenerateScript(string inputFile, FileInfoFFmpeg fileInfo, string scriptLocation)
        {
            var Script = scriptFactory.CreateAvisynthScript();
            Script.AddPluginPath();

            //Script.AppendLine(@"SetFilterMTMode(""DEFAULT_MT_MODE"",2)");
            //Script.AppendLine(@"SetFilterMTMode(""LWLibavVideoSource"",3)");
            //Script.AppendLine(@"SetFilterMTMode(""LWLibavAudioSource"",3)");
            var IsAudio = scriptPath.AudioExtensions.Contains(fileSystem.Path.GetExtension(inputFile).ToLower());
            Script.OpenDirect(inputFile, fileInfo.AudioStream != null, !IsAudio && fileInfo.VideoStream != null);
            if (IsAudio)
            {
                Script.AppendLine(CultureInfo.InvariantCulture, "AudioDub(BlankClip(Last, width=8, height=8), Last)");
            }

            Script.AppendLine(CultureInfo.InvariantCulture, "Preroll(int(FrameRate*3))");
            // This causes a slight audio delay in AviSynth 2.6
            Script.LoadPluginDll("TimeStretch.dll");
            Script.AppendLine(CultureInfo.InvariantCulture, "ResampleAudio(48000)");
            Script.AppendLine(CultureInfo.InvariantCulture, "TimeStretchPlugin(pitch = 100.0 * 0.98181819915771484)");
            //Script.AppendLine("Prefetch({0})", CPU);

            Script.Cleanup();
            Script.WriteToFile(scriptLocation);
        }
    }
}

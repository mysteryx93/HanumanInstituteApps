using EmergenceGuardian.FFmpeg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmergenceGuardian.Avisynth {
    public static class AvisynthEnv {
        public static string PluginsPath { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Encoder\");

        private static AvisynthVersion avisynthVersionCache;

        public static AvisynthVersion GetAvisynthVersion() {
            if (avisynthVersionCache != AvisynthVersion.None)
                return avisynthVersionCache;

            string AviSynthFile = Path.Combine(Environment.SystemDirectory, "AviSynth.dll");
            if (File.Exists(AviSynthFile)) {
                FileVersionInfo Info = FileVersionInfo.GetVersionInfo(AviSynthFile);
                if (Info.ProductName.ToLower().StartsWith("avisynth+"))
                    avisynthVersionCache = AvisynthVersion.AviSynthPlus;
                else
                    avisynthVersionCache = AvisynthVersion.AviSynth26;
            } else
                avisynthVersionCache = AvisynthVersion.None;
            return avisynthVersionCache;
        }


        /// <summary>
        /// The first time the media encoder window is shown, it will test whether OpenCL 1.2 is support to use the latest version 
        /// of KNLMeans. If not, an older version of the library will be used.
        /// </summary>
        private static SupportedOpenClVersion gpuSupportsOpenCL12;
        public static SupportedOpenClVersion GpuSupport {
            get {
                if (gpuSupportsOpenCL12 == SupportedOpenClVersion.NotTested) {
                    gpuSupportsOpenCL12 = TestSupportedOpenClVersion();
                }
                return gpuSupportsOpenCL12;
            }
        }

        /// <summary>
        /// Returns which version of OpenCL the GPU supports.
        /// </summary>
        /// <returns>The version of OpenCL supported by the GPU.</returns>
        private static SupportedOpenClVersion TestSupportedOpenClVersion() {
            if (GpuSupportsOpenCL(true))
                return SupportedOpenClVersion.v12;
            else if (GpuSupportsOpenCL(false))
                return SupportedOpenClVersion.v11;
            else
                return SupportedOpenClVersion.None;
        }

        /// <summary>
        /// Returns whether the GPU supports the latest version of KNLMeans with OpenCL 1.2
        /// </summary>
        /// <param name="supports11">If true, it will instead test whether the GPU supports OpenCL 1.1</param>
        /// <returns>True if OpenCL is supported.</returns>
        private static bool GpuSupportsOpenCL(bool version12) {
            string TempScript = Path.Combine(Path.GetTempPath(), "GpuSupportsOpenCL.avs");
            string TempResult = Path.Combine(Path.GetTempPath(), "GpuSupportsOpenCL.txt");

            AviSynthScriptBuilder Script = new AviSynthScriptBuilder();
            Script.AddPluginPath();
            Script.LoadPluginDll(string.Format("KNLMeansCL{0}.dll", version12 ? "" : "-6.11"));
            Script.AppendLine(@"colorbars(pixel_type = ""yv12"").killaudio().trim(1, 1)");
            Script.AppendLine("Result = true");
            Script.AppendLine("try {");
            Script.AppendLine(@"KNLMeansCL(device_type=""GPU"")");
            Script.AppendLine("} catch(error_msg) {");
            Script.AppendLine("Result = false");
            Script.AppendLine("}");
            Script.AppendLine(@"WriteFileStart(""{0}"", string(Result))", TempResult);
            Script.WriteToFile(TempScript);

            // Run script.
            FFmpegProcess Worker = new FFmpegProcess();
            Worker.Options.Timeout = TimeSpan.FromSeconds(10);
            Worker.Options.DisplayMode = FFmpegDisplayMode.ErrorOnly;
            Worker.RunAvisynth(TempScript);

            // Read frame count
            bool Result = false;
            if (File.Exists(TempResult)) {
                string FileString = File.ReadAllText(TempResult);
                try {
                    Result = bool.Parse(FileString.TrimEnd());
                } catch {
                    Result = false;
                }
            }

            // Delete temp files.
            File.Delete(TempScript);
            File.Delete(TempResult);

            return Result;
        }
    }

    public enum AvisynthVersion {
        None,
        AviSynth26,
        AviSynthPlus
    }

    public enum SupportedOpenClVersion {
        NotTested,
        None,
        v11,
        v12
    }
}

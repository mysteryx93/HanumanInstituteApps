using System;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Facilitates the creation of AviSynth scripts.
    /// </summary>
    public interface IScriptBuilderAvisynth : IScriptBuilder
    {
        /// <summary>
        /// Adds a line containing the plugin path. Must be called before loading any other plugin.
        /// </summary>
        void AddPluginPath();
        /// <summary>
        /// Moves all LoadPlugin and Import commands to the beginning to make the script more readable.
        /// </summary>
        void Cleanup();
        /// <summary>
        /// Rewrites the script to run through MP_Pipeline.
        /// </summary>
        void ConvertToMultiProcesses(double sourceFrameRate);
        /// <summary>
        /// Removes MultiThreading commands from script.
        /// </summary>
        void RemoveMT();
        void LoadPluginDll(string fileName);
        void LoadPluginAvsi(string fileName);
        void OpenAvi(string fileName, bool audio);
        void OpenDirect(string fileName, bool audio);
        void OpenDirect(string fileName, bool audio, bool video);
    }
}

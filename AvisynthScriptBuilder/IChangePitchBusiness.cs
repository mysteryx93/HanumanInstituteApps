using System;
using System.IO.Abstractions;
using System.Linq;
using HanumanInstitute.FFmpeg;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Handles 432hz auto-pitch features, which makes the music in better harmony with your heart.
    /// </summary>
    public interface IChangePitchBusiness
    {
        /// <summary>
        /// Generates an AviSynth script that will auto-pitch the audio to 432hz. You then open this script file in the video player instead of directly opening the file.
        /// </summary>
        /// <param name="inputFile">The video to play.</param>
        /// <param name="infoReader">An object to read media information.</param>
        void GenerateScript(string inputFile, IFileInfoFFmpeg fileInfo, string scriptLocation);
    }
}

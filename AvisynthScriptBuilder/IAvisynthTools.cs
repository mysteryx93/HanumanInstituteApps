using System;
using System.IO.Abstractions;
using HanumanInstitute.Encoder;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Provides tools to get information about videos.
    /// </summary>
    public interface IAvisynthTools
    {
        /// <summary>
        /// Gets an AviSynth clip information by running a script that outputs the frame count to a file.
        /// </summary>
        /// <param name="source">The AviSynth script to get information about.</param>
        /// <param name="options">The options that control the behaviors of the process.</param>
        /// <returns>The frame count.</returns>
        long GetFrameCount(string source, ProcessOptionsEncoder options);
        /// <summary>
        /// Returns the audio gain that can be applied to an audio file.
        /// </summary>
        /// <param name="settings">The source file to analyze.</param>
        /// <param name="options">The options that control the behaviors of the process.</param>
        /// <returns>A float value representing the audio gain that can be applied, or null if it failed.</returns>
        float? GetAudioGain(string filePath, ProcessOptionsEncoder options);
    }
}

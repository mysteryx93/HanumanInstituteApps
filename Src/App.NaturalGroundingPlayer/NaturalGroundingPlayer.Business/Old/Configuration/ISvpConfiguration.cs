using System;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.NaturalGroundingPlayer.Configuration
{
    /// <summary>
    /// Manages the configuration of SVP (Smooth Video Project).
    /// </summary>
    public interface ISvpConfiguration
    {
        /// <summary>
        /// Gets or sets whether SVP is enabled.
        /// </summary>
        bool IsSvpEnabled { get; set; }
        /// <summary>
        /// Clears SVP log files so that it doesn't display an error message when restarting.
        /// </summary>
        void ClearLog();
        /// <summary>
        /// Adds the Natural Grounding Player to SVP's blacklist so that it doesn't affect internal players.
        /// </summary>
        void AddAppToSvpBlacklist();
        /// <summary>
        /// Returns the SVP process.
        /// </summary>
        IProcess GetSvpProcess();
    }
}

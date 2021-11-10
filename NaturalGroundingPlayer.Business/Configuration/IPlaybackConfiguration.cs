using System;
using HanumanInstitute.NaturalGroundingPlayer.Models;

namespace HanumanInstitute.NaturalGroundingPlayer.Configuration
{
    /// <summary>
    /// Manages the configuration of media playback components.
    /// </summary>
    public interface IPlaybackConfiguration
    {
        /// <summary>
        /// Configures MPC-HC and SVP settings to work properly with the Natural Grounding Player.
        /// </summary>
        void ConfigureSettings();
        /// <summary>
        /// Automatically starts or stops SVP and madVR based on current video requirements.
        /// </summary>
        /// <param name="videoStatus">The media containing performance status information.</param>
        void AutoConfigure(Media videoStatus);
    }
}

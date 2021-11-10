using System;

namespace HanumanInstitute.NaturalGroundingPlayer.Configuration
{
    /// <summary>
    /// Manages the configuration of MPV.
    /// </summary>
    public interface IMpvConfiguration
    {
        /// <summary>
        /// Gets or sets whether MadVR is enabled in MPC-HC.
        /// </summary>
        bool IsMadvrEnabled { get; set; }
        /// <summary>
        /// Gets or sets whether to play videos in loop.
        /// </summary>
        bool Loop { get; set; }
        /// <summary>
        /// Gets or sets whether the player should remember file position when re-opening the player.
        /// </summary>
        bool RememberFilePos { get; set; }
        /// <summary>
        /// Gets or sets whether to override aspect ratio as 16:9 (widescreen).
        /// </summary>
        bool IsWidescreenEnabled { get; set; }
        /// <summary>
        /// Kills any MPC-HC running instance.
        /// </summary>
        void KillMpcProcesses();
        /// <summary>
        /// Starts a MPC process without hooking into it.
        /// </summary>
        /// <param name="fileName">The video file to open.</param>
        void StartMpc(string fileName);
    }
}

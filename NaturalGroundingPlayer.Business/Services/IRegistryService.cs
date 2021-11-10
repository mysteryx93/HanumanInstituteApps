using System;

namespace HanumanInstitute.NaturalGroundingPlayer.Services
{
    /// <summary>
    /// Provides access to Windows registry keys.
    /// </summary>
    public interface IRegistryService
    {
        /// <summary>
        /// Gets the install location of MPC-HC from the registry, validates the file exists, and returns a default location if not found.
        /// </summary>
        string MpcExePath { get; }
        /// <summary>
        /// Gets or sets MPC-HC's Video Renderer.
        /// </summary>
        int? MpcVideoRenderer { get; set; }
        /// <summary>
        /// Gets or sets whether MPC-HC has Loop enabled.
        /// </summary>
        bool MpcLoop { get; set; }
        /// <summary>
        /// Gets or sets whether MPC-HC remembers last file position after closing.
        /// </summary>
        bool MpcRememberFilePos { get; set; }
        /// <summary>
        /// Gets or sets MPC-HC aspect ratio X.
        /// </summary>
        int? MpcAspectRatioX { get; set; }
        /// <summary>
        /// Gets or sets MPC-HC aspect ratio Y.
        /// </summary>
        int? MpcAspectRatioY { get; set; }
    }
}

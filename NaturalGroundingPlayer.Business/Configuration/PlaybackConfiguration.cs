using System;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    #region Interface

    /// <summary>
    /// Manages the configuration of media playback components.
    /// </summary>
    public interface IPlaybackConfiguration {
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

    #endregion

    /// <summary>
    /// Manages the configuration of media playback components.
    /// </summary>
    public class PlaybackConfiguration : IPlaybackConfiguration {

        #region Declarations / Constructors

        protected readonly ISettings settings;
        protected readonly IMpcConfiguration mpc;
        protected readonly ISvpConfiguration svp;

        public PlaybackConfiguration() : this(new Settings(), new MpcConfiguration(), new SvpConfiguration()) { }

        public PlaybackConfiguration(ISettings settings, IMpcConfiguration mpcConfig, ISvpConfiguration svpConfig) {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.mpc = mpcConfig ?? throw new ArgumentNullException(nameof(mpcConfig));
            this.svp = svpConfig ?? throw new ArgumentNullException(nameof(svpConfig));
        }

        #endregion

        /// <summary>
        /// Configures MPC-HC and SVP settings to work properly with the Natural Grounding Player.
        /// </summary>
        public void ConfigureSettings() {
            if (settings.Data.MediaPlayerApp != MediaPlayerEnum.MpcHc)
                return;

            mpc.Loop = true;
            mpc.RememberFilePos = false;
            svp.AddAppToSvpBlacklist();
        }

        /// <summary>
        /// Automatically starts or stops SVP and madVR based on current video requirements.
        /// </summary>
        /// <param name="videoStatus">The media containing performance status information.</param>
        public void AutoConfigure(Media videoStatus) {
            if (settings.Data.MediaPlayerApp != MediaPlayerEnum.MpcHc)
                return;

            // If no status information is supplied, create default object with default values of 'false'.
            if (videoStatus == null)
                videoStatus = new Media();

            svp.IsSvpEnabled = settings.Data.EnableSvp && !videoStatus.DisableSvp;
            mpc.IsMadvrEnabled = settings.Data.EnableMadVR && !videoStatus.DisableMadVr;
        }
    }
}

using System;
using HanumanInstitute.NaturalGroundingPlayer.Models;
using HanumanInstitute.NaturalGroundingPlayer.Services;

namespace HanumanInstitute.NaturalGroundingPlayer.Configuration
{
    /// <summary>
    /// Manages the configuration of media playback components.
    /// </summary>
    public class PlaybackConfiguration : IPlaybackConfiguration
    {
        private readonly IAppSettingsProvider _settings;
        private readonly IMpvConfiguration _mpv;
        private readonly ISvpConfiguration _svp;

        public PlaybackConfiguration(IAppSettingsProvider settings, IMpvConfiguration mpcConfig, ISvpConfiguration svpConfig)
        {
            _settings = settings;
            _mpv = mpcConfig;
            _svp = svpConfig;
        }

        /// <summary>
        /// Configures MPC-HC and SVP settings to work properly with the Natural Grounding Player.
        /// </summary>
        public void ConfigureSettings()
        {
            //if (_settings.Value.MediaPlayerApp != MediaPlayerEnum.MpcHc)
            //{
            //    return;
            //}

            _mpv.Loop = true;
            _mpv.RememberFilePos = false;
            _svp.AddAppToSvpBlacklist();
        }

        /// <summary>
        /// Automatically starts or stops SVP and madVR based on current video requirements.
        /// </summary>
        /// <param name="videoStatus">The media containing performance status information.</param>
        public void AutoConfigure(Media videoStatus)
        {
            //if (_settings.Value.MediaPlayerApp != MediaPlayerEnum.MpcHc)
            //{
            //    return;
            //}

            // If no status information is supplied, create default object with default values of 'false'.
            if (videoStatus == null)
            {
                videoStatus = new Media();
            }

            _svp.IsSvpEnabled = _settings.Value.EnableSvp && !videoStatus.DisableSvp;
            _mpv.IsMadvrEnabled = _settings.Value.EnableMadVR && !videoStatus.DisableMadVr;
        }
    }
}

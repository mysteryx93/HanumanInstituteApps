using System;
using System.Collections.Generic;
using System.ComponentModel;
using HanumanInstitute.AvisynthScriptBuilder;
using HanumanInstitute.FFmpeg;

namespace HanumanInstitute.Player432hz.Business
{
    /// <summary>
    /// Manages the playback of a list of media files while creating 432hz scripts.
    /// </summary>
    public class PlaylistPlayer432hz : IPlaylistPlayer, INotifyPropertyChanged
    {
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        private readonly PlaylistPlayer basePlayer;
        private readonly IChangePitchBusiness changePitch;
        private readonly IAppPathService appPath;
        private readonly IMediaInfoReader mediaInfo;

        public PlaylistPlayer432hz(PlaylistPlayer basePlayer, IChangePitchBusiness changePitch, IAppPathService appPath, IMediaInfoReader mediaInfo)
        {
            this.basePlayer = basePlayer ?? throw new ArgumentNullException(nameof(basePlayer));
            basePlayer.PropertyChanged += BasePlayer_PropertyChanged;
            this.changePitch = changePitch;
            this.appPath = appPath;
            this.mediaInfo = mediaInfo;
        }

        private void BasePlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(basePlayer.NowPlaying))
            {
                // NowPlaying Changed
                if (!string.IsNullOrEmpty(basePlayer.NowPlaying))
                {
                    var fileInfo = mediaInfo.GetFileInfo(basePlayer.NowPlaying);
                    changePitch.GenerateScript(basePlayer.NowPlaying, fileInfo, appPath.Player432hzScriptFile);
                    NowPlaying = appPath.Player432hzScriptFile;
                }
                else
                {
                    NowPlaying = null;
                }
            }
            else if (e.PropertyName == nameof(basePlayer.NowPlayingTitle))
            {
                // NowPlayingTitle Changed
                NowPlayingTitle = basePlayer.NowPlayingTitle;
            }
        }

        /// <summary>
        /// Returns the list of files currently playing.
        /// </summary>
        public IList<string> Files => basePlayer.Files;

        /// <summary>
        /// Returns the path of the file currently playing.
        /// </summary>
        public string NowPlaying { get; set; }

        /// <summary>
        /// Gets the display title of the file currently playing.
        /// </summary>
        public string NowPlayingTitle { get; set; }

        /// <summary>
        /// Starts the playback of specified list of media files.
        /// </summary>
        /// <param name="list">The list of file paths to play.</param>
        /// <param name="current">If specified, playback will start with specified file.</param>
        public void Play(IEnumerable<string> list, string current) => basePlayer.Play(list, current);

        /// <summary>
        /// Starts playing the next media file from the list.
        /// </summary>
        public void PlayNext() => basePlayer.PlayNext();
    }
}

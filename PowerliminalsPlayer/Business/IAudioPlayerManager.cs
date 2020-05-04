using System;
using System.Collections.Generic;

namespace HanumanInstitute.PowerliminalsPlayer.Business
{
    /// <summary>
    /// Manages the playback of a list of media files.
    /// </summary>
    public interface IAudioPlayerManager
    {
        /// <summary>
        /// Returns the list of files currently playing.
        /// </summary>
        List<string> Files { get; }
        /// <summary>
        /// Returns the path of the file currently playing.
        /// </summary>
        string NowPlaying { get; set; }
        /// <summary>
        /// Occurs when a media file starts to play.
        /// </summary>
        event EventHandler<PlayingEventArgs> StartPlaying;
        /// <summary>
        /// Starts the playback of specified list of media files.
        /// </summary>
        /// <param name="list">The list of file paths to play.</param>
        /// <param name="current">If specified, playback will start with specified file.</param>
        void Play(IEnumerable<string> list, string current);
        /// <summary>
        /// Starts playing the next media file from the list.
        /// </summary>
        void PlayNext();
    }
}

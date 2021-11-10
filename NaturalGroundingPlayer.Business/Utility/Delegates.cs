using System;
using System.Collections.Generic;

namespace HanumanInstitute.NaturalGroundingPlayer.Business
{
    /// <summary>
    /// This delegate is used to display a message to the user and return the answer.
    /// </summary>
    /// <param name="title">The title of the message to display.</param>
    /// <param name="message">The message to display.</param>
    /// <returns>The user's response.</returns>
    public delegate bool? DisplayMessageDelegate(string title, string message);

    /// <summary>
    /// Contains information for the NowPlaying event.
    /// </summary>
    public class NowPlayingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets whether the media info was edited and must be reloaded.
        /// </summary>
        public bool ReloadInfo { get; set; }

        /// <summary>
        /// Initializes a new instance of the NowPlayingEventArgs class.
        /// </summary>
        public NowPlayingEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the NowPlayingEventArgs class.
        /// </summary>
        /// <param name="reloadInfo">Whether data was edited and must be reloaded.</param>
        public NowPlayingEventArgs(bool reloadInfo)
        {
            this.ReloadInfo = reloadInfo;
        }
    }
}

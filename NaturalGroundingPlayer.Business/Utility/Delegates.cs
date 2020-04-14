using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;
using System;
using System.Collections.Generic;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {
    /// <summary>
    /// This delegate is used to display a message to the user and return the answer.
    /// </summary>
    /// <param name="title">The title of the message to display.</param>
    /// <param name="message">The message to display.</param>
    /// <returns>The user's response.</returns>
    public delegate bool? DisplayMessageDelegate(string title, string message);

    /// <summary>
    /// Contains information for the GetConditions event. The event handler should fill Conditions with search conditions.
    /// </summary>
    public class GetConditionsEventArgs : EventArgs {
        /// <summary>
        /// Gets or sets whether to widen the range of criterias, if none were previously found.
        /// </summary>
        public bool IncreaseTolerance { get; set; }
        /// <summary>
        /// Gets or sets the position in the playlist pos to fill.
        /// </summary>
        public int QueuePos { get; set; }
        /// <summary>
        /// Gets or sets the conditions to use for searching the next video.
        /// </summary>
        public SearchSettings Conditions { get; set; }

        /// <summary>
        /// Initializes a new instance of the GetConditionsEventArgs class.
        /// </summary>
        public GetConditionsEventArgs() {
        }

        public GetConditionsEventArgs(SearchSettings conditions) {
            this.Conditions = conditions;
            this.Conditions.RatingFilters = new List<SearchRatingSetting>();
            this.Conditions.RatingFilters.Add(new SearchRatingSetting());
        }
    }

    /// <summary>
    /// Contains information for the NowPlaying event.
    /// </summary>
    public class NowPlayingEventArgs : EventArgs {
        /// <summary>
        /// Gets or sets whether the media info was edited and must be reloaded.
        /// </summary>
        public bool ReloadInfo { get; set; }

        /// <summary>
        /// Initializes a new instance of the NowPlayingEventArgs class.
        /// </summary>
        public NowPlayingEventArgs() {
        }

        /// <summary>
        /// Initializes a new instance of the NowPlayingEventArgs class.
        /// </summary>
        /// <param name="reloadInfo">Whether data was edited and must be reloaded.</param>
        public NowPlayingEventArgs(bool reloadInfo) {
            this.ReloadInfo = reloadInfo;
        }
    }
}

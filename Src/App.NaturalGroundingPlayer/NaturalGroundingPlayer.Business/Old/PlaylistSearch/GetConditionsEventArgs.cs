using System;

namespace HanumanInstitute.NaturalGroundingPlayer.PlaylistSearch
{
    /// <summary>
    /// Contains information for the GetConditions event. The event handler should fill Conditions with search conditions.
    /// </summary>
    public class GetConditionsEventArgs : EventArgs
    {
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
        public GetConditionsEventArgs() : this(null)
        { }

        public GetConditionsEventArgs(SearchSettings? conditions)
        {
            this.Conditions = conditions ?? new SearchSettings();
        }
    }
}

using System;

namespace HanumanInstitute.NaturalGroundingPlayer.PlaylistSearch
{
    /// <summary>
    /// Contains special boolean search conditions. See SearchFieldFilter for the list of conditions.
    /// </summary>
    [Serializable()]
    public class SearchConditionSetting
    {
        /// <summary>
        /// A boolean condition to search.
        /// </summary>
        public SearchFieldFilter Field { get; set; }
        /// <summary>
        /// The boolean value for that condition to search.
        /// </summary>
        public SearchBoolFilter Value { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EmergenceGuardian.NaturalGroundingPlayer.DataAccess {
    /// <summary>
    /// Contains special boolean search conditions. See FieldConditionEnum for the list of conditions.
    /// </summary>
    [Serializable()]
    public class SearchConditionSetting {
        /// <summary>
        /// A boolean condition to search.
        /// </summary>
        public FieldConditionEnum Field { get; set; }
        /// <summary>
        /// The boolean value for that condition to search.
        /// </summary>
        public BoolConditionEnum Value { get; set; }
    }
}

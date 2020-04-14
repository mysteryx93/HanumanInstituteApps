using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EmergenceGuardian.NaturalGroundingPlayer.DataAccess {
    /// <summary>
    /// Contains rating search criterias.
    /// </summary>
    [Serializable()]
    public class SearchRatingSetting {
        /// <summary>
        /// The rating category on which to apply the filter.
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// The conditional operator for the filter.
        /// </summary>
        public OperatorConditionEnum Operator { get; set; }
        /// <summary>
        /// The filter value for the rating category.
        /// </summary>
        public double? Value { get; set; }
        /// <summary>
        /// Stacking SearchRatingSetting objects performs an AND operator. Place conditions here to perform an OR operator.
        /// </summary>
        public SearchRatingSetting Or { get; set; }

        public SearchRatingSetting() { }

        public SearchRatingSetting(string category, OperatorConditionEnum op, double? value) : this(category, op, value, null) { }

        public SearchRatingSetting(string category, OperatorConditionEnum op, double? value, SearchRatingSetting or) {
            this.Category = category;
            this.Operator = op;
            this.Value = value;
            this.Or = or;
        }
    }
}

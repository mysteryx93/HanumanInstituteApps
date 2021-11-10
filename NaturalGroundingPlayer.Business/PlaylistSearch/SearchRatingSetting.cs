using System;

namespace HanumanInstitute.NaturalGroundingPlayer.PlaylistSearch
{
    /// <summary>
    /// Contains rating search criterias.
    /// </summary>
    [Serializable()]
    public class SearchRatingSetting
    {
        /// <summary>
        /// The rating category on which to apply the filter.
        /// </summary>
        public string Category { get; set; } = string.Empty;
        /// <summary>
        /// The conditional operator for the filter.
        /// </summary>
        public SearchOperator Operator { get; set; }
        /// <summary>
        /// The filter value for the rating category.
        /// </summary>
        public double? Value { get; set; }
        /// <summary>
        /// Stacking SearchRatingSetting objects performs an AND operator. Place conditions here to perform an OR operator.
        /// </summary>
        public SearchRatingSetting? Or { get; set; }

        public SearchRatingSetting() { }

        public SearchRatingSetting(string category, SearchOperator op, double? value) : this(category, op, value, null) { }

        public SearchRatingSetting(string category, SearchOperator op, double? value, SearchRatingSetting? or)
        {
            this.Category = category;
            this.Operator = op;
            this.Value = value;
            this.Or = or;
        }
    }
}

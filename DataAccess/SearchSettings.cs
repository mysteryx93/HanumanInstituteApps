using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    /// <summary>
    /// Contains settings to search videos.
    /// </summary>
    [Serializable()]
    public class SearchSettings {
        public SearchSettings() {
            // Add first row that is found to standard properties.
            RatingFilters = new List<SearchRatingSetting>();
            RatingFilters.Add(new SearchRatingSetting());
            ConditionFilters = new List<SearchConditionSetting>();
            ConditionFilters.Add(new SearchConditionSetting());
        }

        /// <summary>
        /// Initializes a new instance of the SearchSettings class.
        /// </summary>
        /// <param name="mediaType">The type of media to return.</param>
        /// <param name="excludeVideos">A list of videos to exclude from the search.</param>
        /// <param name="allowDownloading">False to allow downloading from the Internet, True to search only local files.</param>
        /// <param name="ratingRatio">A number between -1 and 1 representing the rating Heigth/Depth priority ratio.</param>
        public SearchSettings(MediaType mediaType, List<Guid> excludeVideos, bool allowDownloading, double ratingRatio):this() {
            this.MediaType = mediaType;
            this.ExcludeVideos = excludeVideos;
            this.AllowDownloading = allowDownloading;
            this.RatingRatio = ratingRatio;
        }

        /// <summary>
        /// A value to search within artist, title, folder and category.
        /// </summary>
        public string Search { get; set; }
        /// <summary>
        /// The type of media to return.
        /// </summary>
        public MediaType MediaType { get; set; }
        /// <summary>
        /// A list of videos to exclude from the search.
        /// </summary>
        public List<Guid> ExcludeVideos { get; set; }
        /// <summary>
        /// Gets or sets whether to search only local files or allow downloading from the Internet.
        /// </summary>
        public bool AllowDownloading { get; set; }

        public List<SearchRatingSetting> RatingFilters { get; set; }
        public List<SearchConditionSetting> ConditionFilters { get; set; }

        public HasRatingEnum HasRating { get; set; }
        public List<string> BuyUrlDomains { get; set; }
        public bool BuyUrlDomainsNegated { get; set; }
        public string OrderBy { get; set; }
        public ListSortDirection OrderByDirection { get; set; }

        /// <summary>
        /// Gets or sets the Height/Depth rating priority ratio as a number between -1 and 1.
        /// </summary>
        public double RatingRatio { get; set; }
        /// <summary>
        /// Returns the total number of items found.
        /// </summary>
        public int TotalFound { get; set; }

        public string RatingCategory {
            get { return RatingFilters[0].Category; }
            set { RatingFilters[0].Category = value; }
        }

        public OperatorConditionEnum RatingOperator {
            get { return RatingFilters[0].Operator; }
            set { RatingFilters[0].Operator = value; }
        }

        public double? RatingValue {
            get { return RatingFilters[0].Value; }
            set { RatingFilters[0].Value = value; }
        }

        public void SetRatingCategory(string ratingCategory, OperatorConditionEnum ratingOperator, double? ratingValue) {
            this.RatingCategory = ratingCategory;
            this.RatingOperator = ratingOperator;
            this.RatingValue = ratingValue;
        }

        public FieldConditionEnum ConditionField {
            get { return ConditionFilters[0].Field; }
            set { ConditionFilters[0].Field = value; }
        }

        public BoolConditionEnum ConditionValue {
            get { return ConditionFilters[0].Value; }
            set { ConditionFilters[0].Value = value; }
        }

        public void SetCondition(FieldConditionEnum conditionField, BoolConditionEnum conditionValue) {
            this.ConditionField = conditionField;
            this.ConditionValue = conditionValue;
        }

        public SearchSettings Update(List<Guid> excludeVideos, bool allowDownloading) {
            this.ExcludeVideos = excludeVideos;
            this.AllowDownloading = allowDownloading;
            return this;
        }
    }

    [Serializable()]
    public class SearchRatingSetting {
        public string Category { get; set; }
        public OperatorConditionEnum Operator { get; set; }
        public double? Value { get; set; }
        public SearchRatingSetting Or { get; set; }

        public SearchRatingSetting() {
        }

        public SearchRatingSetting(string category, OperatorConditionEnum op, double? value) {
            this.Category = category;
            this.Operator = op;
            this.Value = value;
        }
    }

    [Serializable()]
    public class SearchConditionSetting {
        public FieldConditionEnum Field { get; set; }
        public BoolConditionEnum Value { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using HanumanInstitute.CommonServices;
using HanumanInstitute.NaturalGroundingPlayer.Models;

namespace HanumanInstitute.NaturalGroundingPlayer.PlaylistSearch
{
    /// <summary>
    /// Contains settings to search videos.
    /// </summary>
    [Serializable()]
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class SearchSettings
    {
        public SearchSettings() { }

        /// <summary>
        /// Initializes a new instance of the SearchSettings class.
        /// </summary>
        /// <param name="mediaType">The type of media to return.</param>
        /// <param name="excludeVideos">A list of videos to exclude from the search.</param>
        /// <param name="allowDownloading">False to allow downloading from the Internet, True to search only local files.</param>
        /// <param name="ratingRatio">A number between -1 and 1 representing the rating Heigth/Depth priority ratio.</param>
        public SearchSettings(MediaType mediaType, List<Guid> excludeVideos, bool allowDownloading, double ratingRatio) : this()
        {
            this.MediaType = mediaType;
            this.ExcludeVideos = excludeVideos;
            this.AllowDownloading = allowDownloading;
            this.RatingRatio = ratingRatio;
        }

        /// <summary>
        /// Gets or sets a type of filter to apply.
        /// </summary>
        public SearchFilter FilterType { get; set; }
        /// <summary>
        /// When set, the filter must match this value.
        /// </summary>
        public string? FilterValue { get; set; }
        /// <summary>
        /// Gets or setes a value to search within artist, title, folder and category.
        /// </summary>
        public string Search { get; set; } = "";
        /// <summary>
        /// The type of media to return.
        /// </summary>
        public MediaType MediaType { get; set; } = MediaType.Video;
        /// <summary>
        /// A list of videos to exclude from the search.
        /// </summary>
        public IList<Guid> ExcludeVideos { get; } = new List<Guid>();
        /// <summary>
        /// Gets or sets whether to search only local files or allow downloading from the Internet.
        /// </summary>
        public bool AllowDownloading { get; set; }
        /// <summary>
        /// A list of SearchRatingSetting objects containing rating search criterias.
        /// </summary>
        public IList<SearchRatingSetting> RatingFilters { get; } = new List<SearchRatingSetting>() { new SearchRatingSetting() };
        /// <summary>
        /// A list of SearchConditionSetting objects containing special boolean conditions. See FieldConditionEnum for the list of conditions.
        /// </summary>
        public IList<SearchConditionSetting> ConditionFilters { get; } = new List<SearchConditionSetting>() { new SearchConditionSetting() };

        public bool? ListIsInDatabase { get; set; } // For categories list
        public bool? IsInDatabase { get; set; } // For details view
        public SearchRatingFilter HasRating { get; set; }
        public IList<string> BuyUrlDomains { get; } = new List<string>();
        public bool BuyUrlDomainsNegated { get; set; }
        public string? OrderBy { get; set; }
        public ListSortDirection OrderByDirection { get; set; }
        public string? DisplayCustomRating { get; set; }

        public string CustomColumn
        {
            get { return !string.IsNullOrEmpty(DisplayCustomRating) ? DisplayCustomRating : RatingCategory; }
        }

        /// <summary>
        /// Gets or sets the Height/Depth rating priority ratio as a number between -1 and 1.
        /// </summary>
        public double RatingRatio { get; set; }
        /// <summary>
        /// Returns the total number of items found.
        /// </summary>
        public int TotalFound { get; set; }

        public string RatingCategory
        {
            get { return RatingFilters[0].Category; }
            set { RatingFilters[0].Category = value; }
        }

        public SearchOperator RatingOperator
        {
            get { return RatingFilters[0].Operator; }
            set { RatingFilters[0].Operator = value; }
        }

        public double? RatingValue
        {
            get { return RatingFilters[0].Value; }
            set { RatingFilters[0].Value = value; }
        }

        public void SetRatingCategory(string ratingCategory, SearchOperator ratingOperator, double? ratingValue)
        {
            this.RatingCategory = ratingCategory;
            this.RatingOperator = ratingOperator;
            this.RatingValue = ratingValue;
        }

        public SearchFieldFilter ConditionField
        {
            get { return ConditionFilters[0].Field; }
            set { ConditionFilters[0].Field = value; }
        }

        public SearchBoolFilter ConditionValue
        {
            get { return ConditionFilters[0].Value; }
            set { ConditionFilters[0].Value = value; }
        }

        public void SetCondition(SearchFieldFilter conditionField, bool conditionValue)
        {
            this.ConditionField = conditionField;
            this.ConditionValue = conditionValue ? SearchBoolFilter.Yes : SearchBoolFilter.No;
        }

        public SearchSettings Update(IEnumerable<Guid> excludeVideos, bool allowDownloading)
        {
            this.ExcludeVideos.Clear();
            this.ExcludeVideos.AddRange(excludeVideos);
            this.AllowDownloading = allowDownloading;
            return this;
        }
    }
}

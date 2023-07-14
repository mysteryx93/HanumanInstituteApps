using System;

namespace HanumanInstitute.NaturalGroundingPlayer.PlaylistSearch
{
    /// <summary>
    /// Represents an item in the search categories list.
    /// </summary>
    public class SearchCategoryItem
    {
        /// <summary>
        /// Gets or sets the type of filtering to apply.
        /// </summary>
        public SearchFilter FilterType { get; set; }
        /// <summary>
        /// When SearchCategory is Artist, Category or Element, gets or sets the value to filter by.
        /// </summary>
        public string FilterValue { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the text to display for the item which may includes the sub-items count.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        public SearchCategoryItem() { }

        public SearchCategoryItem(SearchFilter filterType, string filterValue, string text)
        {
            this.FilterType = filterType;
            this.FilterValue = filterValue;
            this.Text = text;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;

namespace HanumanInstitute.NaturalGroundingPlayer.DataAccess {

    #region Interface

    /// <summary>
    /// Provides data access for Search.
    /// </summary>
    public interface ISearchMediaAccess {
        /// <summary>
        /// Returns a MediaListItem for specified media ID.
        /// </summary>
        /// <param name="videoId">The ID of the video to return.</param>
        /// <returns>A MediaListItem object.</returns>
        MediaListItem GetVideo(Guid videoId);
        /// <summary>
        /// Returns the list of artists that have more than one song.
        /// </summary>
        /// <returns>A list of SearchCategoryItem objects.</returns>
        List<SearchCategoryItem> GetCategoryArtists(SearchSettings settings);
        /// <summary>
        /// Returns the list of categories.
        /// </summary>
        /// <returns>A list of SearchCategoryItem objects.</returns>
        List<SearchCategoryItem> GetCategoryCategories(SearchSettings settings);
        /// <summary>
        /// Returns the list of elements.
        /// </summary>
        /// <returns>A list of SearchCategoryItem objects.</returns>
        List<SearchCategoryItem> GetCategoryElements(SearchSettings settings);
        /// <summary>
        /// Returns a list of MediaListItem based on specified search settings.
        /// </summary>
        /// <param name="settings">The search settings.</param>
        /// <returns>A list of MediaListItem objects.</returns>
        List<MediaListItem> GetList(SearchSettings settings);
        /// <summary>
        /// Returns the list of custom rating categories (not part of the standard ones).
        /// </summary>
        /// <returns>A list of RatingCategory objects.</returns>
        List<RatingCategory> GetCustomRatingCategories();
        /// <summary>
        /// Returns the list of all file names stored in the database.
        /// </summary>
        /// <returns>A list of file names.</returns>
        List<string> GetAllFileNames();
        /// <summary>
        /// Applies all search filters to the video selection query.
        /// </summary>
        /// <param name="Query">The video selection query.</param>
        /// <param name="settings">The settings to apply to the query.</param>
        /// <param name="context">The data context to the database.</param>
        /// <returns>The updated query with filters applied.</returns>
        IQueryable<Media> ApplySearchFilters(IQueryable<Media> Query, SearchSettings settings, Entities context);
    }

    #endregion

    /// <summary>
    /// Provides data access for Search.
    /// </summary>
    public class SearchMediaAccess : ISearchMediaAccess {

        #region Declarations / Constructors

        private INgpContextFactory contextFactory;

        public SearchMediaAccess() : this(new NgpContextFactory()) { }

        public SearchMediaAccess(INgpContextFactory contextFactory) {
            this.contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        #endregion

        /// <summary>
        /// Returns a MediaListItem for specified media ID.
        /// </summary>
        /// <param name="videoId">The ID of the video to return.</param>
        /// <returns>A MediaListItem object.</returns>
        public MediaListItem GetVideo(Guid videoId) {
            using (Entities context = contextFactory.Create()) {
                var Query = context.Media.Where(v => v.MediaId == videoId);
                var Result = QueryVideoListItem(Query, null);
                return Result.FirstOrDefault();
            }
        }

        /// <summary>
        /// Returns the list of artists that have more than one song.
        /// </summary>
        /// <returns>A list of SearchCategoryItem objects.</returns>
        public List<SearchCategoryItem> GetCategoryArtists(SearchSettings settings) {
            using (Entities context = contextFactory.Create()) {
                var Query = (from v in context.Media
                             where v.MediaTypeId == (int)settings.MediaType
                             group v by v.Artist into a
                             where a.Key != "" && 
                                a.Count() > 1 && 
                                (settings.Search == "" || a.Key.Contains(settings.Search))
                             orderby a.Key
                             select new SearchCategoryItem() {
                                 FilterType = SearchFilterEnum.Artist,
                                 FilterValue = a.Key,
                                 Text = a.Key + " (" + a.Count() + ")"
                             });
                return Query.ToList();
            }
        }

        /// <summary>
        /// Returns the list of categories.
        /// </summary>
        /// <returns>A list of SearchCategoryItem objects.</returns>
        public List<SearchCategoryItem> GetCategoryCategories(SearchSettings settings) {
            using (Entities context = contextFactory.Create()) {
                var Query = (from c in context.MediaCategories
                             where c.MediaTypeId == (int)settings.MediaType &&
                                (settings.Search == "" || c.Name.Contains(settings.Search))
                             orderby c.Name
                             select new SearchCategoryItem() {
                                 FilterType = SearchFilterEnum.Category,
                                 FilterValue = c.Name,
                                 Text = c.Name
                             });
                return Query.ToList();
            }
        }

        /// <summary>
        /// Returns the list of elements.
        /// </summary>
        /// <returns>A list of SearchCategoryItem objects.</returns>
        public List<SearchCategoryItem> GetCategoryElements(SearchSettings settings) {
            using (Entities context = contextFactory.Create()) {
                var Query = (from c in context.RatingCategories
                             where (settings.Search == "" || c.Name.Contains(settings.Search))
                             orderby c.Name
                             select new SearchCategoryItem() {
                                 FilterType = SearchFilterEnum.Element,
                                 FilterValue = c.Name,
                                 Text = c.Name
                             });
                return Query.ToList();
            }
        }

        /// <summary>
        /// Returns a list of MediaListItem based on specified search settings.
        /// </summary>
        /// <param name="settings">The search settings.</param>
        /// <returns>A list of MediaListItem objects.</returns>
        public List<MediaListItem> GetList(SearchSettings settings) {
            using (Entities context = contextFactory.Create()) {
                var Query = (from v in context.Media select v);

                // Apply all search filters.
                Query = ApplySearchFilters(Query, settings, context);

                // Read necessary fields.
                var Result = QueryVideoListItem(Query, settings);

                // Apply sort.
                string OrderByString = "IsInDatabase DESC, ";
                OrderByString += (string.IsNullOrEmpty(settings.OrderBy) ? "Artist" : settings.OrderBy);
                if (settings.OrderByDirection == ListSortDirection.Descending)
                    OrderByString += " DESC";
                OrderByString += ", Title";
                Result = Result.OrderBy(OrderByString);

                return Result.ToList();
            }
        }

        /// <summary>
        /// Returns the list of custom rating categories (not part of the standard ones).
        /// </summary>
        /// <returns>A list of RatingCategory objects.</returns>
        public List<RatingCategory> GetCustomRatingCategories() {
            using (Entities context = contextFactory.Create()) {
                var Query = (from c in context.RatingCategories
                             where c.Custom
                             orderby c.Name
                             select c);
                return Query.ToList();
            }
        }

        /// <summary>
        /// Returns the list of all file names stored in the database.
        /// </summary>
        /// <returns>A list of file names.</returns>
        public List<string> GetAllFileNames() {
            using (Entities context = contextFactory.Create()) {
                var Query = (from v in context.Media
                             where v.FileName != null
                             orderby v.FileName
                             select v.FileName);
                return Query.ToList();
            }
        }

        /// <summary>
        /// Applies all search filters to the video selection query.
        /// </summary>
        /// <param name="Query">The video selection query.</param>
        /// <param name="settings">The settings to apply to the query.</param>
        /// <param name="context">The data context to the database.</param>
        /// <returns>The updated query with filters applied.</returns>
        public IQueryable<Media> ApplySearchFilters(IQueryable<Media> Query, SearchSettings settings, Entities context) {
            // Media type
            if (settings.MediaType != MediaType.None)
                Query = Query.Where(v => v.MediaTypeId == (int)settings.MediaType);

            // Apply FilterKey.
            if (settings.FilterType == SearchFilterEnum.All)
                settings.IsInDatabase = true;
            else if (settings.FilterValue != null) {
                if (settings.FilterType == SearchFilterEnum.Artist)
                    Query = Query.Where(v => v.Artist == settings.FilterValue);
                else if (settings.FilterType == SearchFilterEnum.Category) {
                    if (settings.FilterValue != "")
                        Query = Query.Where(v => v.MediaCategory.Name == settings.FilterValue);
                    else
                        Query = Query.Where(v => v.MediaCategoryId == null);
                } else if (settings.FilterType == SearchFilterEnum.Element) {
                    Query = Query.Where(v => v.MediaRatings.Where(r => r.RatingCategory.Name == settings.FilterValue).Any());
                }
            } else if (settings.FilterType == SearchFilterEnum.Files)
                settings.IsInDatabase = false;
            else if (settings.FilterType == SearchFilterEnum.ArtistSingles)
                Query = Query.Where(v => (from s in context.Media
                                          group s by s.Artist into a
                                          where a.Count() == 1
                                          select a.Key).Contains(v.Artist));

            // Search string
            if (!string.IsNullOrEmpty(settings.Search))
                Query = Query.Where(v => v.Artist.Contains(settings.Search) || v.Title.Contains(settings.Search) || v.FileName.Contains(settings.Search) || v.MediaCategory.Name.Contains(settings.Search));
            // Rating category and value
            foreach (SearchRatingSetting item in settings.RatingFilters) {
                if (!string.IsNullOrEmpty(item.Category) && item.Value.HasValue) {
                    Query = Query.Where(GetFilterClause(Query, item, settings.RatingRatio, context));
                }
            }
            // Other conditions
            foreach (SearchConditionSetting item in settings.ConditionFilters) {
                if (item.Value != BoolConditionEnum.None) {
                    bool CondValue = (item.Value == BoolConditionEnum.Yes ? true : false);
                    if (item.Field == FieldConditionEnum.HasDownloadUrl)
                        Query = Query.Where(v => (v.DownloadUrl != "") == CondValue);
                    else if (item.Field == FieldConditionEnum.HasBuyOrDownloadUrl)
                        Query = Query.Where(v => (v.DownloadUrl != "" || v.BuyUrl != "") == CondValue);
                    else if (item.Field == FieldConditionEnum.PerformanceProblem)
                        Query = Query.Where(v => (v.DisableMadVr || v.DisableSvp || v.DisablePitch) == CondValue);
                    else if (item.Field == FieldConditionEnum.IsPersonal)
                        Query = Query.Where(v => v.IsPersonal == CondValue);
                }
            }
            // Buy Url Domains
            if (settings.BuyUrlDomains != null && settings.BuyUrlDomains.Count() > 0) {
                if (!settings.BuyUrlDomainsNegated)
                    Query = Query.Where(v => v.BuyUrl != "" && settings.BuyUrlDomains.Any(d => v.BuyUrl.Contains(d)));
                else
                    Query = Query.Where(v => v.BuyUrl != "" && !settings.BuyUrlDomains.Any(d => v.BuyUrl.Contains(d)));
            }
            // Has ratings
            if (settings.HasRating != HasRatingEnum.All) {
                if (settings.HasRating == HasRatingEnum.With)
                    Query = Query.Where(v => v.MediaRatings.Any());
                else if (settings.HasRating == HasRatingEnum.Without)
                    Query = Query.Where(v => !v.MediaRatings.Any());
                else if (settings.HasRating == HasRatingEnum.Incomplete && context != null)
                    Query = Query.Where(v =>
                        (v.MediaRatings.Where(r => r.Height == null || r.Depth == null).Any() ||
                        v.MediaRatings.Where(r => !r.RatingCategory.Custom).Count() < context.RatingCategories.Where(r => !r.Custom).Count()));
            }
            return Query;
        }

        /// <summary>
        /// Returns the Where clause for specified rating filter.
        /// </summary>
        /// <param name="Query">The video selection query to filter.</param>
        /// <param name="item">The rating filter to apply.</param>
        /// <param name="ratingRatio">The rating ratio.</param>
        /// <param name="context">The data context to the database.</param>
        /// <returns>The Where clause to apply to the query.</returns>
        private Expression<Func<Media, bool>> GetFilterClause(IQueryable<Media> Query, SearchRatingSetting item, double ratingRatio, Entities context) {
            Expression<Func<Media, bool>> Result;
            if (item.Category == "Length")
                Result = (v => context.CompareValues(v.Length, item.Operator, item.Value * 60));
            else if (item.Category == "Height")
                Result = (v => context.CompareValues(v.Height, item.Operator, item.Value));
            else if (item.Category == "Preference")
                Result = (v => context.CompareValues(v.Preference, item.Operator, item.Value));
            else if (item.Category == "Highest")
                Result = (v => context.CompareValues(v.MediaRatings.Max(r => r.DbGetValue(r.Height, r.Depth, ratingRatio)), item.Operator, item.Value));
            else if (item.Category.StartsWith("!Physical") || item.Category.StartsWith("!Emotional") || item.Category.StartsWith("!Spiritual")) {
                // All other polarity energies smaller than...
                string ItemCategory = item.Category.Substring(1);
                Result = (v =>
                    !(from r in v.MediaRatings
                      let val = r.DbGetValue(r.Height, r.Depth, ratingRatio)
                      where !r.RatingCategory.Name.StartsWith(ItemCategory) && r.RatingCategory.Custom == false &&
                      r.RatingCategory.Name != "Egoless" && r.RatingCategory.Name != "Love" && (
                      (item.Operator == OperatorConditionEnum.Smaller && val >= item.Value))
                      select 1).Any());
            } else if (item.Category.StartsWith("!")) {
                // All other energies smaller than...
                string ItemCategory = item.Category.Substring(1);
                Result = (v =>
                    !(from r in v.MediaRatings
                      let val = r.DbGetValue(r.Height, r.Depth, ratingRatio)
                      where r.RatingCategory.Name != ItemCategory && (
                      (item.Operator == OperatorConditionEnum.Smaller && val >= item.Value))
                      select 1).Any());
            } else if (item.Category.StartsWith("Intensity"))
                // The average of the 5 highest values.
                Result = (v =>
                    (from t in v.MediaRatings
                     let val = (from r in v.MediaRatings
                                let val = r.DbGetValue(r.Height, r.Depth, ratingRatio)
                                orderby val descending
                                select val).Take(5).Average()
                     where context.CompareValues(val, item.Operator, item.Value)
                     select 1).Any());
            else if (item.Operator != OperatorConditionEnum.Smaller) {
                // Standard rating filters.
                Result = (v =>
                    (from t in
                         (from r in v.MediaRatings
                          where r.RatingCategory.Name.StartsWith(item.Category)
                          orderby r.DbGetValue(r.Height, r.Depth, ratingRatio) descending
                          select r).Take(1)
                     let val = t.DbGetValue(t.Height, t.Depth, ratingRatio)
                     where context.CompareValues(val, item.Operator, item.Value)
                     select 1).Any());
            } else {
                // Standard rating filter with < operator.
                Result = (v =>
                    (from t in
                         (from r in v.MediaRatings
                          where r.RatingCategory.Name.StartsWith(item.Category)
                          orderby r.DbGetValue(r.Height, r.Depth, ratingRatio) descending
                          select r).Take(1)
                     let val = t.DbGetValue(t.Height, t.Depth, ratingRatio)
                     let val2 = t.DbGetValue(t.Height, t.Depth, 0) // '<' operator applies to both specified ratio and ratio 0.
                     where val >= item.Value && val2 >= item.Value
                     select 1).Any() == false);
            }

            // Apply 'or' filter.
            if (item.Or != null)
                Result = Result.OrElse(GetFilterClause(Query, item.Or, ratingRatio, context));

            return Result;
        }

        /// <summary>
        /// Converts a LINQ query into MediaListItem objects.
        /// </summary>
        /// <param name="query">The query to convert.</param>
        /// <param name="settings">The search settings.</param>
        /// <returns>A queriable list of MediaListItem.</returns>
        private IQueryable<MediaListItem> QueryVideoListItem(IQueryable<Media> query, SearchSettings settings) {
            if (settings == null)
                settings = new SearchSettings();

            var Result = (from v in query
                          select new MediaListItem() {
                              MediaId = v.MediaId,
                              MediaType = (MediaType)v.MediaTypeId,
                              Artist = v.Artist,
                              Title = v.Title,
                              Album = v.Album,
                              MediaCategoryId = v.MediaCategoryId,
                              FileName = v.FileName,
                              Preference = v.Preference,
                              Length = v.Length,
                              HasDownloadUrl = (v.DownloadUrl != ""),
                              BuyUrl = v.BuyUrl,
                              Intensity = (from r in v.MediaRatings
                                           let val = r.DbGetValue(r.Height, r.Depth, settings.RatingRatio)
                                           orderby val descending
                                           select val).Take(5).Average(),
                              Custom = string.IsNullOrEmpty(settings.CustomColumn) ? null :
                                      (from r in v.MediaRatings
                                       where r.RatingCategory.Name == settings.CustomColumn
                                       select r.DbGetValue(r.Height, r.Depth, settings.RatingRatio)).FirstOrDefault(),
                              IsInDatabase = true
                          });
            return Result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.DataAccess {

    #region Interface

    /// <summary>
    /// Provides data access to display and edit media ratings.
    /// </summary>
    public interface IMediaRatingsAccess : IDisposable {
        /// <summary>
        /// Gets the video for which to edit ratings.
        /// </summary>
        Media Video { get; }
        /// <summary>
        /// Gets or sets the Height:Depth ratio used to calculate ratings.
        /// </summary>
        double Ratio { get; set; }
        /// <summary>
        /// Returns the list of all ratings.
        /// </summary>
        List<MediaRating> RatingsList { get; }
        /// <summary>
        /// Gets or sets the Physical Masculine rating.
        /// </summary>
        MediaRating PM { get; set; }
        /// <summary>
        /// Gets or sets the Physical Feminine rating.
        /// </summary>
        MediaRating PF { get; set; }
        /// <summary>
        /// Gets or sets the Emotional Masculine rating.
        /// </summary>
        MediaRating EM { get; set; }
        /// <summary>
        /// Gets or sets the Emotional Feminine rating.
        /// </summary>
        MediaRating EF { get; set; }
        /// <summary>
        /// Gets or sets the Spiritual Masculine rating.
        /// </summary>
        MediaRating SM { get; set; }
        /// <summary>
        /// Gets or sets the Spiritual Feminine rating.
        /// </summary>
        MediaRating SF { get; set; }
        /// <summary>
        /// Gets or sets the Love rating.
        /// </summary>
        MediaRating Love { get; set; }
        /// <summary>
        /// Gets or sets the Egoless rating.
        /// </summary>
        MediaRating Egoless { get; set; }
        /// <summary>
        /// Gets or sets the first custom rating name.
        /// </summary>
        string Custom1Text { get; set; }
        /// <summary>
        /// Gets or sets the first custom rating value.
        /// </summary>
        MediaRating Custom1 { get; set; }
        /// <summary>
        /// Gets or sets the second custom rating name.
        /// </summary>
        string Custom2Text { get; set; }
        /// <summary>
        /// Gets or sets the second custom rating value.
        /// </summary>
        MediaRating Custom2 { get; set; }
        /// <summary>
        /// Returns the rating with specified name.
        /// </summary>
        /// <param name="ratingName">The name of the rating to return.</param>
        /// <returns>A MediaRating object.</returns>
        MediaRating GetRatingRow(string ratingName);
        /// <summary>
        /// Returns the Intensity value, which is the average of the 5 highest ratings.
        /// </summary>
        /// <returns>The calculated intensity value, or null if there are no ratings.</returns>
        double? GetIntensity();
        /// <summary>
        /// Updates the ratings back into the Media object.
        /// </summary>
        /// <param name="commit">Whether to commit the changes into the database.</param>
        void UpdateChanges(bool commit);
        /// <summary>
        /// Adds a new rating to the list.
        /// </summary>
        /// <param name="name">The name of the rating.</param>
        /// <param name="rating">The rating value.</param>
        /// <param name="categories">A list of rating categories.</param>
        void AddRating(string name, MediaRating rating, List<RatingCategory> categories);
        /// <summary>
        /// Saves updated preference field. Only available if ContextFactory is set in the constructor.
        /// </summary>
        /// <param name="video">The media object to update.</param>
        void SavePreference(Media item);
    }

    #endregion

    /// <summary>
    /// Provides data access to display and edit media ratings.
    /// </summary>
    public class MediaRatingsAccess : IMediaRatingsAccess {

        #region Declarations / Constructors

        /// <summary>
        /// Gets the video for which to edit ratings.
        /// </summary>
        public Media Video { get; private set; }
        /// <summary>
        /// Gets or sets the Height:Depth ratio used to calculate ratings.
        /// </summary>
        public double Ratio { get; set; }
        private INgpContextFactory contextFactory;
        private Entities context;

        public MediaRatingsAccess() : this(new Media(), 1, new NgpContextFactory()) { }

        public MediaRatingsAccess(Media video, double ratio, INgpContextFactory contextFactory) : this(video, ratio, contextFactory.Create()) {
            this.contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public MediaRatingsAccess(Media video, double ratio, Entities context) {
            this.Video = video ?? throw new ArgumentNullException(nameof(video));
            this.Ratio = ratio;
            this.context = context ?? throw new ArgumentNullException(nameof(context));
            if (video.MediaId != null && video.MediaId != Guid.Empty)
                DisplayData();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the list of all ratings.
        /// </summary>
        public List<MediaRating> RatingsList { get; private set; }
        /// <summary>
        /// Gets or sets the Physical Masculine rating.
        /// </summary>
        public MediaRating PM { get; set; }
        /// <summary>
        /// Gets or sets the Physical Feminine rating.
        /// </summary>
        public MediaRating PF { get; set; }
        /// <summary>
        /// Gets or sets the Emotional Masculine rating.
        /// </summary>
        public MediaRating EM { get; set; }
        /// <summary>
        /// Gets or sets the Emotional Feminine rating.
        /// </summary>
        public MediaRating EF { get; set; }
        /// <summary>
        /// Gets or sets the Spiritual Masculine rating.
        /// </summary>
        public MediaRating SM { get; set; }
        /// <summary>
        /// Gets or sets the Spiritual Feminine rating.
        /// </summary>
        public MediaRating SF { get; set; }
        /// <summary>
        /// Gets or sets the Love rating.
        /// </summary>
        public MediaRating Love { get; set; }
        /// <summary>
        /// Gets or sets the Egoless rating.
        /// </summary>
        public MediaRating Egoless { get; set; }
        /// <summary>
        /// Gets or sets the first custom rating name.
        /// </summary>
        public string Custom1Text { get; set; }
        /// <summary>
        /// Gets or sets the first custom rating value.
        /// </summary>
        public MediaRating Custom1 { get; set; }
        /// <summary>
        /// Gets or sets the second custom rating name.
        /// </summary>
        public string Custom2Text { get; set; }
        /// <summary>
        /// Gets or sets the second custom rating value.
        /// </summary>
        public MediaRating Custom2 { get; set; }

        #endregion

        /// <summary>
        /// Initializes the ratings properties.
        /// </summary>
        private void DisplayData() {
            RatingsList = Video.MediaRatings.ToList();
            PM = GetRatingRow("Physical Masculine");
            PF = GetRatingRow("Physical Feminine");
            EM = GetRatingRow("Emotional Masculine");
            EF = GetRatingRow("Emotional Feminine");
            SM = GetRatingRow("Spiritual Masculine");
            SF = GetRatingRow("Spiritual Feminine");
            Love = GetRatingRow("Love");
            Egoless = GetRatingRow("Egoless");
            var CustomCategories = Video.MediaRatings.Where(r => r.RatingCategory.Custom).OrderBy(r => r.RatingCategory.Name);
            Custom1 = CustomCategories.FirstOrDefault();
            if (Custom1 != null)
                Custom2 = CustomCategories.Skip(1).FirstOrDefault();
            if (Custom1 != null)
                Custom1Text = Custom1.RatingCategory.Name;
            else
                Custom1 = new MediaRating();
            if (Custom2 != null)
                Custom2Text = Custom2.RatingCategory.Name;
            else
                Custom2 = new MediaRating();
        }

        /// <summary>
        /// Returns the rating with specified name.
        /// </summary>
        /// <param name="ratingName">The name of the rating to return.</param>
        /// <returns>A MediaRating object.</returns>
        public MediaRating GetRatingRow(string ratingName) {
            return RatingsList.Where(r => r.RatingCategory.Name == ratingName).FirstOrDefault() ?? new MediaRating();
        }

        /// <summary>
        /// Returns the Intensity value, which is the average of the 5 highest ratings.
        /// </summary>
        /// <returns>The calculated intensity value, or null if there are no ratings.</returns>
        public double? GetIntensity() {
            if (RatingsList.Count > 0)
                return Math.Round(HighestValues().Average(), 1);
            else
                return null;
        }

        /// <summary>
        /// Returns a list of the 5 highest rating values.
        /// </summary>
        private List<double> HighestValues() {
            return (from r in RatingsList
                    let val = r.GetValue(Ratio)
                    where val != null
                    orderby val descending
                    select val.Value).Take(5).ToList();
        }

        /// <summary>
        /// Updates the ratings back into the Media object.
        /// </summary>
        /// <param name="commit">Whether to commit the changes into the database.</param>
        public void UpdateChanges(bool commit) {
            List<RatingCategory> Categories = context.RatingCategories.ToList();
            Video.MediaRatings.Clear();
            AddRating("Physical Masculine", PM, Categories);
            AddRating("Physical Feminine", PF, Categories);
            AddRating("Emotional Masculine", EM, Categories);
            AddRating("Emotional Feminine", EF, Categories);
            AddRating("Spiritual Masculine", SM, Categories);
            AddRating("Spiritual Feminine", SF, Categories);
            AddRating("Love", Love, Categories);
            AddRating("Egoless", Egoless, Categories);
            AddRating(Custom1Text, Custom1, Categories);
            AddRating(Custom2Text, Custom2, Categories);
            if (commit)
                context.SaveChanges();
        }

        /// <summary>
        /// Adds a new rating to the list.
        /// </summary>
        /// <param name="name">The name of the rating.</param>
        /// <param name="rating">The rating value.</param>
        /// <param name="categories">A list of rating categories.</param>
        public void AddRating(string name, MediaRating rating, List<RatingCategory> categories) {
            if (!string.IsNullOrEmpty(name) && (rating.Height.HasValue || rating.Depth.HasValue)) {
                RatingCategory Rating = categories.Find(c => c.Name.ToLower() == name.ToLower());
                if (Rating != null)
                    Video.MediaRatings.Add(new MediaRating() { RatingCategory = Rating, Height = rating.Height, Depth = rating.Depth });
            }
        }

        /// <summary>
        /// Saves updated preference field. Only available if ContextFactory is set in the constructor.
        /// </summary>
        /// <param name="video">The media object to update.</param>
        public void SavePreference(Media item) {
            if (contextFactory == null)
                throw new NullReferenceException("This method is only available if ContextFactory is set in the constructor.");
            if (item.MediaId != Guid.Empty) {
                using (var context = contextFactory.Create()) {
                    Media Item = context.Media.Where(v => v.MediaId == item.MediaId).Single();
                    Item.Preference = item.Preference;
                    Item.IsCustomPreference = (item.Preference.HasValue);
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Disposes the context only if it was created from the ContextFactory.
        /// </summary>
        public void Dispose() {
            if (contextFactory != null)
                context?.Dispose();
        }
    }
}

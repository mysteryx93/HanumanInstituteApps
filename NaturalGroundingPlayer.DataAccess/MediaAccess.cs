using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EmergenceGuardian.NaturalGroundingPlayer.DataAccess {

    // Note: This is NOT a Singleton class, each instance must be unique.

    #region Interface

    /// <summary>
    /// Provides data access to view and edit videos.
    /// </summary>
    public interface IMediaAccess : IDisposable {
        /// <summary>
        /// Returns a Media object from its ID.
        /// </summary>
        /// <param name="videoId">The ID of the media to return.</param>
        /// <returns>A Media object.</returns>
        Media GetMediaById(Guid videoId);
        /// <summary>
        /// Returns a Media object from its file name.
        /// </summary>
        /// <param name="fileName">The file name of the media to return.</param>
        /// <returns>A Media object.</returns>
        Media GetMediaByFileName(string fileName);
        /// <summary>
        /// Returns a Media object from its title.
        /// </summary>
        /// <param name="mediaType">The type of media to return.</param>
        /// <param name="artist">The media's artist.</param>
        /// <param name="title">The media's title.</param>
        /// <returns>The matching Media object, if found.</returns>
        Media GetMediaByTitle(MediaType mediaType, string artist, string title);
        /// <summary>
        /// Returns the list of media categories for specified media type.
        /// </summary>
        /// <param name="mediaTypeId">The media type for which to return the categories.</param>
        /// <returns>A list of categories.</returns>
        List<KeyValuePair<Guid?, string>> GetCategories(int mediaTypeId);
        /// <summary>
        /// Returns the list of custom rating categories.
        /// </summary>
        List<RatingCategory> GetCustomRatingCategories();
        /// <summary>
        /// Returns a new Media object and adds it to the data context so it can later be saved.
        /// </summary>
        Media NewMedia();
        /// <summary>
        /// Adds specified Media to the data context and generates a new ID to avoid conflicting with existing items.
        /// </summary>
        /// <param name="media">The media to add to the data context..</param>
        void AddMedia(Media media);
        /// <summary>
        /// Returns whether the Title and Artist of specified Media is already in the database.
        /// </summary>
        /// <param name="media">The media for which to look for an existing duplicate.</param>
        /// <returns>True if Artist and Title was found, otherwise false.</returns>
        bool IsTitleDuplicate(Media media);
        /// <summary>
        /// Saves the changes back into the database.
        /// </summary>
        void Save();
        /// <summary>
        /// Deletes specified media.
        /// </summary>
        /// <param name="media">The Media to delete.</param>
        void Delete(Media media);
    }

    #endregion

    /// <summary>
    /// Provides data access to view and edit videos.
    /// </summary>
    public class MediaAccess : IMediaAccess {

        #region Declarations / Constructors

        private Entities context;

        private INgpContextFactory contextFactory;

        public MediaAccess() : this(new NgpContextFactory()) { }

        public MediaAccess(INgpContextFactory contextFactory) {
            this.contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            this.context = contextFactory.Create(true);
        }

        #endregion

        /// <summary>
        /// Returns a Media object from its ID.
        /// </summary>
        /// <param name="videoId">The ID of the media to return.</param>
        /// <returns>A Media object.</returns>
        public Media GetMediaById(Guid videoId) {
            Media Result = (from v in context.Media
                            where v.MediaId == videoId
                            select v).Include(v => v.MediaRatings.Select(r => r.RatingCategory)).FirstOrDefault();
            return Result;
        }

        /// <summary>
        /// Returns a Media object from its file name.
        /// </summary>
        /// <param name="fileName">The file name of the media to return.</param>
        /// <returns>A Media object.</returns>
        public Media GetMediaByFileName(string fileName) {
            Media Result = (from v in context.Media
                            where v.FileName == fileName
                            select v).Include(v => v.MediaRatings.Select(r => r.RatingCategory)).FirstOrDefault();
            return Result;
        }

        /// <summary>
        /// Returns a Media object from its title.
        /// </summary>
        /// <param name="mediaType">The type of media to return.</param>
        /// <param name="artist">The media's artist.</param>
        /// <param name="title">The media's title.</param>
        /// <returns>The matching Media object, if found.</returns>
        public Media GetMediaByTitle(MediaType mediaType, string artist, string title) {
            Media Result = (from v in context.Media
                            where v.MediaTypeId == (int)mediaType && v.Artist == artist && v.Title == title
                            select v).Include(v => v.MediaRatings.Select(r => r.RatingCategory)).FirstOrDefault();
            return Result;
        }

        /// <summary>
        /// Returns the list of media categories for specified media type.
        /// </summary>
        /// <param name="mediaTypeId">The media type for which to return the categories.</param>
        /// <returns>A list of categories.</returns>
        public List<KeyValuePair<Guid?, string>> GetCategories(int mediaTypeId) {
            var Query = (from c in context.MediaCategories
                         where c.MediaTypeId == mediaTypeId
                         orderby c.Folder
                         select new { c.MediaCategoryId, c.Name });
            var Result = Query.AsEnumerable().Select(i => new KeyValuePair<Guid?, string>(i.MediaCategoryId, i.Name)).ToList();
            Result.Insert(0, new KeyValuePair<Guid?, string>(null, ""));
            return Result;
        }

        /// <summary>
        /// Returns the list of custom rating categories.
        /// </summary>
        public List<RatingCategory> GetCustomRatingCategories() {
            var Result = (from c in context.RatingCategories
                          where c.Custom == true
                          orderby c.Name
                          select c).ToList();
            Result.Insert(0, new RatingCategory());
            return Result;
        }

        /// <summary>
        /// Returns a new Media object and adds it to the data context so it can later be saved.
        /// </summary>
        public Media NewMedia() {
            Media Result = new Media();
            Result.MediaId = Guid.NewGuid();
            context.Media.Add(Result);
            return Result;
        }

        /// <summary>
        /// Adds specified Media to the data context and generates a new ID to avoid conflicting with existing items.
        /// </summary>
        /// <param name="media">The media to add to the data context..</param>
        public void AddMedia(Media media) {
            media.MediaId = Guid.NewGuid();
            context.Media.Add(media);
        }

        /// <summary>
        /// Returns whether the Title and Artist of specified Media is already in the database.
        /// </summary>
        /// <param name="media">The media for which to look for an existing duplicate.</param>
        /// <returns>True if Artist and Title was found, otherwise false.</returns>
        public bool IsTitleDuplicate(Media media) {
            bool IsDuplicate = (from v in context.Media
                                where v.MediaId != media.MediaId &&
                                    v.MediaTypeId == media.MediaTypeId &&
                                    (media.Artist == null || v.Artist == media.Artist) &&
                                    (media.Title == null || v.Title == media.Title)
                                select v.MediaId).Any();
            return IsDuplicate;
        }

        /// <summary>
        /// Saves the changes back into the database.
        /// </summary>
        public void Save() {
            // Set IsCustomPreference field when updating Preference.
            foreach (var item in context.ChangeTracker.Entries<Media>().Where(e => e.State == EntityState.Modified)) {
                double? OldValue = item.OriginalValues["Preference"] as double?;
                double? NewValue = item.Entity.Preference;
                if (OldValue != NewValue)
                    item.Entity.IsCustomPreference = (NewValue.HasValue);
            }

            context.SaveChanges();
        }

        /// <summary>
        /// Deletes specified media.
        /// </summary>
        /// <param name="media">The Media to delete.</param>
        public void Delete(Media media) {
            context.Media.Remove(media);
            context.SaveChanges();
        }

        /// <summary>
        /// Disposes of the data context.
        /// </summary>
        public void Dispose() {
            context?.Dispose();
        }
    }
}

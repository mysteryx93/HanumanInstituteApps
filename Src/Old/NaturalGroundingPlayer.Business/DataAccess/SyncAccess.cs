using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace HanumanInstitute.NaturalGroundingPlayer.DataAccess {

    #region Interface

    /// <summary>
    /// Provides data access for synchronization features.
    /// </summary>
    public interface ISyncAccess {
        /// <summary>
        /// Returns the list of media categories.
        /// </summary>
        List<MediaCategory> GetMediaCategories();
        /// <summary>
        /// Returns the list of rating categories.
        /// </summary>
        List<RatingCategory> GetRatingCategories();
        /// <summary>
        /// Returns a list of media matching specified list of media IDs.
        /// </summary>
        /// <param name="exportList">The list of IDs to return.</param>
        /// <returns>A list of media.</returns>
        List<Media> GetMediaListById(List<Guid> exportList);
        /// <summary>
        /// Imports the data previously loaded into the database.
        /// </summary>
        /// <param name="selection">The IDs of the elements to import.</param>
        /// <param name="importList">The content of the imported file.</param>
        /// <param name="progress">Reports the progress of the operation. First report is the amount of files to process, then subsequent reports represent the quantity done.</param>
        void Import(List<Guid> selection, List<Media> importList, IProgress<int> progress);
    }

    #endregion

    /// <summary>
    /// Provides data access for synchronization features.
    /// </summary>
    public class SyncAccess : ISyncAccess {

        #region Declarations / Constructors

        private INgpContextFactory contextFactory;

        public SyncAccess() : this(new NgpContextFactory()) { }

        public SyncAccess(INgpContextFactory contextFactory) {
            this.contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        #endregion

        /// <summary>
        /// Returns the list of media categories.
        /// </summary>
        public List<MediaCategory> GetMediaCategories() {
            using (Entities context = contextFactory.Create()) {
                return context.MediaCategories.ToList();
            }
        }

        /// <summary>
        /// Returns the list of rating categories.
        /// </summary>
        public List<RatingCategory> GetRatingCategories() {
            using (Entities context = contextFactory.Create()) {
                return context.RatingCategories.ToList();
            }
        }

        /// <summary>
        /// Returns a list of media matching specified list of media IDs.
        /// </summary>
        /// <param name="exportList">The list of IDs to return.</param>
        /// <returns>A list of media.</returns>
        public List<Media> GetMediaListById(List<Guid> exportList) {
            using (Entities context = contextFactory.Create()) {
                var MediaData = (from v in context.Media
                                    .Include(t => t.MediaRatings.Select(r => r.RatingCategory))
                                    .Include(t => t.MediaCategory)
                                 where exportList.Contains(v.MediaId)
                                 orderby v.Artist, v.Title
                                 select v).ToList();
                return MediaData;
            }
        }

        /// <summary>
        /// Imports the data previously loaded into the database.
        /// </summary>
        /// <param name="selection">The IDs of the elements to import.</param>
        /// <param name="importList">The content of the imported file.</param>
        /// <param name="progress">Reports the progress of the operation. First report is the amount of files to process, then subsequent reports represent the quantity done.</param>
        public void Import(List<Guid> selection, List<Media> importList, IProgress<int> progress) {
            int i = 0;
            Media item;
            using (Entities context = contextFactory.Create()) {
                DbContextTransaction Trans = context.Database.BeginTransaction();
                foreach (Guid id in selection) {
                    item = importList.Single(v => v.MediaId == id);
                    item.IsPersonal = true;
                    InsertOrMerge(item, context);
                    if (progress != null)
                        progress.Report(++i);
                }
                Trans.Commit();
            }
        }

        /// <summary>
        /// Inserts specified media into the database, or updates an existing entry.
        /// </summary>
        /// <param name="item">The media item to merge into the database.</param>
        /// <param name="context">The database context.</param>
        private void InsertOrMerge(Media item, Entities context) {
            // Try to find matching element already within the database.
            List<Media> DbMatch = (from m in context.Media
                                   where m.MediaId == item.MediaId || (m.Artist == item.Artist && m.Title == item.Title)
                                   select m).ToList();
            Media DbItem;
            if (DbMatch.Count > 1) // If both matches happen simultaneously, Artist/Title must take priority to avoid creating duplicate.
                DbItem = DbMatch.Single(m => m.Artist == item.Artist && m.Title == item.Title);
            else
                DbItem = DbMatch.FirstOrDefault();

            // If merging, delete existing entry and we'll create a new one and merge the fields.
            if (DbItem != null)
                context.Media.Remove(DbItem);

            // Insert new data.
            context.Media.Add(item);

            // Merge fields.
            if (DbItem != null) {
                item.FileName = DbItem.FileName;
                item.MediaId = DbItem.MediaId; // If matching by Artist/Title, we keep the same ID to avoid creating duplicate ID.
            }

            // Commit changes.
            context.SaveChanges();
        }
    }
}

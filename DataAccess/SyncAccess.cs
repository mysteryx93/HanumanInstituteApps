using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public static class SyncAccess {
        public static List<MediaCategory> GetMediaCategories() {
            using (Entities context = new Entities()) {
                return context.MediaCategories.ToList();
            }
        }

        public static List<RatingCategory> GetRatingCategories() {
            using (Entities context = new Entities()) {
                return context.RatingCategories.ToList();
            }
        }

        public static List<Media> GetMediaListById(List<Guid> exportList) {
            using (Entities context = new Entities()) {
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
        /// Inserts specified media into the database, or updates an existing entry.
        /// </summary>
        /// <param name="item">The media item to merge into the database.</param>
        /// <param name="context">The database context.</param>
        public static void InsertOrMerge(Media item, Entities context) {
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

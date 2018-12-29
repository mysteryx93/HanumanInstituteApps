using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace Business {
    public class EditVideoBusiness {
        Entities context = new Entities();

        public EditVideoBusiness() {
        }

        public Media GetVideoById(Guid videoId) {
            Media Result = (from v in context.Media
                            where v.MediaId == videoId
                            select v).Include(v => v.MediaRatings.Select(r => r.RatingCategory)).FirstOrDefault();
            return Result;
        }

        public Media GetVideoByFileName(string fileName) {
            Media Result = (from v in context.Media
                            where v.FileName == fileName
                            select v).Include(v => v.MediaRatings.Select(r => r.RatingCategory)).FirstOrDefault();
            return Result;
        }

        public Media GetVideoByTitle(MediaType mediaType, string artist, string title) {
            Media Result = (from v in context.Media
                            where v.MediaTypeId == (int)mediaType && v.Artist == artist && v.Title == title
                            select v).Include(v => v.MediaRatings.Select(r => r.RatingCategory)).FirstOrDefault();
            return Result;
        }

        public List<KeyValuePair<Guid?, string>> GetCategories(int mediaTypeId) {
            var Query = (from c in context.MediaCategories
                          where c.MediaTypeId == mediaTypeId
                          orderby c.Folder
                          select new { c.MediaCategoryId, c.Name });
            var Result = Query.AsEnumerable().Select(i => new KeyValuePair<Guid?, string>(i.MediaCategoryId, i.Name)).ToList();
            Result.Insert(0, new KeyValuePair<Guid?, string>(null, ""));
            return Result;
        }

        public List<RatingCategory> GetCustomRatingCategories() {
            var Result = (from c in context.RatingCategories
                          where c.Custom == true
                          orderby c.Name
                          select c).ToList();
            Result.Insert(0, new RatingCategory());
            return Result;
        }

        public EditRatingsBusiness GetRatings(Media video) {
            return new EditRatingsBusiness(video, context);
        }

        public Media NewVideo() {
            Media Result = new Media();
            Result.MediaId = Guid.NewGuid();
            context.Media.Add(Result);
            return Result;
        }

        public void AddVideo(Media video) {
            video.MediaId = Guid.NewGuid();
            context.Media.Add(video);
        }

        public bool IsTitleDuplicate(Media video) {
            bool IsDuplicate = (from v in context.Media
                                where v.MediaId != video.MediaId &&
                                    v.MediaTypeId == video.MediaTypeId &&
                                    (video.Artist == null || v.Artist == video.Artist) &&
                                    (video.Title == null || v.Title == video.Title)
                                select v.MediaId).Any();
            return IsDuplicate;
        }

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

        public void Delete(Media video) {
            context.Media.Remove(video);
            context.SaveChanges();
        }

        public void DeleteFile(string fileName) {
            File.Delete(Settings.NaturalGroundingFolder + fileName);
        }

        public static MediaType GetFileType(string fileName) {
            MediaType Result = MediaType.None;
            string Ext = Path.GetExtension(fileName).ToLower();
            if (AppPaths.VideoExtensions.Contains(Ext))
                Result = MediaType.Video;
            else if (AppPaths.AudioExtensions.Contains(Ext))
                Result = MediaType.Audio;
            else if (AppPaths.ImageExtensions.Contains(Ext))
                Result = MediaType.Image;
            return Result;
        }
    }
}

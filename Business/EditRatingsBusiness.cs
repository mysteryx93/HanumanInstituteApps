using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace Business {
    public class EditRatingsBusiness {
        private Entities context;
        private Media video;
        public double Ratio { get; set; }
        public int MediaId { get; set; }
        public List<MediaRating> RatingsList;
        public MediaRating PM { get; set; }
        public MediaRating PF { get; set; }
        public MediaRating EM { get; set; }
        public MediaRating EF { get; set; }
        public MediaRating SM { get; set; }
        public MediaRating SF { get; set; }
        public MediaRating Love { get; set; }
        public MediaRating Egoless { get; set; }
        public string Custom1Text { get; set; }
        public MediaRating Custom1 { get; set; }
        public string Custom2Text { get; set; }
        public MediaRating Custom2 { get; set; }

        public EditRatingsBusiness(Media video, double ratio) {
            this.video = video;
            this.Ratio = ratio;
            DisplayData();
        }

        public Media Video {
            get { return video; }
        }

        public EditRatingsBusiness(Media video, Entities context) {
            this.context = context;
            this.video = video;
            if (video.MediaId != null && video.MediaId != Guid.Empty)
                DisplayData();
        }

        private void DisplayData() {
            RatingsList = video.MediaRatings.ToList();
            PM = GetRatingRow("Physical Masculine");
            PF = GetRatingRow("Physical Feminine");
            EM = GetRatingRow("Emotional Masculine");
            EF = GetRatingRow("Emotional Feminine");
            SM = GetRatingRow("Spiritual Masculine");
            SF = GetRatingRow("Spiritual Feminine");
            Love = GetRatingRow("Love");
            Egoless = GetRatingRow("Egoless");
            var CustomCategories = video.MediaRatings.Where(r => r.RatingCategory.Custom).OrderBy(r => r.RatingCategory.Name);
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

        public MediaRating GetRatingRow(string ratingName) {
            MediaRating Result = RatingsList.Where(r => r.RatingCategory.Name == ratingName).FirstOrDefault();
            if (Result == null)
                Result = new MediaRating();
            return Result;
        }

        public double? GetIntensity() {
            if (RatingsList.Count > 0)
                return Math.Round(HighestValues().Average(), 1);
            else
                return null;
        }

        public List<double> HighestValues() {
            return (from r in RatingsList
                    let val = r.GetValue(Ratio)
                    where val != null
                    orderby val descending
                    select val.Value).Take(5).ToList();
        }

        public void UpdateChanges() {
            List<RatingCategory> Categories = context.RatingCategories.ToList();
            video.MediaRatings.Clear();
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
        }

        public void AddRating(string name, MediaRating rating, List<RatingCategory> categories) {
            if (!string.IsNullOrEmpty(name) && (rating.Height.HasValue || rating.Depth.HasValue)) {
                RatingCategory Rating = categories.Find(c => c.Name.ToLower() == name.ToLower());
                //if (Rating == null) {
                //    Rating = new RatingCategory() { Name = name, Custom = true };
                //    context.RatingCategories.Add(Rating);
                //}
                if (Rating != null)
                    video.MediaRatings.Add(new MediaRating() { RatingCategory = Rating, Height = rating.Height, Depth = rating.Depth });
            }
        }

        /// <summary>
        /// Saves updated preference field.
        /// </summary>
        /// <param name="video">The media object to update.</param>
        public void UpdatePreference(Media item) {
            if (item.MediaId != Guid.Empty) {
                using (var context = new Entities()) {
                    Media Item = context.Media.Where(v => v.MediaId == item.MediaId).Single();
                    Item.Preference = item.Preference;
                    Item.IsCustomPreference = (item.Preference.HasValue);
                    context.SaveChanges();
                }
            }
        }
    }
}

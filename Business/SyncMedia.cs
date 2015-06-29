using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DataAccess;

namespace Business {
    [XmlType("Media")]
    public class SyncMedia {
        public Guid MediaId { get; set; }
        public MediaType MediaType { get; set; }
        [DefaultValue("")]
        public string Artist { get; set; }
        public string Title { get; set; }
        [DefaultValue("")]
        public string Album { get; set; }
        public DateTime? EditedOn { get; set; }
        public bool ShouldSerializeDateEdited() {
            return EditedOn.HasValue;
        }
        [DefaultValue("")]
        public string Category { get; set; }
        [DefaultValue(null)]
        public double? Preference { get; set; }
        public bool ShouldSerializePreference() {
            return Preference.HasValue;
        }
        public short? Length { get; set; }
        public bool ShouldSerializeLength() {
            return Length.HasValue;
        }
        public short? StartPos { get; set; }
        public bool ShouldSerializeStartPos() {
            return StartPos.HasValue;
        }
        public short? EndPos { get; set; }
        public bool ShouldSerializeEndPos() {
            return EndPos.HasValue;
        }
        [DefaultValue("")]
        public string DownloadName { get; set; }
        [DefaultValue("")]
        public string DownloadUrl { get; set; }
        [DefaultValue("")]
        public string BuyUrl { get; set; }
        [DefaultValue(false)]
        public bool DisableSvp { get; set; }
        [DefaultValue(false)]
        public bool DisableMadVr { get; set; }
        [XmlElement("Rating")]
        public List<SyncRating> Ratings { get; set; }

        public SyncMedia() {
        }

        public SyncMedia(Media source) {
            MediaId = source.MediaId;
            EditedOn = source.EditedOn;
            MediaType = (MediaType)source.MediaTypeId;
            Artist = source.Artist;
            Album = source.Album;
            Title = source.Title;
            if (source.MediaCategoryId.HasValue)
                Category = source.MediaCategory.Name;
            Preference = source.Preference;
            Length = source.Length;
            StartPos = source.StartPos;
            EndPos = source.EndPos;
            DownloadName = source.DownloadName;
            DownloadUrl = source.DownloadUrl;
            BuyUrl = source.BuyUrl;

            Ratings = new List<SyncRating>();
            foreach (MediaRating item in source.MediaRatings) {
                Ratings.Add(new SyncRating(item.RatingCategory.Name, item.Height, item.Depth));
            }
        }

        public Media ConvertToMedia(List<MediaCategory> mediaCategories, List<RatingCategory> ratingCategories) {
            Media Result = new Media();
            Result.MediaId = MediaId != Guid.Empty ? MediaId : Guid.NewGuid();
            Result.EditedOn = EditedOn ?? DateTime.Now;
            Result.MediaType = MediaType;
            Result.Artist = Artist ?? string.Empty;
            Result.Album = Album ?? string.Empty;
            Result.Title = Title;
            if (!string.IsNullOrEmpty(Category)) {
                MediaCategory ItemCategory = mediaCategories.FirstOrDefault(v => v.Name == Category && v.MediaTypeId == (int)MediaType);
                if (ItemCategory != null)
                    Result.MediaCategoryId = ItemCategory.MediaCategoryId;
            }
            Result.Preference = Preference;
            Result.Length = Length;
            Result.StartPos = StartPos;
            Result.EndPos = EndPos;
            Result.DownloadName = DownloadName ?? string.Empty;
            Result.DownloadUrl = DownloadUrl ?? string.Empty;
            Result.BuyUrl = BuyUrl ?? string.Empty;

            MediaRating NewRating;
            foreach (SyncRating item in Ratings) {
                NewRating = new MediaRating();
                NewRating.MediaId = Result.MediaId;
                NewRating.RatingId = ratingCategories.FirstOrDefault(r => r.Name == item.Name).RatingId;
                NewRating.Height = (item.Height != -1 ? (double?)item.Height : null);
                NewRating.Depth = (item.Depth != -1 ? (double?)item.Depth : null);
                Result.MediaRatings.Add(NewRating);
            }

            return Result;
        }
    }

    [XmlRoot("Rating")]
    public class SyncRating {
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlAttribute("Height"), DefaultValue(-1)]
        public double Height { get; set; }
        [XmlAttribute("Depth"), DefaultValue(-1)]
        public double Depth { get; set; }

        public SyncRating() {
        }

        public SyncRating(string name, double? height, double? depth) {
            this.Name = name;
            this.Height = height ?? -1;
            this.Depth = depth ?? -1;
        }
    }
}

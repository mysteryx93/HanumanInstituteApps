using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DataAccess {
    [XmlType("Media")]
    public class MediaSync {
        public MediaType MediaType { get; set; }
        [DefaultValue("")]
        public string Artist { get; set; }
        public string Title { get; set; }
        [DefaultValue("")]
        public string Album { get; set; }
        public DateTime? DateEdited { get; set; }
        public bool ShouldSerializeDateEdited() {
            return DateEdited.HasValue;
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
        [XmlElement("Rating")]
        public List<MediaRatingSync> Ratings { get; set; }

        public MediaSync() {
        }

        public MediaSync(Media item) {
            DateEdited = item.EditedOn;
            MediaType = (MediaType)item.MediaTypeId;
            Artist = item.Artist;
            Album = item.Album;
            Title = item.Title;
            Category = item.MediaCategory.Name;
            Preference = item.Preference;
            Length = item.Length;
            StartPos = item.StartPos;
            EndPos = item.EndPos;
            DownloadName = item.DownloadName;
            DownloadUrl = item.DownloadUrl;
            BuyUrl = item.BuyUrl;

            Ratings = new List<MediaRatingSync>();
            foreach (MediaRating r in item.MediaRatings) {
                Ratings.Add(new MediaRatingSync(r.RatingCategory.Name, r.Height, r.Depth));
            }
        }
    }

    [XmlRoot("Rating")]
    public class MediaRatingSync {
        [XmlAttribute("Name")]
        public string Name { get; set; }
        [XmlAttribute("Height")]
        public double? Height { get; set; }
        [XmlAttribute("Depth")]
        public double? Depth { get; set; }

        public MediaRatingSync() {
        }

        public MediaRatingSync(string name, double? height, double? depth) {
            this.Name = name;
            this.Height = height;
            this.Depth = depth;
        }
    }
}

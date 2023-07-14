using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HanumanInstitute.NaturalGroundingPlayer.Models
{
    /// <summary>
    /// Contains the information of a Media object that can be serialized and exported.
    /// </summary>
    [XmlType("Media")]
    public class SyncMedia
    {
        public Guid MediaId { get; set; }
        public MediaType MediaType { get; set; }
        [DefaultValue("")]
        public string Artist { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        [DefaultValue("")]
        public string Album { get; set; } = string.Empty;
        public DateTime? EditedOn { get; set; }
        public bool ShouldSerializeDateEdited()
        {
            return EditedOn.HasValue;
        }
        [DefaultValue(null)]
        public double? Preference { get; set; }
        public bool ShouldSerializePreference()
        {
            return Preference.HasValue;
        }
        public short? Length { get; set; }
        public bool ShouldSerializeLength()
        {
            return Length.HasValue;
        }
        public short? StartPos { get; set; }
        public bool ShouldSerializeStartPos()
        {
            return StartPos.HasValue;
        }
        public short? EndPos { get; set; }
        public bool ShouldSerializeEndPos()
        {
            return EndPos.HasValue;
        }
        [DefaultValue("")]
        public string DownloadName { get; set; } = string.Empty;
        [DefaultValue("")]
        public string DownloadUrl { get; set; } = string.Empty;
        [DefaultValue("")]
        public string BuyUrl { get; set; } = string.Empty;
        [DefaultValue(false)]
        public bool DisableSvp { get; set; }
        [DefaultValue(false)]
        public bool DisableMadVr { get; set; }
        [XmlElement("Rating")]
        public IList<SyncRating>? Ratings { get; set; }

        public SyncMedia() { }
    }
}

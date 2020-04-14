using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {
    /// <summary>
    /// Contains the information of a Media object that can be serialized and exported.
    /// </summary>
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

        public SyncMedia() { }
    }
}

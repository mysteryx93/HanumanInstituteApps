using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace HanumanInstitute.NaturalGroundingPlayer.Models
{
    /// <summary>
    /// Contains the information of a MediaRating that can be serialized and exported.
    /// </summary>
    [XmlRoot("Rating")]
    public class SyncRating
    {
        [XmlAttribute("Name")]
        public string Name { get; set; } = string.Empty;
        [XmlAttribute("Height"), DefaultValue(-1)]
        public double Height { get; set; }
        [XmlAttribute("Depth"), DefaultValue(-1)]
        public double Depth { get; set; }

        public SyncRating() { }

        public SyncRating(string name, double? height, double? depth)
        {
            this.Name = name;
            this.Height = height ?? -1;
            this.Depth = depth ?? -1;
        }
    }
}

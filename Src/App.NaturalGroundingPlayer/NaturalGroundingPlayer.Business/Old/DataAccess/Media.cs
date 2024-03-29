//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HanumanInstitute.NaturalGroundingPlayer.DataAccess
{
    using System;
    using System.Collections.Generic;
    
    public partial class Media
    {
        public Media()
        {
            this.Artist = "";
            this.Title = "";
            this.Album = "";
            this.DownloadName = "";
            this.DownloadUrl = "";
            this.BuyUrl = "";
            this.MediaRatings = new HashSet<MediaRating>();
        }
    
        public System.Guid MediaId { get; set; }
        public int MediaTypeId { get; set; }
        public System.DateTime EditedOn { get; set; }
        public Nullable<System.DateTime> LastSyncOn { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public Nullable<System.Guid> MediaCategoryId { get; set; }
        public string FileName { get; set; }
        public Nullable<double> Preference { get; set; }
        public bool IsCustomPreference { get; set; }
        public Nullable<short> Height { get; set; }
        public Nullable<short> Length { get; set; }
        public Nullable<short> StartPos { get; set; }
        public Nullable<short> EndPos { get; set; }
        public string DownloadName { get; set; }
        public string DownloadUrl { get; set; }
        public string BuyUrl { get; set; }
        public bool DisableSvp { get; set; }
        public bool DisableMadVr { get; set; }
        public bool DisablePitch { get; set; }
        public bool IsPersonal { get; set; }
    
        public virtual MediaCategory MediaCategory { get; set; }
        public virtual ICollection<MediaRating> MediaRatings { get; set; }
    }
}

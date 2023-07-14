using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HanumanInstitute.NaturalGroundingPlayer.Models
{
    public partial class Media
    {
        [Key]
        public Guid MediaId { get; set; }
        public MediaType MediaTypeId { get; set; }
        public DateTime EditedOn { get; set; }
        public DateTime? LastSyncOn { get; set; }
        [Required]
        public string Artist { get; set; } = string.Empty;
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Album { get; set; } = string.Empty;
        public string? FileName { get; set; }
        public float? Preference { get; set; }
        public bool IsCustomPreference { get; set; }
        public short? Height { get; set; }
        public short? Length { get; set; }
        public short? StartPos { get; set; }
        public short? EndPos { get; set; }
        public string DownloadName { get; set; } = string.Empty;
        [SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Reviewed: DB structure")]
        public string DownloadUrl { get; set; } = string.Empty;
        [SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "Reviewed: DB structure")]
        public string BuyUrl { get; set; } = string.Empty;
        public bool DisableSvp { get; set; }
        public bool DisablePitch { get; set; }
        public bool IsPersonal { get; set; }

        [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Reviewed: must be set by Entity Framework")]
        public ICollection<MediaRating> MediaRatings { get; set; } = null!;
    }
}

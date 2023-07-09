using System;
using System.ComponentModel.DataAnnotations;

namespace HanumanInstitute.NaturalGroundingPlayer.Models
{
    public partial class RatingCategory
    {
        public Guid RatingId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public bool Custom { get; set; }
    }
}

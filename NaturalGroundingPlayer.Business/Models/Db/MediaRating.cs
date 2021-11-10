using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HanumanInstitute.NaturalGroundingPlayer.Models
{
    public partial class MediaRating
    {
        [Key, Column(Order = 1)]
        public Guid MediaId { get; set; }
        [Key, Column(Order = 2)]
        public Guid RatingId { get; set; }
        public float? Height { get; set; }
        public float? Depth { get; set; }

        public RatingCategory RatingCategory { get; set; } = null!;
    }
}

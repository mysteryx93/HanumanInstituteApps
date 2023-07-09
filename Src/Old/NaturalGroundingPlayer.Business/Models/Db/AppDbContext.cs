using System;
using HanumanInstitute.CommonServices;
using Microsoft.EntityFrameworkCore;

namespace HanumanInstitute.NaturalGroundingPlayer.Models
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        { }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        public virtual DbSet<Media> Media => Set<Media>();
        public virtual DbSet<MediaRating> MediaRatings => Set<MediaRating>();
        public virtual DbSet<RatingCategory> RatingCategories => Set<RatingCategory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.CheckNotNull(nameof(modelBuilder));

            //modelBuilder.HasAnnotation("ProductVersion", "2.2.2-servicing-10034");

            modelBuilder.Entity<Media>(entity =>
            {
                entity.HasIndex(e => new[] { e.Artist, e.Title }).IsUnique().HasName("IX_Media_ArtistTitle");
                entity.HasIndex(e => e.MediaTypeId).HasName("IX_Media_MediaTypeId");
                entity.HasIndex(e => e.FileName).IsUnique().HasName("IX_Media_FileName");

                entity.Property(e => e.MediaId).ValueGeneratedOnAdd();
                entity.Property(e => e.Artist).HasColumnType("TEXT COLLATE NOCASE");
                entity.Property(e => e.Title).HasColumnType("TEXT COLLATE NOCASE");
                entity.Property(e => e.Album).HasColumnType("TEXT COLLATE NOCASE");
                entity.Property(e => e.FileName).HasColumnType("TEXT COLLATE NOCASE");
                entity.Property(e => e.DownloadName).HasColumnType("TEXT COLLATE NOCASE");
                entity.Property(e => e.DownloadUrl).HasColumnType("TEXT COLLATE NOCASE");
                entity.Property(e => e.BuyUrl).HasColumnType("TEXT COLLATE NOCASE");
            });

            modelBuilder.Entity<MediaRating>(entity =>
            {
                entity.HasKey(e => new[] { e.MediaId, e.RatingId });
                entity.HasIndex(e => e.MediaId);
                entity.HasIndex(e => e.RatingId);
            });

            modelBuilder.Entity<RatingCategory>(entity =>
            {
                entity.Property(e => e.Name).HasColumnType("TEXT COLLATE NOCASE");
            });
        }
    }
}

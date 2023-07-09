using System;
using System.Collections.Generic;
using System.Linq;
using HanumanInstitute.CommonServices;
using HanumanInstitute.NaturalGroundingPlayer.Models;
using Microsoft.EntityFrameworkCore;

namespace HanumanInstitute.NaturalGroundingPlayer.Services
{
    public class MediaRepository : IMediaRepository
    {
        private readonly AppDbContext _context;

        public MediaRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a Media object from its ID.
        /// </summary>
        /// <param name="videoId">The ID of the media to return.</param>
        /// <returns>A Media object.</returns>
        public Media GetMediaById(Guid videoId)
        {
            var result = (from v in _context.Media
                          where v.MediaId == videoId
                          select v).Include(v => v.MediaRatings.Select(r => r.RatingCategory)).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Returns a Media object from its file name.
        /// </summary>
        /// <param name="fileName">The file name of the media to return.</param>
        /// <returns>A Media object.</returns>
        public Media GetMediaByFileName(string fileName)
        {
            var result = (from v in _context.Media
                          where v.FileName == fileName
                          select v).Include(v => v.MediaRatings.Select(r => r.RatingCategory)).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Returns a Media object from its title.
        /// </summary>
        /// <param name="mediaType">The type of media to return.</param>
        /// <param name="artist">The media's artist.</param>
        /// <param name="title">The media's title.</param>
        /// <returns>The matching Media object, if found.</returns>
        public Media GetMediaByTitle(MediaType mediaType, string artist, string title)
        {
            var result = (from v in _context.Media
                          where v.MediaTypeId == mediaType && v.Artist == artist && v.Title == title
                          select v).Include(v => v.MediaRatings.Select(r => r.RatingCategory)).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Returns the list of custom rating categories.
        /// </summary>
        public List<RatingCategory> GetCustomRatingCategories()
        {
            var result = (from c in _context.RatingCategories
                          where c.Custom == true
                          orderby c.Name
                          select c).ToList();
            result.Insert(0, new RatingCategory());
            return result!;
        }

        /// <summary>
        /// Returns a new Media object and adds it to the data _context so it can later be saved.
        /// </summary>
        public Media NewMedia()
        {
            var result = new Media
            {
                MediaId = Guid.NewGuid()
            };
            _context.Media.Add(result);
            return result;
        }

        /// <summary>
        /// Adds specified Media to the data _context and generates a new ID to avoid conflicting with existing items.
        /// </summary>
        /// <param name="media">The media to add to the data _context..</param>
        public void AddMedia(Media media)
        {
            media.CheckNotNull(nameof(media));

            media.MediaId = Guid.NewGuid();
            _context.Media.Add(media);
        }

        /// <summary>
        /// Returns whether the Title and Artist of specified Media is already in the database.
        /// </summary>
        /// <param name="media">The media for which to look for an existing duplicate.</param>
        /// <returns>True if Artist and Title was found, otherwise false.</returns>
        public bool IsTitleDuplicate(Media media)
        {
            var isDuplicate = (from v in _context.Media
                               where v.MediaId != media.MediaId &&
                                   v.MediaTypeId == media.MediaTypeId &&
                                   (media.Artist == null || v.Artist == media.Artist) &&
                                   (media.Title == null || v.Title == media.Title)
                               select v.MediaId).Any();
            return isDuplicate;
        }

        /// <summary>
        /// Saves the changes back into the database.
        /// </summary>
        public void Save()
        {
            // Set IsCustomPreference field when updating Preference.
            foreach (var item in _context.ChangeTracker.Entries<Media>().Where(e => e.State == EntityState.Modified))
            {
                var oldValue = item.OriginalValues["Preference"] as double?;
                double? newValue = item.Entity.Preference;
                if (oldValue != newValue)
                {
                    item.Entity.IsCustomPreference = (newValue.HasValue);
                }
            }

            _context.SaveChanges();
        }

        /// <summary>
        /// Deletes specified media.
        /// </summary>
        /// <param name="media">The Media to delete.</param>
        public void Delete(Media media)
        {
            _context.Media.Remove(media);
            _context.SaveChanges();
        }

        /// <summary>
        /// Returns how many songs per artist are in the database.
        /// </summary>
        public IEnumerable<ArtistMediaCount> GetMediaCountPerArtist()
        {
            var result = (from m in _context.Media
                          where m.Artist.Length > 0
                          group m.MediaId by new { m.MediaTypeId, m.Artist } into a
                          select new ArtistMediaCount()
                          {
                              MediaType = (MediaType)a.Key.MediaTypeId,
                              Artist = a.Key.Artist,
                              Count = a.Count()
                          }).ToList();
            return result;
        }
    }
}

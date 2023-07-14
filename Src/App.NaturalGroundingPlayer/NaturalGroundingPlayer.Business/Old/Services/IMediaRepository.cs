using System;
using System.Collections.Generic;
using HanumanInstitute.NaturalGroundingPlayer.Models;

namespace HanumanInstitute.NaturalGroundingPlayer.Services
{
    public interface IMediaRepository
    {
        /// <summary>
        /// Returns a Media object from its ID.
        /// </summary>
        /// <param name="videoId">The ID of the media to return.</param>
        /// <returns>A Media object.</returns>
        Media GetMediaById(Guid videoId);

        /// <summary>
        /// Returns a Media object from its file name.
        /// </summary>
        /// <param name="fileName">The file name of the media to return.</param>
        /// <returns>A Media object.</returns>
        Media GetMediaByFileName(string fileName);

        /// <summary>
        /// Returns a Media object from its title.
        /// </summary>
        /// <param name="mediaType">The type of media to return.</param>
        /// <param name="artist">The media's artist.</param>
        /// <param name="title">The media's title.</param>
        /// <returns>The matching Media object, if found.</returns>
        Media GetMediaByTitle(MediaType mediaType, string artist, string title);

        /// <summary>
        /// Returns the list of custom rating categories.
        /// </summary>
        List<RatingCategory> GetCustomRatingCategories();

        /// <summary>
        /// Returns a new Media object and adds it to the data _context so it can later be saved.
        /// </summary>
        Media NewMedia();

        /// <summary>
        /// Adds specified Media to the data _context and generates a new ID to avoid conflicting with existing items.
        /// </summary>
        /// <param name="media">The media to add to the data _context..</param>
        void AddMedia(Media media);

        /// <summary>
        /// Returns whether the Title and Artist of specified Media is already in the database.
        /// </summary>
        /// <param name="media">The media for which to look for an existing duplicate.</param>
        /// <returns>True if Artist and Title was found, otherwise false.</returns>
        bool IsTitleDuplicate(Media media);

        /// <summary>
        /// Saves the changes back into the database.
        /// </summary>
        void Save();

        /// <summary>
        /// Deletes specified media.
        /// </summary>
        /// <param name="media">The Media to delete.</param>
        void Delete(Media media);

        /// <summary>
        /// Returns how many songs per artist are in the database.
        /// </summary>
        IEnumerable<ArtistMediaCount> GetMediaCountPerArtist();
    }
}

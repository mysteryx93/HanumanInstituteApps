using System;
using HanumanInstitute.NaturalGroundingPlayer.Models;

namespace HanumanInstitute.NaturalGroundingPlayer.Services
{
    /// <summary>
    /// Generates default storage paths for media files.
    /// </summary>
    public interface IDefaultMediaPathService
    {
        /// <summary>
        /// Returns the default file name for specified video.
        /// </summary>
        /// <param name="artist">The video's artist.</param>
        /// <param name="title">The video's title.</param>
        /// <param name="mediaType">The type of media.</param>
        /// <returns>A relative path and file excluding the extension.</returns>
        string GetDefaultFileName(string artist, string title, MediaType mediaType);
        /// <summary>
        /// Returns the default file name for specified video.
        /// </summary>
        /// <param name="artist">The video's artist.</param>
        /// <param name="title">The video's title.</param>
        /// <param name="mediaType">The type of media.</param>
        /// <param name="customFolder">If specified, files will by default be placed in specified folder.</param>
        /// <returns>A relative path and file excluding the extension.</returns>
        string GetDefaultFileName(string artist, string title, MediaType mediaType, string customFolder);
    }
}

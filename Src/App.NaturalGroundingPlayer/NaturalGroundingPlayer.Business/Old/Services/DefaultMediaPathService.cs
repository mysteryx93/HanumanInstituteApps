using System;
using System.Collections.Generic;
using System.Linq;
using HanumanInstitute.CommonServices;
using HanumanInstitute.NaturalGroundingPlayer.Models;

namespace HanumanInstitute.NaturalGroundingPlayer.Services
{
    /// <summary>
    /// Generates default storage paths for media files.
    /// </summary>
    public class DefaultMediaPathService : IDefaultMediaPathService
    {
        private readonly IFileSystemService _fileSystem;
        private readonly IMediaRepository _repository;

        public DefaultMediaPathService(IFileSystemService fileSystemService, IMediaRepository mediaRepository)
        {
            _fileSystem = fileSystemService;
            _repository = mediaRepository;
        }

        private IEnumerable<ArtistMediaCount>? _artistCount;

        /// <summary>
        /// Loads the data required to generate default paths.
        /// </summary>
        private void LoadData()
        {
            _artistCount = _repository.GetMediaCountPerArtist();
        }

        /// <summary>
        /// Returns the default file name for specified video.
        /// </summary>
        /// <param name="artist">The video's artist.</param>
        /// <param name="title">The video's title.</param>
        /// <param name="mediaType">The type of media.</param>
        /// <returns>A relative path and file excluding the extension.</returns>
        public string GetDefaultFileName(string artist, string title, MediaType mediaType)
        {
            return GetDefaultFileName(artist, title, mediaType);
        }

        /// <summary>
        /// Returns the default file name for specified video.
        /// </summary>
        /// <param name="artist">The video's artist.</param>
        /// <param name="title">The video's title.</param>
        /// <param name="mediaType">The type of media.</param>
        /// <param name="customFolder">If specified, files will by default be placed in specified folder.</param>
        /// <returns>A relative path and file excluding the extension.</returns>
        public string GetDefaultFileName(string artist, string title, MediaType mediaType, string customFolder)
        {
            if (customFolder == null && _artistCount == null)
            {
                LoadData();
            }

            var artistHasFolder = false;
            string? folder = null;

            if (customFolder == null)
            {
                // Artist has its own folder if there are at least 3 videos in the database for that artist
                if (artist.Length > 0)
                {
                    artistHasFolder = _artistCount.Where(a => a.MediaType == mediaType && string.Equals(a.Artist, artist, StringComparison.OrdinalIgnoreCase) && a.Count >= 3).Any();
                }

                // Folder is Other unless artist has 3 videos.
                if (artistHasFolder)
                {
                    folder = artist;
                }
            }
            else
            {
                // A custom folder was requested, such as Downloads
                folder = customFolder;
            }

            // Generate default path and file name.
            // string FolderName = Folder.Length > 0 ? folder : (artist.Length > 0 ? artist : "Other");
            string fileName;
            if (!artistHasFolder && artist.Length > 0)
            {
                fileName = string.Format("{0} - {1}", artist, title);
            }
            else
            {
                fileName = title;
            }

            if (customFolder == null)
            {
                if (mediaType == MediaType.Video)
                {
                    if (folder == null)
                    {
                        folder = "Others";
                    }
                }
                if (mediaType == MediaType.Audio)
                {
                    if (folder != null)
                    {
                        folder = "Audios\\" + folder;
                    }
                    else
                    {
                        folder = "Audios";
                    }
                }
                else if (mediaType == MediaType.Image)
                {
                    if (folder != null)
                    {
                        folder = "Images\\" + folder;
                    }
                    else
                    {
                        folder = "Images";
                    }
                }
            }

            // Remove illegal characters.
            foreach (var c in _fileSystem.Path.GetInvalidPathChars())
            {
                folder = folder?.Replace(c.ToString(), "");
            }
            foreach (var c in _fileSystem.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c.ToString(), "");
            }
            // Avoid a duplicate '.' before the extension
            fileName = fileName.TrimEnd('.');
            // Avoid a '.' at the end of a folder name.
            folder = folder?.TrimEnd('.');

            return $"{folder}\\{fileName}";
        }
    }
}

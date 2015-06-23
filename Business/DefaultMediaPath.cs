using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using System.IO;

namespace Business {
    public class DefaultMediaPath {
        private string customFolder = null;
        private static List<ArtistMediaCount> artistCount;
        private static List<MediaCategory> categories;

        public DefaultMediaPath():this(null) {
        }

        public DefaultMediaPath(string customFolder) {
            this.customFolder = customFolder;
            if (customFolder == null && artistCount == null)
                LoadData();
        }

        public void LoadData() {
            artistCount = DefaultMediaPathAccess.GetMediaCountPerArtist();
            if (categories == null)
                categories = DefaultMediaPathAccess.GetCategories();
        }

        /// <summary>
        /// Returns the default file name for specified video.
        /// </summary>
        /// <param name="artist">The video's artist.</param>
        /// <param name="title">The video's title.</param>
        /// <param name="folder">The folder where to place the file.</param>
        /// <param name="mediaType">The type of media.</param>
        /// <returns>A relative path and file excluding the extension.</returns>
        public string GetDefaultFileName(string artist, string title, Guid? category, MediaType mediaType) {
            bool ArtistHasFolder = false;
            string Folder = null;

            if (customFolder == null) {
                // Artist has its own folder if there are at least 5 videos in the database for that artist
                if (artist.Length > 0)
                    ArtistHasFolder = artistCount.Where(a => a.MediaType == mediaType && string.Equals(a.Artist, artist, StringComparison.OrdinalIgnoreCase) && a.Count >= 5).Any();

                // Folder is Category unless artist has 6 videos. If no category is specified, folder is Other.
                if (ArtistHasFolder)
                    Folder = artist;
                else if (category != null)
                    Folder = categories.FirstOrDefault(c => c.MediaCategoryId == category).Folder;
            } else {
                // A custom folder was requested, such as Downloads
                Folder = customFolder;
            }

            // Generate default path and file name.
            // string FolderName = Folder.Length > 0 ? folder : (artist.Length > 0 ? artist : "Other");
            string FileName;
            if (!ArtistHasFolder && artist.Length > 0)
                FileName = string.Format("{0} - {1}", artist, title);
            else
                FileName = title;

            if (customFolder == null) {
                if (mediaType == MediaType.Video) {
                    if (Folder == null)
                        Folder = "Others";
                } if (mediaType == MediaType.Audio) {
                    if (Folder != null)
                        Folder = "Audios\\" + Folder;
                    else
                        Folder = "Audios";
                } else if (mediaType == MediaType.Image) {
                    if (Folder != null)
                        Folder = "Images\\" + Folder;
                    else
                        Folder = "Images";
                }
            }

            // Remove illegal characters.
            foreach (char c in Path.GetInvalidPathChars()) {
                Folder = Folder.Replace(c.ToString(), "");
            }
            foreach (char c in Path.GetInvalidFileNameChars()) {
                FileName = FileName.Replace(c.ToString(), "");
            }
            // Avoid a duplicate '.' before the extension
            FileName = FileName.TrimEnd('.');
            // Avoid a '.' at the end of a folder name.
            Folder = Folder.TrimEnd('.');

            return String.Format("{0}\\{1}", Folder, FileName);
        }
    }
}

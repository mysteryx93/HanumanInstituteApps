using System;
using System.Collections.Generic;
using System.Linq;
using EmergenceGuardian.CommonServices;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    #region Interface

    /// <summary>
    /// Allows moving and reorganizing files to their default locations.
    /// </summary>
    public interface IMoveFilesBusiness {
        /// <summary>
        /// Loads the list of files stored the database.
        /// </summary>
        /// <returns>A list of MoveFileItem objects.</returns>
        List<MoveFileItem> LoadList();
        /// <summary>
        /// Reloads the media with specified ID from the list.
        /// </summary>
        /// <param name="mediaId">The ID of the media to reload.</param>
        /// <returns>The reloaded MoveFileItem.</returns>
        MoveFileItem Reload(Guid mediaId);
        /// <summary>
        /// Returns all files in the list that match specified search value.
        /// </summary>
        /// <param name="search">The value to search for.</param>
        /// <returns>A list of MoveFileItem objects.</returns>
        List<MoveFileItem> FilterList(string search);
        /// <summary>
        /// Moves specified video to specified location.
        /// </summary>
        /// <param name="item">The video to move.</param>
        /// <param name="destination">The destination to move the file to.</param>
        /// <returns>True if the move was successful, otherwise False.</returns>
        bool MoveFile(Media item, string destination);
        /// <summary>
        /// Moves the specified list of videos to their default locations.
        /// </summary>
        /// <param name="list">The list of files to move.</param>
        void MoveFiles(List<MoveFileItem> list);
        /// <summary>
        ///  Moves specified video to specified location.
        /// </summary>
        /// <param name="item">The information about the move.</param>
        /// <param name="save">Whether to save changes to the database.</param>
        /// <returns>True if the move was successful, otherwise False.</returns>
        bool MoveFile(MoveFileItem item, bool save);
    }

    #endregion

    /// <summary>
    /// Allows moving and reorganizing files to their default locations.
    /// </summary>
    public class MoveFilesBusiness : IMoveFilesBusiness {

        #region Declarations / Constructors

        protected readonly IFileSystemService fileSystem;
        private IDefaultMediaPath defaultPath;
        protected readonly ISettings settings;
        private IMediaAccess mediaAccess;

        public MoveFilesBusiness() : this(new FileSystemService(), new DefaultMediaPath(), new Settings(), new MediaAccess()) { }

        public MoveFilesBusiness(IFileSystemService fileSystemService, IDefaultMediaPath defaultPath, ISettings settings, IMediaAccess mediaAccess) {
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            this.defaultPath = defaultPath ?? throw new ArgumentNullException(nameof(defaultPath));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.mediaAccess = mediaAccess ?? throw new ArgumentNullException(nameof(mediaAccess));
        }

        #endregion

        private List<MoveFileItem> filesList;

        /// <summary>
        /// Loads the list of files stored the database.
        /// </summary>
        /// <returns>A list of MoveFileItem objects.</returns>
        public List<MoveFileItem> LoadList() {
            filesList = LoadListInternal(null);
            return filesList;
        }

        /// <summary>
        /// Reloads the media with specified ID from the list.
        /// </summary>
        /// <param name="mediaId">The ID of the media to reload.</param>
        /// <returns>The reloaded MoveFileItem.</returns>
        public MoveFileItem Reload(Guid mediaId) {
            filesList.RemoveAll(v => v.VideoId == mediaId);
            MoveFileItem NewItem = LoadListInternal(mediaId).FirstOrDefault();
            if (NewItem != null)
                filesList.Add(NewItem);
            return NewItem;
        }

        /// <summary>
        /// Loads the list of files stored in the database.
        /// </summary>
        /// <param name="videoId">If specified, reloads specified media, otherwise loads all data.</param>
        /// <returns>A list of MoveFileItem objects.</returns>
        private List<MoveFileItem> LoadListInternal(Guid? videoId) {
            Entities context = new Entities();
            var Result = (from v in context.Media
                          where v.FileName != null &&
                            (videoId == null || v.MediaId == videoId)
                          select new MoveFileItem() {
                              VideoId = v.MediaId,
                              MediaType = (MediaType)v.MediaTypeId,
                              Artist = v.Artist,
                              Title = v.Title,
                              MediaCategoryId = v.MediaCategoryId,
                              FileName = v.FileName
                          }).ToList();

            foreach (MoveFileItem item in Result) {
                item.NewFileName = defaultPath.GetDefaultFileName(item.Artist, item.Title, item.MediaCategoryId, item.MediaType) + fileSystem.Path.GetExtension(item.FileName);
                item.FileExists = fileSystem.File.Exists(settings.NaturalGroundingFolder + item.FileName);
            }

            Result.RemoveAll(v => v.NewFileName == v.FileName || !v.FileExists);
            return Result;
        }

        /// <summary>
        /// Returns all files in the list that match specified search value.
        /// </summary>
        /// <param name="search">The value to search for.</param>
        /// <returns>A list of MoveFileItem objects.</returns>
        public List<MoveFileItem> FilterList(string search) {
            return filesList.Where(v =>
                string.IsNullOrEmpty(search) ||
                v.FileName.IndexOf(search, StringComparison.OrdinalIgnoreCase) != -1 ||
                v.NewFileName.IndexOf(search, StringComparison.OrdinalIgnoreCase) != -1).OrderBy(v => v.NewFileName).ToList();
        }

        /// <summary>
        /// Moves specified video to specified location.
        /// </summary>
        /// <param name="item">The video to move.</param>
        /// <param name="destination">The destination to move the file to.</param>
        /// <returns>True if the move was successful, otherwise False.</returns>
        public bool MoveFile(Media item, string destination) {
            MoveFileItem FileInfo = new MoveFileItem() {
                VideoId = item.MediaId,
                FileName = item.FileName,
                NewFileName = destination
            };
            return MoveFile(FileInfo, false);
        }

        /// <summary>
        /// Moves the specified list of videos to their default locations.
        /// </summary>
        /// <param name="list">The list of files to move.</param>
        public void MoveFiles(List<MoveFileItem> list) {
            foreach (MoveFileItem item in list) {
                MoveFile(item, true);
            }
        }

        /// <summary>
        ///  Moves specified video to specified location.
        /// </summary>
        /// <param name="item">The information about the move.</param>
        /// <param name="save">Whether to save changes to the database.</param>
        /// <returns>True if the move was successful, otherwise False.</returns>
        public bool MoveFile(MoveFileItem item, bool save) {
            try {
                fileSystem.Directory.CreateDirectory(fileSystem.Path.GetDirectoryName(settings.NaturalGroundingFolder + item.NewFileName));
                fileSystem.File.Move(settings.NaturalGroundingFolder + item.FileName, settings.NaturalGroundingFolder + item.NewFileName);
                if (save) {
                    Media EditVideo = mediaAccess.GetMediaById(item.VideoId);
                    EditVideo.FileName = item.NewFileName;
                    mediaAccess.Save();
                }
                // Delete source folder if empty.
                string SourceDirectory = fileSystem.Path.GetDirectoryName(settings.NaturalGroundingFolder + item.FileName);
                if (!fileSystem.Directory.EnumerateFileSystemEntries(SourceDirectory).Any())
                    fileSystem.Directory.Delete(SourceDirectory);
                return true;
            }
            catch {
                return false;
            }
        }
    }
}

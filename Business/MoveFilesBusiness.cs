using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace Business {
    public class MoveFilesBusiness {
        private List<MoveFileItem> filesList;

        public MoveFilesBusiness() {
        }

        public List<MoveFileItem> LoadList() {
            filesList = LoadListInternal(null);
            return filesList;
        }

        public MoveFileItem Load(Guid videoId) {
            filesList.RemoveAll(v => v.VideoId == videoId);
            MoveFileItem NewItem = LoadListInternal(videoId).FirstOrDefault();
            if (NewItem != null)
                filesList.Add(NewItem);
            return NewItem;
        }

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

            DefaultMediaPath PathCalc = new DefaultMediaPath();
            PathCalc.LoadData();
            foreach (MoveFileItem item in Result) {
                item.NewFileName = PathCalc.GetDefaultFileName(item.Artist, item.Title, item.MediaCategoryId, item.MediaType) + Path.GetExtension(item.FileName);
                item.FileExists = File.Exists(Settings.NaturalGroundingFolder + item.FileName);
            }

            Result.RemoveAll(v => v.NewFileName == v.FileName || !v.FileExists);
            return Result;
        }

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
                Directory.CreateDirectory(Path.GetDirectoryName(Settings.NaturalGroundingFolder + item.NewFileName));
                File.Move(Settings.NaturalGroundingFolder + item.FileName, Settings.NaturalGroundingFolder + item.NewFileName);
                if (save) {
                    EditVideoBusiness Business = new EditVideoBusiness();
                    Media EditVideo = Business.GetVideoById(item.VideoId);
                    EditVideo.FileName = item.NewFileName;
                    Business.Save();
                }
                // Delete source folder if empty.
                string SourceDirectory = Path.GetDirectoryName(Settings.NaturalGroundingFolder + item.FileName);
                if (!Directory.EnumerateFileSystemEntries(SourceDirectory).Any())
                    Directory.Delete(SourceDirectory);
                return true;
            }
            catch {
                return false;
            }
        }
    }
}

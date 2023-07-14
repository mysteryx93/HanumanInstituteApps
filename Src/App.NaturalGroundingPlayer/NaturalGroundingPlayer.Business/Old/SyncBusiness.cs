using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using HanumanInstitute.CommonServices;
using HanumanInstitute.NaturalGroundingPlayer.DataAccess;

namespace HanumanInstitute.NaturalGroundingPlayer.Business {
    /// <summary>
    /// Provides features to export, import and synchronize data.
    /// </summary>
    public class SyncBusiness {

        #region Declarations / Constructors

        // Store imported data after generating a preview.
        private List<Media> importList;

        private ISyncAccess access;
        protected readonly ISerializationService serialization;

        public SyncBusiness() : this(new SyncAccess(), new SerializationService()) { }

        public SyncBusiness(ISyncAccess syncAccess, ISerializationService serializationService) {
            this.access = syncAccess ?? throw new ArgumentNullException(nameof(syncAccess));
            this.serialization = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
        }

        #endregion

        /// <summary>
        /// Exports Media items matching the specified list of IDs into specified file in XML format.
        /// </summary>
        /// <param name="exportList">The list of Media IDs to export.</param>
        /// <param name="outputFile">The file to export to.</param>
        public void ExportToFile(List<Guid> exportList, string outputFile) {
            List<SyncMedia> ExportData = (from v in access.GetMediaListById(exportList) select ConvertFromMedia(v)).ToList();
            string ExportXml = SerializeXml(ExportData);
            File.WriteAllText(outputFile, ExportXml);
        }

        /// <summary>
        /// Loads specified file and returns a preview of the data.
        /// </summary>
        /// <param name="inputFile">The file containing data to import.</param>
        public List<MediaListItem> ImportPreview(string inputFile) {
            importList = ParseFile(inputFile);
            return GeneratePreview(importList);
        }

        /// <summary>
        /// Imports the data previously loaded into the database.
        /// </summary>
        /// <param name="importList">The IDs of the elements to import.</param>
        /// <param name="progress">Reports the progress of the operation. First report is the amount of files to process, then subsequent reports represent the quantity done.</param>
        public async Task ImportToDatabase(List<Guid> selection, IProgress<int> progress) {
            if (importList == null)
                throw new InvalidOperationException("You must first call ImportPreview");
            await Task.Run(() => access.Import(selection, importList, progress));
        }

        /// <summary>
        /// Parses specified file and returns its raw data.
        /// </summary>
        /// <param name="inputFile">The file to parse.</param>
        /// <returns>A list of Media objects.</returns>
        private List<Media> ParseFile(string inputFile) {
            string ImportXml = File.ReadAllText(inputFile);
            List<SyncMedia> ImportList = DeserializeXml(ImportXml);
            List<MediaCategory> CacheCategories = access.GetMediaCategories();
            List<RatingCategory> CacheRatings = access.GetRatingCategories();
            List<Media> Result = (from i in ImportList select ConvertToMedia(i, CacheCategories, CacheRatings)).ToList();
            return Result;
        }

        /// <summary>
        /// Extracts preview data from the specified list of Media objects.
        /// </summary>
        /// <param name="list">The list of medias to extract preview from.</param>
        /// <returns>A list of MediaListItem objects.</returns>
        private List<MediaListItem> GeneratePreview(List<Media> list) {
            var Result = (from m in list
                          select new MediaListItem() {
                              MediaId = m.MediaId,
                              Artist = m.Artist,
                              Title = m.Title,
                              Length = m.Length,
                              FileExists = true,
                              IsInDatabase = true
                          }).ToList();
            return Result;
        }

        /// <summary>
        /// Serializes specified sync data into XML.
        /// </summary>
        /// <param name="exportList">The list of data to serialize.</param>
        /// <returns>A string containing XML data.</returns>
        private string SerializeXml(List<SyncMedia> data) {
            return serialization.Serialize<List<SyncMedia>>(data);
        }

        /// <summary>
        /// Deserializes specified XML sync data.
        /// </summary>
        /// <param name="data">The data to deserialize.</param>
        /// <returns>A list of SyncMedia objects.</returns>
        private List<SyncMedia> DeserializeXml(string data) {
            return serialization.Deserialize<List<SyncMedia>>(data);
        }

        /// <summary>
        /// Converts a Media object into a SyncMedia object.
        /// </summary>
        /// <param name="source">The Media object to convert.</param>
        /// <returns>A SyncMedia object.</returns>
        private SyncMedia ConvertFromMedia(Media source) {
            SyncMedia Result = new SyncMedia() {
                MediaId = source.MediaId,
                EditedOn = source.EditedOn,
                MediaType = (MediaType)source.MediaTypeId,
                Artist = source.Artist,
                Album = source.Album,
                Title = source.Title,
                Category = source?.MediaCategory?.Name,
                Preference = source.Preference,
                Length = source.Length,
                StartPos = source.StartPos,
                EndPos = source.EndPos,
                DownloadName = source.DownloadName,
                DownloadUrl = source.DownloadUrl,
                BuyUrl = source.BuyUrl,
                Ratings = source.MediaRatings.Select(r => new SyncRating(r.RatingCategory.Name, r.Height, r.Depth)).ToList()
            };
            return Result;
        }

        /// <summary>
        /// Converts a SyncMedia object into a Media object.
        /// </summary>
        /// <param name="syncMedia">The SyncMedia object to convert.</param>
        /// <param name="mediaCategories">A list of media categories.</param>
        /// <param name="ratingCategories">A list of rating categories.</param>
        /// <returns>A Media object.</returns>
        private Media ConvertToMedia(SyncMedia syncMedia, List<MediaCategory> mediaCategories, List<RatingCategory> ratingCategories) {
            Media Result = new Media() {
                MediaId = syncMedia.MediaId != Guid.Empty ? syncMedia.MediaId : Guid.NewGuid(),
                EditedOn = syncMedia.EditedOn ?? DateTime.Now,
                MediaType = syncMedia.MediaType,
                Artist = syncMedia.Artist ?? string.Empty,
                Album = syncMedia.Album ?? string.Empty,
                Title = syncMedia.Title,
                Preference = syncMedia.Preference,
                Length = syncMedia.Length,
                StartPos = syncMedia.StartPos,
                EndPos = syncMedia.EndPos,
                DownloadName = syncMedia.DownloadName ?? string.Empty,
                DownloadUrl = syncMedia.DownloadUrl ?? string.Empty,
                BuyUrl = syncMedia.BuyUrl ?? string.Empty,
            };
            if (!string.IsNullOrEmpty(syncMedia.Category)) {
                MediaCategory ItemCategory = mediaCategories.FirstOrDefault(v => v.Name == syncMedia.Category && v.MediaTypeId == (int)syncMedia.MediaType);
                if (ItemCategory != null)
                    Result.MediaCategoryId = ItemCategory.MediaCategoryId;
            }
            Result.MediaRatings = syncMedia.Ratings.Select(r => new MediaRating() {
                MediaId = Result.MediaId,
                RatingId = ratingCategories.FirstOrDefault(c => c.Name == r.Name).RatingId,
                Height = (r.Height != -1 ? (double?)r.Height : null),
                Depth = (r.Depth != -1 ? (double?)r.Depth : null)
            }).ToList();
            return Result;
        }
    }
}

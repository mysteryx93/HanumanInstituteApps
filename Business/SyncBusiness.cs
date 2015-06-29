using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using DataAccess;

namespace Business {
    /// <summary>
    /// Provides features to export, import and synchronize data.
    /// </summary>
    public class SyncBusiness {
        // Store imported data after generating a preview.
        private List<Media> importList;

        /// <summary>
        /// Initializes a new instance of the SyncBusiness class.
        /// </summary>
        public SyncBusiness() {
        }

        /// <summary>
        /// Exports Media items matching the specified list of IDs into specified file in XML format.
        /// </summary>
        /// <param name="exportList">The list of Media IDs to export.</param>
        /// <param name="outputFile">The file to export to.</param>
        public void ExportToFile(List<Guid> exportList, string outputFile) {
            List<SyncMedia> ExportData = (from v in SyncAccess.GetMediaListById(exportList) select new SyncMedia(v)).ToList();
            string ExportXml = SerializeXml(ExportData);
            File.WriteAllText(outputFile, ExportXml);
        }

        /// <summary>
        /// Loads specified file and returns a preview of the data.
        /// </summary>
        /// <param name="inputFile">The file containing data to import.</param>
        public List<VideoListItem> ImportPreview(string inputFile) {
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

            int i = 0;
            Media item;
            await Task.Run(() => {
                using (Entities context = new Entities()) {
                    DbContextTransaction Trans = context.Database.BeginTransaction();
                    foreach (Guid id in selection) {
                        item = importList.Single(v => v.MediaId == id);
                        item.IsPersonal = true;
                        SyncAccess.InsertOrMerge(item, context);
                        if (progress != null)
                            progress.Report(++i);
                    }
                    Trans.Commit();
                }
            });
        }

        /// <summary>
        /// Parses specified file and returns its raw data.
        /// </summary>
        /// <param name="inputFile">The file to parse.</param>
        /// <returns>A list of Media objects.</returns>
        private List<Media> ParseFile(string inputFile) {
            string ImportXml = File.ReadAllText(inputFile);
            List<SyncMedia> ImportList = DeserializeXml(ImportXml);
            List<MediaCategory> CacheCategories = SyncAccess.GetMediaCategories();
            List<RatingCategory> CacheRatings = SyncAccess.GetRatingCategories();
            List<Media> Result = (from i in ImportList select i.ConvertToMedia(CacheCategories, CacheRatings)).ToList();
            return Result;
        }

        /// <summary>
        /// Extracts preview data from the specified list of Media objects.
        /// </summary>
        /// <param name="list">The list of medias to extract preview from.</param>
        /// <returns>A list of VideoListItem objects.</returns>
        private List<VideoListItem> GeneratePreview(List<Media> list) {
            var Result = (from m in list
                          select new VideoListItem() {
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
            return XmlHelper.Serialize<List<SyncMedia>>(data, "MediaList");
        }

        /// <summary>
        /// Deserializes specified XML sync data.
        /// </summary>
        /// <param name="data">The data to deserialize.</param>
        /// <returns>A list of SyncMedia objects.</returns>
        private List<SyncMedia> DeserializeXml(string data) {
            return XmlHelper.Deserialize<List<SyncMedia>>(data, "MediaList");
        }
    }
}

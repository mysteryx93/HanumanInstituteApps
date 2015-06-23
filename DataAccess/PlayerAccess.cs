using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public static class PlayerAccess {
        public static Media GetVideoById(Guid videoId) {
            using (Entities context = new Entities()) {
                Media Result = (from v in context.Media.Include("MediaRatings.RatingCategory")
                                where v.MediaId == videoId
                                select v).FirstOrDefault();
                return Result;
            }
        }

        public static Media GetVideoByFileName(string fileName) {
            using (Entities context = new Entities()) {
                Media Result = (from v in context.Media.Include("MediaRatings.RatingCategory")
                                where v.FileName == fileName
                                select v).FirstOrDefault();
                return Result;
            }
        }

        public static Media GetVideoByTitle(string artist, string title) {
            using (Entities context = new Entities()) {
                Media Result = (from v in context.Media.Include("MediaRatings.RatingCategory")
                                where v.Artist == artist && v.Title == title
                                select v).FirstOrDefault();
                return Result;
            }
        }

        /// <summary>
        /// Returns whether specified video matches specified conditions.
        /// </summary>
        /// <param name="video">The video to evaluate.</param>
        /// <param name="settings">An object containing various search settings.</param>
        /// <returns>True if video matches conditions, otherwise false.</returns>
        public static bool VideoMatchesConditions(Media video, SearchSettings settings) {
            using (Entities context = new Entities()) {
                Media Result = SelectVideo(context.Media.Where(v => v.MediaId == video.MediaId), settings);
                return (Result != null);
            }
        }

        /// <summary>
        /// Return a random video from the database matching specified conditions.
        /// </summary>
        /// <param name="playedVideos">The list of videos already played.</param>
        /// <param name="count">Returns the number of results matching specified conditions.</param>
        /// <param name="settings">An object containing various search settings.</param>
        /// <returns>The randomly selected video, or null if no matches are found.</returns>
        public static Media SelectVideo(SearchSettings settings) {
            using (Entities context = new Entities()) {
                // context.Database.Log = Console.Write;
                return SelectVideo(context.Media.Include("MediaRatings.RatingCategory"), settings);
            }
        }

        /// <summary>
        /// Return a random video from the database matching specified conditions.
        /// </summary>
        /// <param name="data">The list of videos in which to search; generally Entities.Media.</param>
        /// <param name="settings">An object containing various search settings.</param>
        /// <returns>The randomly selected video, or null if no matches are found.</returns>
        public static Media SelectVideo(IQueryable<Media> data, SearchSettings settings) {
            if (settings == null)
                settings = new SearchSettings();
            if (settings.ExcludeVideos == null)
                settings.ExcludeVideos = new List<Guid>();

            var Query = (from v in data
                         where (settings.AllowDownloading || v.FileName != null) &&
                         (v.FileName != null || v.DownloadUrl != "") &&
                         (v.Length == null || ((v.EndPos != null ? v.EndPos.Value : v.Length.Value) - (v.StartPos != null ? v.StartPos.Value : 0)) <= 12 * 60) && // Don't get videos longer than 12 minutes
                         !settings.ExcludeVideos.Contains(v.MediaId) // Don't repeat same videos
                         select v);

            // Apply search filters.
            Query = SearchVideoAccess.ApplySearchFilters(Query, settings, null);

            // Return random result
            int TotalFound = 0;
            Guid? ResultId = SelectRandomId(Query, out TotalFound);
            settings.TotalFound = TotalFound;
            if (TotalFound == 0 || ResultId == null)
                return null;
            else if (TotalFound == 1)
                return Query.FirstOrDefault();
            else 
                return data.Where(v => v.MediaId == ResultId).FirstOrDefault();
        }

        /// <summary>
        /// Selects a random video from query result while giving higher priority to videos with higher preference.
        /// </summary>
        /// <param name="query">The query returning the list of videos to chose from.</param>
        /// <returns>A random video ID.</returns>
        public static Guid? SelectRandomId(IQueryable<Media> query, out int totalFound) {
            Guid? Result = null;

            // Pull list of ID and Preference from database.
            var IdList = query.Select(v => new { ID = v.MediaId, Preference = v.Preference }).ToList();
            totalFound = IdList.Count();
            if (totalFound == 0)
                return null;
            else if (totalFound == 1)
                return IdList[0].ID;

            // Calculate preferences average and total.
            int PreferenceCount = IdList.Where(v => v.Preference.HasValue).Count();
            int NoPreferenceCount = IdList.Count() - PreferenceCount;
            int PreferenceSum = IdList.Where(v => v.Preference.HasValue).Sum(v => PreferenceToInt(v.Preference.Value));
            // Use value 10 for every item if it is not specified for any.
            int PreferenceAvg = (PreferenceCount > 0 ? PreferenceSum / PreferenceCount : 10);
            // Videos with no preference get the average value.
            int PreferenceTotal = PreferenceSum + NoPreferenceCount * PreferenceAvg;


            // Get a random number between zero and the sum of all the preferences
            Random rand = new Random();
            int number = rand.Next(0, PreferenceTotal);

            int rollingSumOfPreferences = 0;

            // Set default value in case a value doesn't get assigned by the loop.
            if (totalFound > 0)
                Result = IdList[0].ID;

            // Select an index from the video list, but weighted by preference
            foreach (var item in IdList) {
                // Add the current item's preference to the rolling sum
                if (item.Preference.HasValue)
                    rollingSumOfPreferences += PreferenceToInt(item.Preference.Value);
                else
                    rollingSumOfPreferences += PreferenceAvg;

                // If we've hit or passed the random number, select this item
                if (rollingSumOfPreferences >= number) {
                    Result = item.ID;
                    break;
                }
            }

            return Result;
        }

        /// <summary>
        /// Returns an integer representation of the preference field where its weight grows exponentially.
        /// </summary>
        /// <param name="preference">The preference between 0 and 10.</param>
        /// <returns>An integer representing its probability weight.</returns>
        private static int PreferenceToInt(double preference) {
            return (int)(Math.Pow(preference, 1.4) * 100);
        }

        //public static void TestRandomness() {
        //    List<VideoRatingCondition> Cond = new List<VideoRatingCondition>();
        //    Cond.Add(new VideoRatingCondition("Emotional", 7.9f, 8.05f, false));
        //    var ResultList = new List<TestResult>();
        //    Media Item;
        //    TestResult NewResult;
        //    DateTime StartTime = DateTime.Now;

        //    for (int i = 0; i < 1000; i++) {
        //        Item = SelectVideo(Cond, null);
        //        NewResult = ResultList.Where(r => r.MediaId == Item.MediaId).FirstOrDefault();
        //        if (NewResult != null)
        //            NewResult.Count++;
        //        else {
        //            NewResult = new TestResult() {
        //                MediaId = Item.MediaId,
        //                Title = Item.Title,
        //                Preference = Item.Preference,
        //                Count = 1
        //            };
        //            ResultList.Add(NewResult);
        //        }
        //    }

        //    DateTime EndTime = DateTime.Now;
        //    TimeSpan TotalTime = EndTime - StartTime;
        //    Debug.WriteLine("Randomness test: " + TotalTime.ToString());
        //    int TotalCount = ResultList.Sum(r => r.Count);

        //    foreach (TestResult item in ResultList.OrderBy(v => v.Title)) {
        //        Debug.WriteLine("{0} {1}     {2} {3} {4}",
        //            item.MediaId,
        //            item.Title,
        //            item.Preference,
        //            item.Count,
        //            ((double)item.Count / TotalCount).ToString("p"));
        //    }
        //}

        //public class TestResult {
        //    public Guid MediaId { get; set; }
        //    public string Title { get; set; }
        //    public double? Preference { get; set; }
        //    public int Count { get; set; }
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace EmergenceGuardian.NaturalGroundingPlayer.DataAccess {

    #region Interface

    /// <summary>
    /// Provides data access for PlayerBusiness.
    /// </summary>
    public interface IPlayerAccess {
        /// <summary>
        /// Returns a Media from its MediaId.
        /// </summary>
        /// <param name="mediaId">The ID of the media to return.</param>
        /// <returns>A Media object from the database.</returns>
        Media GetVideoById(Guid mediaId);
        /// <summary>
        /// Returns a Media from its FileName.
        /// </summary>
        /// <param name="fileName">The file name where the Media is stored.</param>
        /// <returns>A Media object form the database.</returns>
        Media GetVideoByFileName(string fileName);
        /// <summary>
        /// Returns a Media from its Artist and Title.
        /// </summary>
        /// <param name="artist">The Artist of the Media.</param>
        /// <param name="title">The Title of the Media.</param>
        /// <returns>A Media object from the database.</returns>
        Media GetVideoByTitle(string artist, string title);
        /// <summary>
        /// Return a random video from the database matching specified conditions.
        /// </summary>
        /// <param name="playedVideos">The list of videos already played.</param>
        /// <param name="settings">An object containing various search settings.</param>
        /// <param name="count">The quantity of results to return.</param>
        /// <param name="maintainCurrent">If specified, the currently selected video will be kept.</param>
        /// <returns>The randomly selected video, or null if no matches are found.</returns>
        List<Media> SelectVideo(SearchSettings settings, int count, Media maintainCurrent);
        /// <summary>
        /// Return a random video from the database matching specified conditions.
        /// </summary>
        /// <param name="data">The list of videos in which to search; generally Entities.Media.</param>
        /// <param name="settings">An object containing various search settings.</param>
        /// <param name="count">The quantity of results to return.</param>
        /// <param name="maintainCurrent">If specified, the currently selected video will be kept.</param>
        /// <returns>The randomly selected video, or null if no matches are found.</returns>
        List<Media> SelectVideo(IQueryable<Media> data, SearchSettings settings, int count, Media maintainCurrent);
        /// <summary>
        /// Selects random videos from query result while giving higher priority to videos with higher preference.
        /// </summary>
        /// <param name="query">The query returning the list of videos to chose from.</param>
        /// <param name="count">The amount of videos to select.</param>
        /// <param name="totalFound">The total amount of videos matching the query.</param>
        /// <returns>A list of random videos.</returns>
        List<Guid> SelectRandomId(IQueryable<Media> query, int count, out int totalFound);
    }

    #endregion

    /// <summary>
    /// Provides data access for PlayerBusiness.
    /// </summary>
    public class PlayerAccess : IPlayerAccess {

        #region Declarations / Constructors

        private INgpContextFactory contextFactory;
        ISearchMediaAccess searchVideo;

        public PlayerAccess() : this(new NgpContextFactory(), new SearchMediaAccess()) { }

        public PlayerAccess(INgpContextFactory contextFactory, ISearchMediaAccess searchVideoAccess) {
            this.contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            this.searchVideo = searchVideoAccess ?? throw new ArgumentNullException(nameof(searchVideoAccess));
        }

        #endregion

        /// <summary>
        /// Returns a Media from its MediaId.
        /// </summary>
        /// <param name="mediaId">The ID of the media to return.</param>
        /// <returns>A Media object from the database.</returns>
        public Media GetVideoById(Guid mediaId) {
            using (Entities context = contextFactory.Create()) {
                Media Result = (from v in context.Media.Include("MediaRatings.RatingCategory")
                                where v.MediaId == mediaId
                                select v).FirstOrDefault();
                return Result;
            }
        }

        /// <summary>
        /// Returns a Media from its FileName.
        /// </summary>
        /// <param name="fileName">The file name where the Media is stored.</param>
        /// <returns>A Media object form the database.</returns>
        public Media GetVideoByFileName(string fileName) {
            using (Entities context = contextFactory.Create()) {
                Media Result = (from v in context.Media.Include("MediaRatings.RatingCategory")
                                where v.FileName == fileName
                                select v).FirstOrDefault();
                return Result;
            }
        }

        /// <summary>
        /// Returns a Media from its Artist and Title.
        /// </summary>
        /// <param name="artist">The Artist of the Media.</param>
        /// <param name="title">The Title of the Media.</param>
        /// <returns>A Media object from the database.</returns>
        public Media GetVideoByTitle(string artist, string title) {
            using (Entities context = contextFactory.Create()) {
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
        //public bool VideoMatchesConditions(Media video, SearchSettings settings) {
        //    return false;
        //    //using (Entities context = contextFactory.Create()) {
        //    //    Media Result = SelectVideo(context.Media.Where(v => v.MediaId == video.MediaId), settings);
        //    //    return (Result != null);
        //    //}
        //}

        /// <summary>
        /// Return a random video from the database matching specified conditions.
        /// </summary>
        /// <param name="playedVideos">The list of videos already played.</param>
        /// <param name="settings">An object containing various search settings.</param>
        /// <param name="count">The quantity of results to return.</param>
        /// <param name="maintainCurrent">If specified, the currently selected video will be kept.</param>
        /// <returns>The randomly selected video, or null if no matches are found.</returns>
        public List<Media> SelectVideo(SearchSettings settings, int count, Media maintainCurrent) {
            using (Entities context = contextFactory.Create()) {
                // context.Database.Log = Console.Write;
                return SelectVideo(context.Media.Include("MediaRatings.RatingCategory"), settings, count, maintainCurrent);
            }
        }

        /// <summary>
        /// Return a random video from the database matching specified conditions.
        /// </summary>
        /// <param name="data">The list of videos in which to search; generally Entities.Media.</param>
        /// <param name="settings">An object containing various search settings.</param>
        /// <param name="count">The quantity of results to return.</param>
        /// <param name="maintainCurrent">If specified, the currently selected video will be kept.</param>
        /// <returns>The randomly selected video, or null if no matches are found.</returns>
        public List<Media> SelectVideo(IQueryable<Media> data, SearchSettings settings, int count, Media maintainCurrent) {
            if (settings == null)
                settings = new SearchSettings();
            if (settings.ExcludeVideos == null)
                settings.ExcludeVideos = new List<Guid>();

            // Exclude currently selected video so that it doesn't get randomly re-reselected.
            if (maintainCurrent != null) {
                settings.ExcludeVideos = new List<Guid>(settings.ExcludeVideos);
                settings.ExcludeVideos.Add(maintainCurrent.MediaId);
                count--;
            }

            var Query = (from v in data
                         where (settings.AllowDownloading || v.FileName != null) &&
                         (v.FileName != null || v.DownloadUrl != "") &&
                         (v.Length == null || ((v.EndPos != null ? v.EndPos.Value : v.Length.Value) - (v.StartPos != null ? v.StartPos.Value : 0)) <= 12 * 60) && // Don't get videos longer than 12 minutes
                         !settings.ExcludeVideos.Contains(v.MediaId) // Don't repeat same videos
                         select v);

            // Apply search filters.
            Query = searchVideo.ApplySearchFilters(Query, settings, null);

            // Return random result
            int TotalFound = 0;
            List<Guid> ResultId = SelectRandomId(Query, count, out TotalFound);
            List<Media> Result = new List<Media>();
            if (maintainCurrent != null)
                Result.Add(maintainCurrent);
            settings.TotalFound = TotalFound;
            if (TotalFound > 0 && ResultId.Count() > 0) {
                if (TotalFound <= count)
                    Result.AddRange(Query.Take(count).ToList());
                else
                    Result.AddRange(data.Where(v => ResultId.Contains(v.MediaId)).ToList());
            }
            return Result;
        }

        /// <summary>
        /// Selects random videos from query result while giving higher priority to videos with higher preference.
        /// </summary>
        /// <param name="query">The query returning the list of videos to chose from.</param>
        /// <param name="count">The amount of videos to select.</param>
        /// <param name="totalFound">The total amount of videos matching the query.</param>
        /// <returns>A list of random videos.</returns>
        public List<Guid> SelectRandomId(IQueryable<Media> query, int count, out int totalFound) {
            List<Guid> Result = new List<Guid>();

            // Pull list of ID and Preference from database.
            var IdList = query.Select(v => new { ID = v.MediaId, Preference = v.Preference }).ToList();
            totalFound = IdList.Count();
            if (totalFound == 0)
                return Result;
            else if (totalFound <= count)
                return IdList.Select(v => v.ID).ToList();

            // Calculate preferences average and total.
            int PreferenceCount = IdList.Where(v => v.Preference.HasValue).Count();
            int NoPreferenceCount = IdList.Count() - PreferenceCount;
            int PreferenceSum = IdList.Where(v => v.Preference.HasValue).Sum(v => PreferenceToInt(v.Preference.Value));
            // Use value 10 for every item if it is not specified for any.
            int PreferenceAvg = (PreferenceCount > 0 ? PreferenceSum / PreferenceCount : 10);
            // Videos with no preference get the average value.
            int PreferenceTotal = PreferenceSum + NoPreferenceCount * PreferenceAvg;


            // Get a random number between zero and the sum of all the preferences
            for (int i = 0; i < count; i++) {
                Random rand = new Random();
                int number = rand.Next(0, PreferenceTotal);
                int rollingSumOfPreferences = 0;

                // Select an index from the video list, but weighted by preference
                foreach (var item in IdList) {
                    // Add the current item's preference to the rolling sum
                    if (item.Preference.HasValue)
                        rollingSumOfPreferences += PreferenceToInt(item.Preference.Value);
                    else
                        rollingSumOfPreferences += PreferenceAvg;

                    // If we've hit or passed the random number, select this item
                    if (rollingSumOfPreferences >= number) {
                        if (!Result.Contains(item.ID))
                            Result.Add(item.ID);
                        else // if item is already selected, try again.
                            i--;
                        break;
                    }
                }
            }

            // Set default value in case a value doesn't get assigned by the loop.
            //if (totalFound > 0)
            //    Result = IdList.Select(v => v.ID).Take(count).ToList();

            return Result;
        }

        /// <summary>
        /// Returns an integer representation of the preference field where its weight grows exponentially.
        /// </summary>
        /// <param name="preference">The preference between 0 and 10.</param>
        /// <returns>An integer representing its probability weight.</returns>
        private int PreferenceToInt(double preference) {
            return (int)(Math.Pow(preference, 1.2) * 100);
            // return (int)(preference * 100);
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

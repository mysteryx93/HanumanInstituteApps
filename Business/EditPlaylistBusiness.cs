using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using DataAccess;
using EmergenceGuardian.FFmpeg;

namespace Business {
    public class EditPlaylistBusiness {
        private List<VideoListItem> playlist;
        private List<LocalFileInfo> files;
        private static bool isLoadingMediaInfo = false;
        private static IProgress<int> loadingMediaInfoProgress;
        private static int loadingMediaInfoCount;

        private static List<string> PremiumProductDomains = new List<string> { "mindreel.tv", "powerliminals.com", "clickbank.net", "1shoppingcart.com" };
        private static List<string> VideoDomains = new List<string> { "www.ethaicd.com" };
        private static List<string> MusicDomains = new List<string> { "www.itunes.com" };

        public EditPlaylistBusiness() {
        }

        /// <summary>
        /// Loads the list of artists.
        /// </summary>
        /// <param name="settings">The filters to apply to the data.</param>
        public async Task<List<SearchCategoryItem>> LoadCategoriesAsync(SearchSettings settings, SearchFilterEnum groupType) {
            List<SearchCategoryItem> Result = new List<SearchCategoryItem>();
            if (settings.ListIsInDatabase != false) {
                Result.Add(new SearchCategoryItem(SearchFilterEnum.All, null, "All"));
                if (groupType != SearchFilterEnum.Artist)
                    Result.Add(new SearchCategoryItem(SearchFilterEnum.Artist, null, "By Artist"));
                if (groupType != SearchFilterEnum.Category)
                    Result.Add(new SearchCategoryItem(SearchFilterEnum.Category, null, "By Category"));
                if (groupType != SearchFilterEnum.Element)
                    Result.Add(new SearchCategoryItem(SearchFilterEnum.Element, null, "By Element"));
            }
            if (settings.ListIsInDatabase != true)
                Result.Add(new SearchCategoryItem(SearchFilterEnum.Files, null, "Other Files"));
            Result.Add(new SearchCategoryItem(SearchFilterEnum.None, null, "---"));

            if (groupType == SearchFilterEnum.Artist) {
                if (string.IsNullOrEmpty(settings.Search)) {
                    Result.Add(new SearchCategoryItem(SearchFilterEnum.Artist, "", "No Artist"));
                    Result.Add(new SearchCategoryItem(SearchFilterEnum.ArtistSingles, null, "Singles"));
                }
                Result.AddRange(await Task.Run(() => SearchVideoAccess.GetCategoryArtists(settings)));
            } else if (groupType == SearchFilterEnum.Category) {
                if (string.IsNullOrEmpty(settings.Search))
                    Result.Add(new SearchCategoryItem(SearchFilterEnum.Category, "", "No Category"));
                Result.AddRange(await Task.Run(() => SearchVideoAccess.GetCategoryCategories(settings)));
            } else if (groupType == SearchFilterEnum.Element)
                Result.AddRange(await Task.Run(() => SearchVideoAccess.GetCategoryElements(settings)));

            return Result;
        }

        /// <summary>
        /// Loads the playlist from the database with specified filter conditions.
        /// </summary>
        /// <param name="settings">The filters to apply to the data.</param>
        /// <returns>A Task object for asynchronous processing.</returns>
        public async Task LoadPlaylistAsync(SearchSettings settings) {
            await LoadPlaylistAsync(settings, false);
        }

        /// <summary>
        /// Loads the playlist from the database with specified filter conditions.
        /// </summary>
        /// <param name="settings">The filters to apply to the data.</param>
        /// <param name="forceScanFolder">Whether to scan files in folder to set FileExists.</param>
        /// <returns>A Task object for asynchronous processing.</returns>
        public async Task LoadPlaylistAsync(SearchSettings settings, bool forceScanFolder) {
            bool LoadFilesRequired = !(settings.IsInDatabase == true);

            // Load database.
            var DbTask = Task.Run(() => SearchVideoAccess.GetList(settings));

            // Load files.
            if (LoadFilesRequired || forceScanFolder) {
                if (files == null) {
                    // Run both tasks simultaneously.
                    var FilesTask = Task.Run(() => LoadFiles());
                    Task.WaitAll(DbTask, FilesTask);
                }
                playlist = await DbTask;
                await Task.Run(() => MergePlaylist(settings));
            } else
                playlist = await DbTask;
        }

        /// <summary>
        /// Loads all orphan files in the Natural Grounding folder as a list of strings.
        /// </summary>
        public void LoadFiles() {
            files = new List<LocalFileInfo>();

            if (!Directory.Exists(Settings.NaturalGroundingFolder))
                return;

            // We must load all file names from the database to know which files are not in the database.
            List<string> AllFiles = SearchVideoAccess.GetAllFileNames();

            string ItemFile;
            bool ItemInDatabase;
            var FileEnum = Directory.EnumerateFiles(Settings.NaturalGroundingFolder, "*.*", System.IO.SearchOption.AllDirectories);
            foreach (string f in FileEnum) {
                // Exclude temp folder.
                if (!f.StartsWith(Settings.TempFilesPath)) {
                    ItemFile = f.Substring(Settings.NaturalGroundingFolder.Length);
                    ItemInDatabase = AllFiles.Any(d => d.Equals(ItemFile, StringComparison.OrdinalIgnoreCase));
                    files.Add(new LocalFileInfo(ItemFile, ItemInDatabase));
                }
            }
        }

        /// <summary>
        /// Merges the list of files in the folder with the data coming from the database.
        /// </summary>
        /// <param name="settings">The filters to apply to the data.</param>
        private void MergePlaylist(SearchSettings settings) {
            VideoListItem ListItem;
            MediaType ItemType;
            foreach (LocalFileInfo item in files) {
                ItemType = EditVideoBusiness.GetFileType(item.FileName);
                ListItem = playlist.Where(v => string.Equals(v.FileName, item.FileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (ListItem != null)
                    ListItem.FileExists = true;
                else {
                    // Add file if it matches search settings.
                    if (!item.IsInDatabase && OrphanMatchesConditions(ItemType, item.FileName, settings)) {
                        // Add file to ListView.
                        playlist.Add(new VideoListItem() {
                            MediaId = null,
                            MediaType = ItemType,
                            Title = item.FileName,
                            FileName = item.FileName,
                            FileExists = true,
                            IsInDatabase = false
                        });
                    }
                }
            }

            // Apply FileExists search filter.
            SearchConditionSetting Cond = settings.ConditionFilters.FirstOrDefault(f => f.Field == FieldConditionEnum.FileExists && f.Value != BoolConditionEnum.None);
            if (Cond != null)
                playlist.RemoveAll(v => v.FileExists == (Cond.Value == BoolConditionEnum.No));
            // Apply IsInDatabase search filter.
            if (settings.IsInDatabase == false)
                playlist.RemoveAll(v => v.IsInDatabase);
        }

        private bool OrphanMatchesConditions(MediaType mediaType, string fileName, SearchSettings settings) {
            bool Result = true;
            Result = (settings.MediaType == MediaType.None || mediaType == settings.MediaType) &&
                        (string.IsNullOrEmpty(settings.Search) || (fileName != null && fileName.IndexOf(settings.Search, StringComparison.OrdinalIgnoreCase) != -1)) &&
                        (string.IsNullOrEmpty(settings.RatingCategory) || !settings.RatingValue.HasValue) &&
                        (settings.IsInDatabase != true) &&
                        (settings.HasRating == HasRatingEnum.All || settings.HasRating == HasRatingEnum.Without);
            foreach (SearchConditionSetting item in settings.ConditionFilters) {
                if (!(item.Field == FieldConditionEnum.None || item.Value == BoolConditionEnum.None ||
                    // (item.Field == FieldConditionEnum.IsInDatabase && item.Value == BoolConditionEnum.No) ||
                    (item.Field == FieldConditionEnum.FileExists && item.Value == BoolConditionEnum.Yes) ||
                    (item.Field == FieldConditionEnum.HasDownloadUrl && item.Value == BoolConditionEnum.No)))
                    Result = false;
            }
            return Result;
        }

        /// <summary>
        /// Loads the list of rating categories for the search interface.
        /// </summary>
        /// <returns>A list of RatingCategory objects.</returns>
        public async Task<List<RatingCategory>> GetRatingCategoriesAsync(bool addSpecial) {
            List<RatingCategory> Result = await Task.Run(() => SearchVideoAccess.GetCustomRatingCategories());
            Result.Insert(0, new RatingCategory() { Name = "--------------------" });
            Result.Insert(0, new RatingCategory() { Name = "Love" });
            Result.Insert(0, new RatingCategory() { Name = "Egoless" });
            Result.Insert(0, new RatingCategory() { Name = "Spiritual Feminine" });
            Result.Insert(0, new RatingCategory() { Name = "Spiritual Masculine" });
            Result.Insert(0, new RatingCategory() { Name = "Emotional Feminine" });
            Result.Insert(0, new RatingCategory() { Name = "Emotional Masculine" });
            Result.Insert(0, new RatingCategory() { Name = "Physical Feminine" });
            Result.Insert(0, new RatingCategory() { Name = "Physical Masculine" });
            Result.Insert(0, new RatingCategory() { Name = "Intensity" });
            if (addSpecial) {
                Result.Insert(0, new RatingCategory() { Name = "--------------------" });
                Result.Insert(0, new RatingCategory() { Name = "Height" });
                Result.Insert(0, new RatingCategory() { Name = "Length" });
                Result.Insert(0, new RatingCategory() { Name = "Highest" });
                Result.Insert(0, new RatingCategory() { Name = "Preference" });
            }
            Result.Insert(0, new RatingCategory());
            return Result;
        }

        /// <summary>
        /// Loops through the Media table to set the FileName, Length, Width and Height fields.
        /// </summary>
        /// <param name="progress">Reports the progress of the operation. First report is the amount of files to process, then subsequent reports represent the quantity done.</param>
        /// <returns>Whether some data was modified.</returns>
        public async Task<bool> LoadMediaInfoAsync(IProgress<int> progress) {
            // Calling this method when it is already running allows listening to the progress.
            if (progress != null) {
                loadingMediaInfoProgress = progress;
                // First progress report is total count.
                if (isLoadingMediaInfo)
                    loadingMediaInfoProgress.Report(loadingMediaInfoCount);
            }

            if (isLoadingMediaInfo)
                return false;
            isLoadingMediaInfo = true;

            bool HasChanges = false;
            using (Entities context = new Entities()) {
                // Loop through all media items with missing Length, Width or Height.
                var Query = (from v in context.Media
                             where v.FileName == null || v.Length == null ||
                                ((v.MediaTypeId == (int)MediaType.Video || v.MediaTypeId == (int)MediaType.Image) && v.Height == null)
                             select v);

                // First progress report contains the total count. Subsequent reports contain the quantity completed.
                loadingMediaInfoCount = Query.Count();
                if (loadingMediaInfoProgress != null)
                    loadingMediaInfoProgress.Report(loadingMediaInfoCount);

                int ItemsCompleted = 0;
                string DefaultFileName;
                DefaultMediaPath PathCalc = new DefaultMediaPath();
                PathCalc.LoadData();

                foreach (Media item in Query) {
                    // Try to auto-attach file if default file name exists.
                    if (item.FileName == null) {
                        DefaultFileName = PathCalc.GetDefaultFileName(item.Artist, item.Title, item.MediaCategoryId, item.MediaType);
                        if (AutoAttachFile(item, DefaultFileName))
                            HasChanges = true;
                        await Task.Delay(1);
                    }

                    // Load media file to set Length, Width and Height.
                    if (item.FileName != null && FileEntryHasMissingInfo(item)) {
                        await LoadFileEntryInfoAsync(item);
                        HasChanges = true;
                    }

                    // Send update with the quantity of files completed.
                    if (loadingMediaInfoProgress != null)
                        loadingMediaInfoProgress.Report(++ItemsCompleted);
                }
                if (HasChanges)
                    context.SaveChanges();
            }
            isLoadingMediaInfo = false;
            loadingMediaInfoCount = 0;
            loadingMediaInfoProgress = null;
            return HasChanges;
        }

        public static bool AutoAttachFile(Media item, string fileName) {
            string FilePath;
            foreach (string ext in AppPaths.GetMediaTypeExtensions(item.MediaType)) {
                FilePath = Settings.NaturalGroundingFolder + fileName + ext;
                if (File.Exists(FilePath)) {
                    item.FileName = fileName + ext;
                    return true;
                }
            }
            return false;
        }

        public static bool FileEntryHasMissingInfo(Media item) {
            return (item.FileName != null &&
                File.Exists(Settings.NaturalGroundingFolder + item.FileName)
                && (item.Length == null || (FileEntryHasDimensions(item) && item.Height == null)));
        }

        public static bool FileEntryHasDimensions(Media item) {
            return item.MediaTypeId == (int)MediaType.Video || item.MediaTypeId == (int)MediaType.Image;
        }

        /// <summary>
        /// Loads the Length, Width and Height of specified media file.
        /// </summary>
        /// <param name="item">The media file to read.</param>
        /// <returns>True if data was loaded, false if no data was needed.</returns>
        public async Task LoadFileEntryInfoAsync(Media item) {
            FFmpegProcess FileInfo = await Task.Run(() => MediaInfo.GetFileInfo(Settings.NaturalGroundingFolder + item.FileName));
            if (FileInfo.FileDuration > TimeSpan.Zero)
                item.Length = (short)FileInfo.FileDuration.TotalSeconds;
            if (FileInfo.VideoStream != null && FileInfo.VideoStream.Height > 0)
                item.Height = (short)FileInfo.VideoStream.Height;
        }

        public async Task LoadBuyListAsync(SearchSettings settings, BuyProductType productType, bool showAllFiles) {
            settings.MediaType = MediaType.None;
            if (showAllFiles) {
                settings.IsInDatabase = true;
                settings.ConditionField = FieldConditionEnum.None;
            } else {
                settings.SetCondition(FieldConditionEnum.FileExists, false);
            }
            settings.BuyUrlDomainsNegated = false;
            if (productType == BuyProductType.PremiumProduct)
                settings.BuyUrlDomains = PremiumProductDomains;
            else if (productType == BuyProductType.Videos)
                settings.BuyUrlDomains = VideoDomains;
            else if (productType == BuyProductType.Music)
                settings.BuyUrlDomains = MusicDomains;
            else if (productType == BuyProductType.Other) {
                List<string> AllDomains = new List<string>();
                AllDomains.AddRange(PremiumProductDomains);
                AllDomains.AddRange(VideoDomains);
                AllDomains.AddRange(MusicDomains);
                settings.BuyUrlDomains = AllDomains;
                settings.BuyUrlDomainsNegated = true;
            }

            await LoadPlaylistAsync(settings, true);
        }

        public void RefreshPlaylist(Media video, string oldFileName) {
            if (video.FileName != null) {
                RefreshFilesCache(video.FileName, true);
                if (oldFileName != null && oldFileName != video.FileName)
                    RefreshFilesCache(oldFileName, false);
            } else if (oldFileName != null)
                RefreshFilesCache(oldFileName, false);
        }

        private void RefreshFilesCache(string fileName, bool isInDatabase) {
            bool FileExists = File.Exists(Settings.NaturalGroundingFolder + fileName);
            LocalFileInfo FileEntry = files.Where(f => f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (FileEntry != null)
                FileEntry.IsInDatabase = isInDatabase;
            if (FileExists && FileEntry == null)
                files.Add(new LocalFileInfo(fileName, isInDatabase));
            else if (!FileExists && FileEntry != null)
                files.Remove(FileEntry);
        }

        public List<VideoListItem> Playlist {
            get { return playlist; }
            set { playlist = value; }
        }

        public List<VideoListItem> GetSortedPlaylist(string search, string orderBy, ListSortDirection orderDirection) {
            // return SearchVideoAccess.FilterAndSort(playlist, search, orderBy, orderDirection);
            return null;
        }

        public bool UpdateFileName(Guid videoId, string fileName) {
            VideoListItem Item = playlist.Find(v => v.MediaId == videoId);
            if (Item == null)
                Item = new VideoListItem() { MediaId = videoId };
            return UpdateFileName(Item, fileName);
        }

        public bool UpdateFileName(VideoListItem item, string fileName) {
            EditVideoBusiness Business = new EditVideoBusiness();
            if (Business.GetVideoByFileName(fileName) == null) {
                // Update database.
                Media DbItem = Business.GetVideoById(item.MediaId.Value);
                DbItem.FileName = fileName;
                Business.Save();
                // Update in-memory list.
                if (!item.FileExists) {
                    item.FileExists = true;
                    item.FileName = fileName;
                    playlist.RemoveAll(v => v.FileName == fileName && v.IsInDatabase == false);
                } else if (!item.IsInDatabase) {
                    item.IsInDatabase = true;
                    item.FileName = fileName;
                    playlist.RemoveAll(v => v.FileName == fileName && v.FileExists == false);
                }
                return true;
            } else
                return false;
        }

        /// <summary>
        /// Returns the 3 first letters of a rating category or the first letter of each word.
        /// </summary>
        /// <returns></returns>
        public string GetRatingInitials(string text) {
            if (text.StartsWith("--"))
                return "";
            string[] Words = text.Replace('-', ' ').Split(' ');
            if (Words.Length == 0)
                return "";
            else if (Words.Length == 1 && Words[0].Length > 4)
                return text.Substring(0, 3);
            else if (Words.Length == 1)
                return Words[0];
            else {
                // Several words, return the first letter of each word.
                return new string(Words.Select(w => w[0]).ToArray()).ToUpper();
            }
        }

        /// <summary>
        /// Automatically binds files in the playlist and load missing info.
        /// </summary>
        public static async Task AutoBindFilesAsync() {
            // Auto-bind files
            EditPlaylistBusiness BindBusiness = new EditPlaylistBusiness();
            SearchSettings BindSettings = new SearchSettings();
            BindSettings.IsInDatabase = true;
            await BindBusiness.LoadPlaylistAsync(BindSettings);
            await BindBusiness.LoadMediaInfoAsync(null);
        }
    }
}

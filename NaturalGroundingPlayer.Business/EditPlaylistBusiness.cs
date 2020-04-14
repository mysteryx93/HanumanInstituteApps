using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using EmergenceGuardian.CommonServices;
using EmergenceGuardian.DownloadManager;
using EmergenceGuardian.Encoder;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    #region Interface

    /// <summary>
    /// Manages a list of medias for display.
    /// </summary>
    public interface IEditPlaylistBusiness {
        /// <summary>
        /// Returns a list of groups: artists, categories or element.
        /// </summary>
        /// <param name="settings">The filters to apply to the data.</param>
        /// <param name="groupType">The type of group categories to return.</param>
        /// <returns>A list of SearchCategoryItem objects.</returns>
        Task<List<SearchCategoryItem>> LoadGroupsAsync(SearchSettings settings, SearchFilterEnum groupType);
        /// <summary>
        /// Loads the Playlist from the database with specified filter conditions.
        /// </summary>
        /// <param name="settings">The filters to apply to the data.</param>
        Task LoadPlaylistAsync(SearchSettings settings);
        /// <summary>
        /// Loads the Playlist from the database with specified filter conditions.
        /// </summary>
        /// <param name="settings">The filters to apply to the data.</param>
        /// <param name="forceScanFolder">Whether to scan files in folder to set FileExists.</param>
        Task LoadPlaylistAsync(SearchSettings settings, bool forceScanFolder);
        /// <summary>
        /// Loads all orphan files in the Natural Grounding folder as a list of strings.
        /// </summary>
        void LoadFiles();
        /// <summary>
        /// Loads the list of rating categories for the search interface.
        /// </summary>
        /// <param name="addSpecial">Whether to add special categories to the list: heigth, length, highest and preference.</param>
        /// <returns>A list of RatingCategory objects.</returns>
        Task<List<RatingCategory>> GetRatingCategoriesAsync(bool addSpecial);
        /// <summary>
        /// Loops through the Media table to set the FileName, Length, Width and Height fields.
        /// </summary>
        /// <param name="progress">Reports the progress of the operation. First report is the amount of files to process, then subsequent reports represent the quantity done.</param>
        /// <returns>Whether some data was modified.</returns>
        Task<bool> LoadMediaInfoAsync(IProgress<int> progress);
        /// <summary>
        /// Loads the list of resources that can be purchased.
        /// </summary>
        /// <param name="settings">The search settings.</param>
        /// <param name="productType">The type of products to buy.</param>
        /// <param name="showAllFiles">Whether to show entries that already have a local file.</param>
        Task LoadBuyListAsync(SearchSettings settings, BuyProductType productType, bool showAllFiles);
        /// <summary>
        /// Returns what type of media a file is based on its extension.
        /// </summary>
        /// <param name="fileName">The path of the file to get the type of.</param>
        /// <returns></returns>
        MediaType GetFileType(string fileName);
        /// <summary>
        /// Attempts to automatically attach a file to a database entry if the specified file name (with any extension) exists.
        /// </summary>
        /// <param name="item">The database Media item on which to attach the file.</param>
        /// <param name="fileName">The path where to look for a file, without extension. Every valid file extension will be tried.</param>
        /// <returns>Whether </returns>
        bool AutoAttachFile(Media item, string fileName);
        /// <summary>
        /// Automatically binds files in the Playlist and load missing info.
        /// </summary>
        Task AutoBindFilesAsync();
        /// <summary>
        /// Returns whether specified Media has missing metadata (Length and Height).
        /// </summary>
        /// <param name="item">The Media item to check.</param>
        /// <returns>Whether metadata is missing.</returns>
        bool FileEntryHasMissingInfo(Media item);
        /// <summary>
        /// Loads the Length, Width and Height of specified media file.
        /// </summary>
        /// <param name="item">The media file to read.</param>
        /// <returns>True if data was loaded, false if no data was needed.</returns>
        Task LoadFileEntryInfoAsync(Media item);
        /// <summary>
        /// Updates the list of files after making changes to a Media entry.
        /// </summary>
        /// <param name="media">The Media entry that was modified.</param>
        /// <param name="oldFileName">The Media's file name before the edit.</param>
        void RefreshPlaylist(Media media, string oldFileName);
        /// <summary>
        /// Updates the file name of specified Media.
        /// </summary>
        /// <param name="videoId">The ID of the Media.</param>
        /// <param name="fileName">The new file name.</param>
        /// <returns>Whether the operation was successful.</returns>
        bool UpdateFileName(Guid videoId, string fileName);
        /// <summary>
        /// Updates the file name of specified Media. It will fail if the file is already attached to another entry.
        /// </summary>
        /// <param name="item">The MediaListItem to update.</param>
        /// <param name="fileName">The new file name.</param>
        /// <returns>Whether the operation was successful.</returns>
        bool UpdateFileName(MediaListItem item, string fileName);
    }

    #endregion

    /// <summary>
    /// Manages a list of medias for display.
    /// </summary>
    public class EditPlaylistBusiness : IEditPlaylistBusiness {

        #region Declarations / Constructors

        /// <summary>
        /// Gets or sets the list of Media items loaded into the class.
        /// </summary>
        public List<MediaListItem> Playlist { get; set; }
        private List<LocalFileInfo> files;
        private static bool isLoadingMediaInfo = false;
        private static IProgress<int> loadingMediaInfoProgress;
        private static int loadingMediaInfoCount;

        private static List<string> PremiumProductDomains = new List<string> { "mindreel.tv", "powerliminals.com", "clickbank.net", "1shoppingcart.com" };
        private static List<string> VideoDomains = new List<string> { "www.ethaicd.com" };
        private static List<string> MusicDomains = new List<string> { "www.itunes.com" };

        private Dictionary<Guid, ScanResultItem> scanResults = new Dictionary<Guid, ScanResultItem>(10);
        private DownloadOptions downloadOptions = new DownloadOptions();

        protected readonly IFileSystemService fileSystem;
        protected readonly ISettings settings;
        protected readonly IAppPathService appPath;
        private IDefaultMediaPath defaultPath;
        private IMediaInfoReader mediaInfo;
        private ISearchMediaAccess searchAccess;
        private IMediaAccess mediaAccess;

        public EditPlaylistBusiness() { }

        public EditPlaylistBusiness(IFileSystemService fileSystem, ISettings settings, IAppPathService appPathService, IDefaultMediaPath defaultPath, IMediaInfoReader mediaInfo, ISearchMediaAccess searchAccess, IMediaAccess editMediaAccess) {
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.appPath = appPathService ?? throw new ArgumentNullException(nameof(appPathService));
            this.defaultPath = defaultPath ?? throw new ArgumentNullException(nameof(defaultPath));
            this.mediaInfo = mediaInfo ?? throw new ArgumentNullException(nameof(mediaInfo));
            this.searchAccess = searchAccess ?? throw new ArgumentNullException(nameof(searchAccess));
            this.mediaAccess = editMediaAccess ?? throw new ArgumentNullException(nameof(editMediaAccess));
        }

        #endregion

        #region Load Data

        /// <summary>
        /// Returns a list of groups: artists, categories or element.
        /// </summary>
        /// <param name="settings">The filters to apply to the data.</param>
        /// <param name="groupType">The type of group categories to return.</param>
        /// <returns>A list of SearchCategoryItem objects.</returns>
        public async Task<List<SearchCategoryItem>> LoadGroupsAsync(SearchSettings settings, SearchFilterEnum groupType) {
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
                Result.AddRange(await Task.Run(() => searchAccess.GetCategoryArtists(settings)));
            } else if (groupType == SearchFilterEnum.Category) {
                if (string.IsNullOrEmpty(settings.Search))
                    Result.Add(new SearchCategoryItem(SearchFilterEnum.Category, "", "No Category"));
                Result.AddRange(await Task.Run(() => searchAccess.GetCategoryCategories(settings)));
            } else if (groupType == SearchFilterEnum.Element)
                Result.AddRange(await Task.Run(() => searchAccess.GetCategoryElements(settings)));

            return Result;
        }

        /// <summary>
        /// Loads the Playlist from the database with specified filter conditions.
        /// </summary>
        /// <param name="settings">The filters to apply to the data.</param>
        public async Task LoadPlaylistAsync(SearchSettings settings) {
            await LoadPlaylistAsync(settings, false);
        }

        /// <summary>
        /// Loads the Playlist from the database with specified filter conditions.
        /// </summary>
        /// <param name="settings">The filters to apply to the data.</param>
        /// <param name="forceScanFolder">Whether to scan files in folder to set FileExists.</param>
        public async Task LoadPlaylistAsync(SearchSettings settings, bool forceScanFolder) {
            bool LoadFilesRequired = !(settings.IsInDatabase == true);

            // Load database.
            var DbTask = Task.Run(() => searchAccess.GetList(settings));

            // Load files.
            if (LoadFilesRequired || forceScanFolder) {
                if (files == null) {
                    // Run both tasks simultaneously.
                    var FilesTask = Task.Run(() => LoadFiles());
                    Task.WaitAll(DbTask, FilesTask);
                }
                Playlist = await DbTask;
                await Task.Run(() => MergePlaylist(settings));
            } else
                Playlist = await DbTask;
        }

        /// <summary>
        /// Loads all orphan files in the Natural Grounding folder as a list of strings.
        /// </summary>
        public void LoadFiles() {
            files = new List<LocalFileInfo>();

            if (!fileSystem.Directory.Exists(settings.NaturalGroundingFolder))
                return;

            // We must load all file names from the database to know which files are not in the database.
            List<string> AllFiles = searchAccess.GetAllFileNames();

            string ItemFile;
            bool ItemInDatabase;
            var FileEnum = fileSystem.Directory.EnumerateFiles(settings.NaturalGroundingFolder, "*.*", System.IO.SearchOption.AllDirectories);
            foreach (string f in FileEnum) {
                // Exclude temp folder.
                if (!f.StartsWith(appPath.LocalTempPath)) {
                    ItemFile = f.Substring(settings.NaturalGroundingFolder.Length);
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
            MediaListItem ListItem;
            MediaType ItemType;
            foreach (LocalFileInfo item in files) {
                ItemType = GetFileType(item.FileName);
                ListItem = Playlist.Where(v => string.Equals(v.FileName, item.FileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (ListItem != null)
                    ListItem.FileExists = true;
                else {
                    // Add file if it matches search settings.
                    if (!item.IsInDatabase && OrphanMatchesConditions(ItemType, item.FileName, settings)) {
                        // Add file to ListView.
                        Playlist.Add(new MediaListItem() {
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
                Playlist.RemoveAll(v => v.FileExists == (Cond.Value == BoolConditionEnum.No));
            // Apply IsInDatabase search filter.
            if (settings.IsInDatabase == false)
                Playlist.RemoveAll(v => v.IsInDatabase);
        }

        /// <summary>
        /// Loads the list of rating categories for the search interface.
        /// </summary>
        /// <param name="addSpecial">Whether to add special categories to the list: heigth, length, highest and preference.</param>
        /// <returns>A list of RatingCategory objects.</returns>
        public async Task<List<RatingCategory>> GetRatingCategoriesAsync(bool addSpecial) {
            List<RatingCategory> Result = await Task.Run(() => searchAccess.GetCustomRatingCategories());
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
                foreach (Media item in Query) {
                    // Try to auto-attach file if default file name exists.
                    if (item.FileName == null) {
                        DefaultFileName = defaultPath.GetDefaultFileName(item.Artist, item.Title, item.MediaCategoryId, item.MediaType);
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

        /// <summary>
        /// Loads the list of resources that can be purchased.
        /// </summary>
        /// <param name="settings">The search settings.</param>
        /// <param name="productType">The type of products to buy.</param>
        /// <param name="showAllFiles">Whether to show entries that already have a local file.</param>
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

        #endregion

        #region Orphans

        /// <summary>
        /// Returns what type of media a file is based on its extension.
        /// </summary>
        /// <param name="fileName">The path of the file to get the type of.</param>
        /// <returns></returns>
        public MediaType GetFileType(string fileName) {
            MediaType Result = MediaType.None;
            string Ext = fileSystem.Path.GetExtension(fileName).ToLower();
            if (appPath.VideoExtensions.Contains(Ext))
                Result = MediaType.Video;
            else if (appPath.AudioExtensions.Contains(Ext))
                Result = MediaType.Audio;
            else if (appPath.ImageExtensions.Contains(Ext))
                Result = MediaType.Image;
            return Result;
        }

        /// <summary>
        /// Returns whether specified orphan file (not in database) matches specified conditions.
        /// </summary>
        /// <param name="mediaType">The type of media of the file.</param>
        /// <param name="fileName">The path of the file on which to check condition.</param>
        /// <param name="settings">The conditions to apply on the file.</param>
        /// <returns>True if file matches conditions, otherwise false.</returns>
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
        /// Attempts to automatically attach a file to a database entry if the specified file name (with any extension) exists.
        /// </summary>
        /// <param name="item">The database Media item on which to attach the file.</param>
        /// <param name="fileName">The path where to look for a file, without extension. Every valid file extension will be tried.</param>
        /// <returns>Whether </returns>
        public bool AutoAttachFile(Media item, string fileName) {
            string FilePath;
            foreach (string ext in appPath.GetMediaTypeExtensions(item.MediaType)) {
                FilePath = settings.NaturalGroundingFolder + fileName + ext;
                if (fileSystem.File.Exists(FilePath)) {
                    item.FileName = fileName + ext;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Automatically binds files in the Playlist and load missing info.
        /// </summary>
        public async Task AutoBindFilesAsync() {
            // Auto-bind files
            EditPlaylistBusiness BindBusiness = new EditPlaylistBusiness();
            SearchSettings BindSettings = new SearchSettings();
            BindSettings.IsInDatabase = true;
            await BindBusiness.LoadPlaylistAsync(BindSettings);
            await BindBusiness.LoadMediaInfoAsync(null);
        }

        #endregion

        #region Metadata

        /// <summary>
        /// Returns whether specified Media has missing metadata (Length and Height).
        /// </summary>
        /// <param name="item">The Media item to check.</param>
        /// <returns>Whether metadata is missing.</returns>
        public bool FileEntryHasMissingInfo(Media item) {
            return (item.FileName != null &&
                fileSystem.File.Exists(settings.NaturalGroundingFolder + item.FileName)
                && (item.Length == null || (MediaTypeHasDimensions(item.MediaType) && item.Height == null)));
        }

        /// <summary>
        /// Returns whether specified media type has dimensions.
        /// </summary>
        /// <param name="mediaType">The type of media to check.</param>
        /// <returns>Whether the media type has dimensions.</returns>
        private bool MediaTypeHasDimensions(MediaType mediaType) {
            return mediaType == MediaType.Video || mediaType == MediaType.Image;
        }

        /// <summary>
        /// Loads the Length, Width and Height of specified media file.
        /// </summary>
        /// <param name="item">The media file to read.</param>
        /// <returns>True if data was loaded, false if no data was needed.</returns>
        public async Task LoadFileEntryInfoAsync(Media item) {
            IFileInfoFFmpeg FileInfo = await Task.Run(() => mediaInfo.GetFileInfo(settings.NaturalGroundingFolder + item.FileName));
            if (FileInfo.FileDuration > TimeSpan.Zero)
                item.Length = (short)FileInfo.FileDuration.TotalSeconds;
            if (FileInfo.VideoStream != null && FileInfo.VideoStream.Height > 0)
                item.Height = (short)FileInfo.VideoStream.Height;
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates the list of files after making changes to a Media entry.
        /// </summary>
        /// <param name="media">The Media entry that was modified.</param>
        /// <param name="oldFileName">The Media's file name before the edit.</param>
        public void RefreshPlaylist(Media media, string oldFileName) {
            if (media.FileName != null) {
                RefreshFilesCache(media.FileName, true);
                if (oldFileName != null && oldFileName != media.FileName)
                    RefreshFilesCache(oldFileName, false);
            } else if (oldFileName != null)
                RefreshFilesCache(oldFileName, false);
        }

        /// <summary>
        /// Updates the list of files after making changes to a Media entry.
        /// </summary>
        /// <param name="fileName">The file name to rescan.</param>
        /// <param name="isInDatabase">Whether that file is in the database.</param>
        private void RefreshFilesCache(string fileName, bool isInDatabase) {
            bool FileExists = fileSystem.File.Exists(settings.NaturalGroundingFolder + fileName);
            LocalFileInfo FileEntry = files.Where(f => f.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            if (FileEntry != null)
                FileEntry.IsInDatabase = isInDatabase;
            if (FileExists && FileEntry == null)
                files.Add(new LocalFileInfo(fileName, isInDatabase));
            else if (!FileExists && FileEntry != null)
                files.Remove(FileEntry);
        }

        /// <summary>
        /// Updates the file name of specified Media.
        /// </summary>
        /// <param name="videoId">The ID of the Media.</param>
        /// <param name="fileName">The new file name.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool UpdateFileName(Guid videoId, string fileName) {
            MediaListItem Item = Playlist.Find(v => v.MediaId == videoId);
            if (Item == null)
                Item = new MediaListItem() { MediaId = videoId };
            return UpdateFileName(Item, fileName);
        }

        /// <summary>
        /// Updates the file name of specified Media. It will fail if the file is already attached to another entry.
        /// </summary>
        /// <param name="item">The MediaListItem to update.</param>
        /// <param name="fileName">The new file name.</param>
        /// <returns>Whether the operation was successful.</returns>
        public bool UpdateFileName(MediaListItem item, string fileName) {
            if (mediaAccess.GetMediaByFileName(fileName) == null) {
                // Update database.
                Media DbItem = mediaAccess.GetMediaById(item.MediaId.Value);
                DbItem.FileName = fileName;
                mediaAccess.Save();
                // Update in-memory list.
                if (!item.FileExists) {
                    item.FileExists = true;
                    item.FileName = fileName;
                    Playlist.RemoveAll(v => v.FileName == fileName && v.IsInDatabase == false);
                } else if (!item.IsInDatabase) {
                    item.IsInDatabase = true;
                    item.FileName = fileName;
                    Playlist.RemoveAll(v => v.FileName == fileName && v.FileExists == false);
                }
                return true;
            } else
                return false;
        }

        #endregion

        //public List<MediaListItem> GetSortedPlaylist(string search, string orderBy, ListSortDirection orderDirection) {
        //    // return searchAccess.FilterAndSort(Playlist, search, orderBy, orderDirection);
        //    return null;
        //}

        /// <summary>
        /// Returns the 3 first letters of a rating category or the first letter of each word.
        /// </summary>
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
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using DataAccess;
using System.Collections.ObjectModel;
using System.Windows;
using EmergenceGuardian.Downloader;

namespace Business {
    public class PlayerBusiness {

        #region Declarations / Constructor

        private List<Guid> playedVideos = new List<Guid>();
        private IMediaPlayerBusiness player { get; set; }
        private Media nextVideo;
        private PlayerMode playMode = PlayerMode.Normal;
        private DispatcherTimer timerChangeConditions;
        private DispatcherTimer timerSession;
        private int sessionTotalSeconds;
        private int lastSearchResultCount;
        public bool IsStarted { get; private set; }
        public bool IsPaused { get; private set; }
        private DownloadBusiness downloadManager = new DownloadBusiness(Settings.SavedFile.Download);
        /// <summary>
        /// Contains all filter settings.
        /// </summary>
        public SearchSettings FilterSettings { get; set; }

        /// <summary>
        /// Gets or sets the type of media to play (audio or video).
        /// </summary>
        //public MediaType MediaType { get; set; }
        /// <summary>
        /// Gets or sets whether the intensity slider is at its lowest.
        /// </summary>
        public bool IsMinimumIntensity { get; set; }
        /// <summary>
        /// Gets or sets whether to loop current video.
        /// </summary>
        public bool Loop { get; set; }
        /// <summary>
        /// Gets or sets the options of next videos to choose from.
        /// </summary>
        public ObservableCollection<Media> NextVideoOptions { get; set; } = new ObservableCollection<Media>();

        /// <summary>
        /// Occurs before selecting a new video to get search conditions.
        /// </summary>
        public event EventHandler<GetConditionsEventArgs> GetConditions;
        /// <summary>
        /// Occurs when conditions need to be increased.
        /// </summary>
        public event EventHandler IncreaseConditions;
        /// <summary>
        /// Occurs after the playlist is updated.
        /// </summary>
        public event EventHandler PlaylistChanged;
        /// <summary>
        /// Occurs when the play time clock is updated.
        /// </summary>
        public event EventHandler DisplayPlayTime;
        /// <summary>
        /// Occurs when playing a new video.
        /// </summary>
        public event EventHandler<NowPlayingEventArgs> NowPlaying;

        /// <summary>
        /// Initializes a new instance of the PlayerBusiness class.
        /// </summary>
        public PlayerBusiness()
            : this(null) {
        }

        public PlayerBusiness(IMediaPlayerBusiness player) {
            FilterSettings = new SearchSettings();
            FilterSettings.MediaType = MediaType.Video;

            timerSession = new DispatcherTimer();
            timerSession.Interval = TimeSpan.FromSeconds(1);
            timerSession.Tick += sessionTimer_Tick;
            timerChangeConditions = new DispatcherTimer();
            timerChangeConditions.Interval = TimeSpan.FromSeconds(1);
            timerChangeConditions.Tick += timerChangeConditions_Tick;

            if (player != null)
                SetPlayer(player);
        }

        #endregion

        #region Properties

        public DownloadBusiness DownloadManager {
            get { return downloadManager; }
        }

        public Media CurrentVideo {
            get {
                if (player != null)
                    return player.CurrentVideo;
                else
                    return null;
            }
        }

        public bool IsPlaying {
            get { return player.IsPlaying; }
        }

        public int SessionTotalSeconds {
            get { return sessionTotalSeconds; }
        }

        public Media NextVideo {
            get { return nextVideo; }
        }

        public PlayerMode PlayMode {
            get { return playMode; }
            private set {
                playMode = value;
                Application.Current.Dispatcher.Invoke(() => PlaylistChanged?.Invoke(this, new EventArgs()));
            }
        }


        public int LastSearchResultCount {
            get { return lastSearchResultCount; }
        }

        public bool IgnorePos {
            get { return player.IgnorePos; }
            set { player.IgnorePos = value; }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// When the player finishes playing a video, select the next video.
        /// </summary>
        private async void player_PlayNext(object sender, EventArgs e) {
            if (!Loop) {
                var VideoDownload = GetNextVideoDownloading();
                if (VideoDownload == null) {
                    if (playMode == PlayerMode.Manual && nextVideo == null) {
                        if (player.CurrentVideo != null && (player.StartPos.HasValue || player.EndPos.HasValue) && !player.IgnorePos) // Enforce end position without moving to next video.
                            await player.SetPositionAsync(player.StartPos.HasValue ? player.StartPos.Value : 0).ConfigureAwait(false);
                        else
                            player.Position = 0;
                    } else {
                        // Play next video if it is not downloading.
                        if (PlayMode == PlayerMode.Normal) {
                            Application.Current.Dispatcher.Invoke(() => IncreaseConditions?.Invoke(this, new EventArgs()));
                        }
                        await PlayNextVideoAsync().ConfigureAwait(false);
                    }
                } else {
                    // If next video still downloading, restart current video.
                    // This method will be called again once download is completed.
                    if (player.CurrentVideo != null && player.StartPos.HasValue && !player.IgnorePos)
                        await player.SetPositionAsync(player.StartPos.Value).ConfigureAwait(false);
                    Application.Current.Dispatcher.Invoke(() => PlaylistChanged?.Invoke(this, new EventArgs()));
                }
            } else if (player.CurrentVideo != null && (player.StartPos.HasValue || player.EndPos.HasValue) && !player.IgnorePos) // Enforce end position without moving to next video.
                await player.SetPositionAsync(player.StartPos.HasValue ? player.StartPos.Value : 0).ConfigureAwait(false);
            else
                player.Position = 0;
        }

        /// <summary>
        /// When the player is playing a file, start the timer and notify the UI.
        /// </summary>
        private void player_NowPlaying(object sender, EventArgs e) {
            timerSession.Start();
            Application.Current.Dispatcher.Invoke(() => NowPlaying?.Invoke(this, new NowPlayingEventArgs()));
        }

        /// <summary>
        /// When the player stops, stop the timer and notify the UI.
        /// </summary>
        void player_Pause(object sender, EventArgs e) {
            timerSession.Stop();
            Application.Current?.Dispatcher?.Invoke(() => DisplayPlayTime?.Invoke(this, new EventArgs()));
        }

        /// <summary>
        /// When the player resumes, start the timer and notify the UI.
        /// </summary>
        void player_Resume(object sender, EventArgs e) {
            timerSession.Start();
            Application.Current.Dispatcher.Invoke(() => DisplayPlayTime?.Invoke(this, new EventArgs()));
        }

        /// <summary>
        /// When the play timer is updated, notify the UI.
        /// </summary>
        void sessionTimer_Tick(object sender, EventArgs e) {
            sessionTotalSeconds++;
            Application.Current.Dispatcher.Invoke(() => DisplayPlayTime?.Invoke(this, new EventArgs()));
        }

        /// <summary>
        /// After conditions were changed, ensure the next video still matches conditions.
        /// </summary>
        private async void timerChangeConditions_Tick(object sender, EventArgs e) {
            timerChangeConditions.Stop();
            if (PlayMode == PlayerMode.Water && !IsMinimumIntensity)
                await SetWaterVideosAsync(false).ConfigureAwait(false);
            else if (IsMinimumIntensity && !IsSpecialMode())
                await SetWaterVideosAsync(true).ConfigureAwait(false);
            else
                await SelectNextVideoAsync(1, true).ConfigureAwait(false);
        }

        public bool IsSpecialMode() {
            return playMode == PlayerMode.WarmPause || playMode == PlayerMode.RequestCategory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts the video player.
        /// </summary>
        public async Task RunPlayerAsync(string fileName) {
            player.AllowClose = false;
            player.Show();
            IsStarted = true;
            bool IsDownloaded = false;
            if (fileName != null)
                await SetNextVideoFileAsync(PlayerMode.Manual, fileName).ConfigureAwait(false);
            else
                IsDownloaded = await SelectNextVideoAsync(0, false).ConfigureAwait(false);
            if (!IsDownloaded) {
                await Task.Delay(1000).ConfigureAwait(false);
                await PlayNextVideoAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Closes the video player.
        /// </summary>
        public void ClosePlayer() {
            if (player != null)
                player.Close();
            MpcConfigBusiness.ClearLog();
        }

        public void PauseSession() {
            IsPaused = true;
            player.AllowClose = true;
            player.Hide();
        }

        public void ResumeSession() {
            IsPaused = false;
            player.AllowClose = false;
            player.Show();
        }

        /// <summary>
        /// When opening an editor window, this allows the player to be closed and be handled differently.
        /// </summary>
        /// <param name="value">True when opening the editor, false when closing.</param>
        public async Task SetEditorModeAsync(bool value) {
            if (value)
                player.AllowClose = true;
            else if (IsStarted && !IsPaused)
                player.AllowClose = false;
            if (!value && IsStarted)
                await SelectNextVideoAsync(1, false);
        }

        /// <summary>
        /// Manually sets the next video in queue.
        /// </summary>
        /// <param name="videoId">The ID of the video play next.</param>
        public async Task SetNextVideoIdAsync(PlayerMode mode, Guid videoId) {
            Media Result = PlayerAccess.GetVideoById(videoId);
            if (Result != null)
                await SetNextVideoAsync(mode, Result).ConfigureAwait(false);
        }

        /// <summary>
        /// Manually sets the next video in queue.
        /// </summary>
        /// <param name="fileName">The name of the file to play.</param>
        public async Task SetNextVideoFileAsync(PlayerMode mode, string fileName) {
            await SetNextVideoAsync(mode, GetMediaObject(fileName)).ConfigureAwait(false);
        }

        /// <summary>
        /// Plays a single video while allowing to close the player, such as from the Edit Video window.
        /// </summary>
        /// <param name="fileName">The name of the file to play.</param>
        public async Task PlaySingleVideoAsync(string fileName) {
            Media video = GetMediaObject(fileName);
            if (!player.IsAvailable || video == null)
                return;

            if (nextVideo != null) {
                CancelNextDownload(nextVideo);
                NextVideoOptions.Clear();
                nextVideo = video;
            }

            // Enable/Disable SVP if necessary.
            MpcConfigBusiness.AutoConfigure(video);
            // Auto-pitch to 432hz
            bool EnableAutoPitch = AutoPitchBusiness.AppyAutoPitch(video);

            await player.PlayVideoAsync(video, EnableAutoPitch).ConfigureAwait(false);

            Application.Current.Dispatcher.Invoke(() => PlaylistChanged?.Invoke(this, new EventArgs()));
        }

        public Media GetMediaObject(string fileName) {
            Media Result = PlayerAccess.GetVideoByFileName(fileName);
            if (Result == null)
                Result = new Media() { FileName = fileName, Title = fileName };
            return Result;
        }

        private async Task SetNextVideoAsync(PlayerMode mode, Media video) {
            if (nextVideo != null)
                CancelNextDownload(nextVideo);
            if (NextVideoOptions.Any()) {
                NextVideoOptions[0] = video;
            }
            nextVideo = video;
            this.PlayMode = mode;
            await PrepareNextVideoAsync(1, 0).ConfigureAwait(false);
            Application.Current.Dispatcher.Invoke(() => PlaylistChanged?.Invoke(this, new EventArgs()));
        }

        public async Task SetNextVideoOptionAsync(Media video) {
            // Don't reset if it's the same value.
            if (NextVideo != null && video.MediaId == NextVideo.MediaId)
                return;
            // Cancel next download.
            if (nextVideo != null)
                CancelNextDownload(nextVideo);
            // Set and prepare next video.
            nextVideo = video;
            await PrepareNextVideoAsync(1, 0).ConfigureAwait(false);
        }

        /// <summary>
        /// Cancels the download and autoplay of specified video.
        /// </summary>
        private void CancelNextDownload(Media video) {
            var VideoDownload = downloadManager.DownloadsList.FirstOrDefault(d => (d.Data as DownloadItemData).Media.MediaId == video.MediaId && !d.IsCompleted);
            if (VideoDownload != null) {
                // Removes autoplay from the next video.
                DownloadItemData IData = VideoDownload.Data as DownloadItemData;
                IData.QueuePos = -1;
                // Cancel the download if progress is less than 40%
                if (VideoDownload.ProgressValue < 40 && playMode != PlayerMode.Manual)
                    VideoDownload.Status = DownloadStatus.Canceled;
            }
        }

        /// <summary>
        /// Sets the next videos to egoless pause.
        /// </summary>
        /// <param name="enabled">True to enable this mode, false to restore normal session.</param>
        public async Task SetFunPauseAsync(bool enabled) {
            PlayMode = (enabled ? PlayerMode.WarmPause : PlayerMode.Normal);
            await SelectNextVideoAsync(0, false).ConfigureAwait(false);
            // PlayNextVideo();

            Application.Current.Dispatcher.Invoke(() => PlaylistChanged?.Invoke(this, new EventArgs()));
        }

        /// <summary>
        /// Apply specified search and rating category filters to the next videos.
        /// </summary>
        /// <param name="request">The search filters being requested, or null to disable the request category mode.</param>
        public async Task SetRequestCategoryAsync(SearchSettings request) {
            if (request != null) {
                FilterSettings.Search = request.Search;
                if (!string.IsNullOrEmpty(request.RatingCategory) && request.RatingValue.HasValue) {
                    FilterSettings.RatingCategory = request.RatingCategory;
                    FilterSettings.RatingOperator = request.RatingOperator;
                    FilterSettings.RatingValue = request.RatingValue;
                }

                playMode = PlayerMode.RequestCategory;
            } else {
                FilterSettings.Search = null;
                FilterSettings.RatingCategory = null;
                FilterSettings.RatingValue = null;
                playMode = PlayerMode.Normal;
            }

            await SelectNextVideoAsync(0, false).ConfigureAwait(false);

            Application.Current.Dispatcher.Invoke(() => PlaylistChanged?.Invoke(this, new EventArgs()));
        }

        /// <summary>
        /// Sets the next videos to water videos for cool down.
        /// </summary>
        /// <param name="enabled">True to enable this mode, false to restore normal session.</param>
        public async Task SetWaterVideosAsync(bool enabled) {
            PlayMode = (enabled ? PlayerMode.Water : PlayerMode.Normal);
            await SelectNextVideoAsync(1, false).ConfigureAwait(false);

            Application.Current.Dispatcher.Invoke(() => PlaylistChanged?.Invoke(this, new EventArgs()));
        }

        /// <summary>
        /// Plays the next video.
        /// </summary>
        private async Task PlayNextVideoAsync() {
            if (nextVideo == null)
                return;

            // Enable/Disable SVP if necessary.
            MpcConfigBusiness.AutoConfigure(nextVideo);

            // If next video is still downloading, advance QueuePos. If QueuePos = 0 when download finishes, it will auto-play.
            var VideoDownload = GetNextVideoDownloading();
            if (VideoDownload != null) {
                DownloadItemData IData = VideoDownload.Data as DownloadItemData;
                if (IData.QueuePos > 0)
                    IData.QueuePos--;
                return;
            }

            // Auto-pitch to 432hz
            bool EnableAutoPitch = AutoPitchBusiness.AppyAutoPitch(nextVideo);

            await player.PlayVideoAsync(nextVideo, EnableAutoPitch).ConfigureAwait(false);
            playedVideos.Add(nextVideo.MediaId);
            nextVideo = null;

            if (PlayMode == PlayerMode.SpecialRequest)
                PlayMode = PlayerMode.Normal;

            if (playMode != PlayerMode.Manual)
                await SelectNextVideoAsync(1, false).ConfigureAwait(false);

            if (PlayMode == PlayerMode.Fire)
                PlayMode = PlayerMode.SpecialRequest;

            Application.Current.Dispatcher.Invoke(() => PlaylistChanged?.Invoke(this, new EventArgs()));
        }

        /// <summary>
        /// Selects which video will be played next.
        /// </summary>
        /// <param name="queuePos">The video position to select. 0 for current, 1 for next.</param>
        /// <param name="maintainCurrent">True to keep the next video and only select alternate options, false to change next video.</param>
        /// <returns>Whether the file is downloading.</returns>
        public async Task<bool> SelectNextVideoAsync(int queuePos, bool maintainCurrent) {
            return await SelectNextVideoAsync(queuePos, maintainCurrent, 0).ConfigureAwait(false);
        }

        /// <summary>
        /// Selects which video will be played next.
        /// </summary>
        /// <param name="queuePos">The video position to select. 0 for current, 1 for next.</param>
        /// <param name="maintainCurrent">True to keep the next video and only select alternate options, false to change next video.</param>
        /// <param name="attempts">The number of attemps already made, to avoid infinite loop.</param>
        /// <returns>Whether the file is downloading.</returns>
        private async Task<bool> SelectNextVideoAsync(int queuePos, bool maintainCurrent, int attempts) {
            bool IsDownloading = false;
            if (attempts > 3) {
                nextVideo = null;
                return false;
            }

            timerChangeConditions.Stop();

            // Get video conditions
            GetConditionsEventArgs e = new GetConditionsEventArgs(FilterSettings);
            e.QueuePos = queuePos;
            Application.Current.Dispatcher.Invoke(() => GetConditions?.Invoke(this, e));

            // Select random video matching conditions.
            List<Media> Result = PlayerAccess.SelectVideo(FilterSettings.Update(playedVideos, Settings.SavedFile.AutoDownload), 3, maintainCurrent ? nextVideo : null);
            lastSearchResultCount = FilterSettings.TotalFound;

            // If no video is found, try again while increasing tolerance
            if (Result == null) {
                e = new GetConditionsEventArgs(FilterSettings);
                e.IncreaseTolerance = true;
                Application.Current.Dispatcher.Invoke(() => GetConditions?.Invoke(this, e));
                Result = PlayerAccess.SelectVideo(FilterSettings.Update(null, Settings.SavedFile.AutoDownload), 3, maintainCurrent ? nextVideo : null);
                FilterSettings.TotalFound = lastSearchResultCount;
            }

            if (Result != null) {
                if (nextVideo != null)
                    CancelNextDownload(nextVideo);
                NextVideoOptions = new ObservableCollection<Media>(Result);
                nextVideo = Result.FirstOrDefault();
                IsDownloading = await PrepareNextVideoAsync(queuePos, attempts).ConfigureAwait(false);
            }

            timerChangeConditions.Stop();
            Application.Current.Dispatcher.Invoke(() => PlaylistChanged?.Invoke(this, new EventArgs()));

            return IsDownloading;
        }

        /// <summary>
        /// Prepares for playing the next video.
        /// </summary>
        /// <param name="queuePos">The video position to select. 0 for current, 1 for next.</param>
        /// <param name="attempts">The number of attemps already made, to avoid infinite loop.</param>
        /// <returns>Whether the file is downloading.</returns>
        private async Task<bool> PrepareNextVideoAsync(int queuePos, int attempts) {
            bool FileExists = false;
            if (nextVideo != null)
                FileExists = File.Exists(Settings.NaturalGroundingFolder + nextVideo.FileName);

            if (!FileExists) {
                // If file doesn't exist and can't be downloaded, select another one.
                if (!Settings.SavedFile.AutoDownload || nextVideo == null || nextVideo.DownloadUrl.Length == 0)
                    await SelectNextVideoAsync(queuePos, false, attempts + 1).ConfigureAwait(false);
                // If file doesn't exist and can be downloaded, download it.
                else if (nextVideo != null && nextVideo.DownloadUrl.Length > 0) {
                    Application.Current.Dispatcher.Invoke(() => PlaylistChanged?.Invoke(this, new EventArgs()));
                    await downloadManager.DownloadVideoAsync(nextVideo, queuePos, Download_Complete).ConfigureAwait(false);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Occurs when download is completed.
        /// </summary>
        private async void Download_Complete(object sender, DownloadCompletedEventArgs args) {
            DownloadItemData IData = args.DownloadInfo.Data as DownloadItemData;
            if (args.DownloadInfo.IsCompleted && (IData.QueuePos == 0 || player.CurrentVideo == null) && !player.AllowClose) {
                nextVideo = IData.Media;
                player_PlayNext(null, null);
            } else if (args.DownloadInfo.IsCanceled && IData.QueuePos > -1 && playMode != PlayerMode.Manual) {
                nextVideo = null;
                await SelectNextVideoAsync(IData.QueuePos, false).ConfigureAwait(false);
            }
        }

        public void ChangeConditions() {
            if (player != null && player.CurrentVideo != null) {
                timerChangeConditions.Stop();
                timerChangeConditions.Start();
            }
        }

        public async Task SkipVideoAsync() {
            if (!player.IsAvailable)
                return;

            if (playedVideos.Count > 0)
                playedVideos.RemoveAt(playedVideos.Count - 1);
            // await EnsureNextVideoMatchesConditionsAsync(true);
            await PlayNextVideoAsync().ConfigureAwait(false);
        }

        public async Task ReplayLastAsync() {
            if (!player.IsAvailable)
                return;

            if (playedVideos.Count > 1) {
                Media LastVideo = PlayerAccess.GetVideoById(playedVideos[playedVideos.Count - 2]);
                if (LastVideo.MediaId != player.CurrentVideo.MediaId) {
                    playedVideos.RemoveAt(playedVideos.Count - 1);
                    if (nextVideo != null)
                        CancelNextDownload(nextVideo);
                    nextVideo = player.CurrentVideo;

                    // Enable/Disable SVP if necessary.
                    MpcConfigBusiness.AutoConfigure(nextVideo);

                    // Auto-pitch to 432hz
                    bool EnableAutoPitch = AutoPitchBusiness.AppyAutoPitch(LastVideo);

                    await player.PlayVideoAsync(LastVideo, EnableAutoPitch).ConfigureAwait(false);
                    Application.Current.Dispatcher.Invoke(() => PlaylistChanged?.Invoke(this, new EventArgs()));
                }
            }
        }

        /// <summary>
        /// Enable/Disable SVP and madVR if necessary.
        /// </summary>
        public void ConfigurePlayer() {
            if (Settings.SavedFile.MediaPlayerApp == MediaPlayerApplication.Mpc) {
                if (CurrentVideo != null)
                    MpcConfigBusiness.AutoConfigure(CurrentVideo);
                else
                    MpcConfigBusiness.AutoConfigure(null);
            }
        }

        //public async Task EnsureNextVideoMatchesConditionsAsync(bool skipping) {
        //    int QueuePos = skipping ? 0 : 1;
        //    if (nextVideo != null && PlayMode == PlayerMode.Normal) {
        //        // Get new conditions
        //        GetConditionsEventArgs Args = new GetConditionsEventArgs(FilterSettings);
        //        Args.QueuePos = QueuePos;
        //        if (GetConditions != null)
        //            GetConditions(this, Args);

        //        // Run the conditions on the next video in the playlist
        //        if (!PlayerAccess.VideoMatchesConditions(nextVideo, FilterSettings.Update(playedVideos, Settings.SavedFile.AutoDownload))) {
        //            // If next video doesn't match conditions, select another one
        //            await SelectNextVideoAsync(QueuePos);
        //        }
        //    } else if (nextVideo == null)
        //        await SelectNextVideoAsync(QueuePos);
        //}

        public void ReloadVideoInfo() {
            if (player.CurrentVideo != null) {
                player.CurrentVideo = PlayerAccess.GetVideoById(player.CurrentVideo.MediaId);
                Application.Current.Dispatcher.Invoke(() => NowPlaying?.Invoke(this, new NowPlayingEventArgs(true)));
            }
        }

        /// <summary>
        /// Loads the list of rating categories for the Elements combobox.
        /// </summary>
        /// <returns>A list of RatingCategory objects.</returns>
        public List<RatingCategory> GetFocusCategories() {
            List<RatingCategory> Result = new List<RatingCategory>();
            Result.Add(new RatingCategory() { Name = "" });
            Result.Add(new RatingCategory() { Name = "Intensity" });
            Result.Add(new RatingCategory() { Name = "Physical" });
            Result.Add(new RatingCategory() { Name = "Emotional" });
            Result.Add(new RatingCategory() { Name = "Spiritual" });
            return Result;
        }

        /// <summary>
        /// Loads the list of rating categories for the Elements combobox.
        /// </summary>
        /// <returns>A list of RatingCategory objects.</returns>
        public async Task<List<RatingCategory>> GetElementCategoriesAsync() {
            List<RatingCategory> Result = new List<RatingCategory>();
            List<RatingCategory> DbResult = await Task.Run(() => SearchVideoAccess.GetCustomRatingCategories());
            Result.Add(new RatingCategory() { Name = "" });
            Result.Add(new RatingCategory() { Name = "Egoless" });
            Result.Add(new RatingCategory() { Name = "Love" });
            Result.AddRange(DbResult);
            return Result;
        }

        /// <summary>
        /// If the next video is still downloading, returns its download information.
        /// </summary>
        /// <returns>An object containing the information about the download.</returns>
        public DownloadItem GetNextVideoDownloading() {
            if (nextVideo != null) {
                var VideoDownload = downloadManager.DownloadsList.FirstOrDefault(d => (d.Data as DownloadItemData).Media.MediaId == nextVideo.MediaId);
                if (VideoDownload != null && !VideoDownload.IsCompleted)
                    return VideoDownload;
            }
            return null;
        }

        /// <summary>
        /// Uses specified player business object to play the session.
        /// </summary>
        /// <param name="player">The player business object through which to play the session.</param>
        public void SetPlayer(IMediaPlayerBusiness player) {
            player.SetPath();
            if (this.player != null) {
                player.AllowClose = this.player.AllowClose;
                if (this.player.CurrentVideo != null)
                    player.CurrentVideo = this.player.CurrentVideo;
            }

            // If changing player, close the previous one.
            if (this.player != null)
                this.player.Close();

            this.player = player;
            player.PlayNext += player_PlayNext;
            player.NowPlaying += player_NowPlaying;
            player.Pause += player_Pause;
            player.Resume += player_Resume;

            if (player.CurrentVideo != null)
                player.PlayVideoAsync(player.CurrentVideo, false);
        }

        public void ResetPlayerMode() {
            playMode = PlayerMode.Normal;
        }

        #endregion
    }

    #region GetConditionsEventHandler

    /// <summary>
    /// Contains information for the GetConditions event. The event handler should fill Conditions with search conditions.
    /// </summary>
    public class GetConditionsEventArgs : EventArgs {
        /// <summary>
        /// Gets or sets whether to widen the range of criterias, if none were previously found.
        /// </summary>
        public bool IncreaseTolerance { get; set; }
        /// <summary>
        /// Gets or sets the position in the playlist pos to fill.
        /// </summary>
        public int QueuePos { get; set; }
        /// <summary>
        /// Gets or sets the conditions to use for searching the next video.
        /// </summary>
        public SearchSettings Conditions { get; set; }

        /// <summary>
        /// Initializes a new instance of the GetConditionsEventArgs class.
        /// </summary>
        public GetConditionsEventArgs() {
        }

        public GetConditionsEventArgs(SearchSettings conditions) {
            this.Conditions = conditions;
            this.Conditions.RatingFilters = new List<SearchRatingSetting>();
            this.Conditions.RatingFilters.Add(new SearchRatingSetting());
        }
    }

    public class NowPlayingEventArgs : EventArgs {
        /// <summary>
        /// Gets or sets whether the media info was edited and must be reloaded.
        /// </summary>
        public bool ReloadInfo { get; set; }

        /// <summary>
        /// Initializes a new instance of the NowPlayingEventArgs class.
        /// </summary>
        public NowPlayingEventArgs() {
        }

        /// <summary>
        /// Initializes a new instance of the NowPlayingEventArgs class.
        /// </summary>
        /// <param name="reloadInfo">Whether data was edited and must be reloaded.</param>
        public NowPlayingEventArgs(bool reloadInfo) {
            this.ReloadInfo = reloadInfo;
        }
    }

    #endregion

    #region PlayerMode

    public enum PlayerMode {
        Normal,
        SpecialRequest,
        RequestCategory,
        WarmPause,
        Fire,
        Water,
        Manual
    }

    #endregion
}

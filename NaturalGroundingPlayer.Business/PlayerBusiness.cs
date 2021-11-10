using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using HanumanInstitute.CommonServices;
using HanumanInstitute.DownloadManager;
using HanumanInstitute.NaturalGroundingPlayer.DataAccess;

namespace HanumanInstitute.NaturalGroundingPlayer.Business {

    #region Interface

    /// <summary>
    /// Manages the Natural Groundiong playback session. This is the core of the application that plugs everything else together.
    /// </summary>
    public interface IPlayerBusiness {
        /// <summary>
        /// Returns whether the session has started.
        /// </summary>
        bool IsStarted { get; }
        /// <summary>
        /// Returns whether the session is on pause.
        /// </summary>
        bool IsPaused { get; }
        /// <summary>
        /// Contains all filter settings.
        /// </summary>
        SearchSettings FilterSettings { get; set; }
        /// <summary>
        /// Gets or sets whether the intensity slider is at its lowest.
        /// </summary>
        bool IsMinimumIntensity { get; set; }
        /// <summary>
        /// Gets or sets whether to loop current video.
        /// </summary>
        bool Loop { get; set; }
        /// <summary>
        /// Gets or sets the options of next videos to choose from.
        /// </summary>
        ObservableCollection<Media> NextVideoOptions { get; set; }
        /// <summary>
        /// Occurs before selecting a new video to get search conditions.
        /// </summary>
        event EventHandler<GetConditionsEventArgs> GetConditions;
        /// <summary>
        /// Occurs when conditions need to be increased.
        /// </summary>
        event EventHandler IncreaseConditions;
        /// <summary>
        /// Occurs after the playlist is updated.
        /// </summary>
        event EventHandler PlaylistChanged;
        /// <summary>
        /// Occurs when the play time clock is updated.
        /// </summary>
        event EventHandler DisplayPlayTime;
        /// <summary>
        /// Occurs when playing a new video.
        /// </summary>
        event EventHandler<NowPlayingEventArgs> NowPlaying;
        /// <summary>
        /// Returns whether the media player is currently playing.
        /// </summary>
        bool IsPlaying { get; }
        /// <summary>
        /// Returns the playing session length.
        /// </summary>
        TimeSpan SessionLength { get; }
        /// <summary>
        /// Returns the video currently playing, or null.
        /// </summary>
        Media CurrentVideo { get; }
        /// <summary>
        /// Returns the video that will play next.
        /// </summary>
        Media NextVideo { get; }
        /// <summary>
        /// Gets or sets the playback mode, which alters the video selection filters.
        /// </summary>
        PlayerMode PlayMode { get; }
        /// <summary>
        /// Returns the amount of videos found by the last search query.
        /// </summary>
        int LastSearchResultCount { get; }
        /// <summary>
        /// Gets or sets whether to ignore start and end positions defined on the videos.
        /// </summary>
        bool IgnorePos { get; set; }
        /// <summary>
        /// Returns true if play mode is WarmPause or RequestCategory.
        /// </summary>
        bool IsSpecialMode();
        /// <summary>
        /// Shows the video player and plays specified media file.
        /// </summary>
        Task ShowPlayerAsync(string fileName);
        /// <summary>
        /// Closes the video player.
        /// </summary>
        void ClosePlayer();
        /// <summary>
        /// Pauses the session and closes the player.
        /// </summary>
        void PauseSession();
        /// <summary>
        /// Resumes the session and shows the player.
        /// </summary>
        void ResumeSession();
        /// <summary>
        /// When opening an editor window, this allows the player to be closed and be handled differently.
        /// </summary>
        /// <param name="value">True when opening the editor, false when closing.</param>
        Task SetEditorModeAsync(bool value);
        /// <summary>
        /// Manually sets the next video in queue.
        /// </summary>
        /// <param name="videoId">The ID of the video play next.</param>
        Task SetNextVideoIdAsync(PlayerMode mode, Guid videoId);
        /// <summary>
        /// Manually sets the next video in queue.
        /// </summary>
        /// <param name="fileName">The name of the file to play.</param>
        Task SetNextVideoFileAsync(PlayerMode mode, string fileName);
        /// <summary>
        /// Plays a single video while allowing to close the player, such as from the Edit Video window.
        /// </summary>
        /// <param name="fileName">The name of the file to play.</param>
        Task PlaySingleVideoAsync(string fileName);
        /// <summary>
        /// Returns a Media object with specified file name from the database, or an empty object with that file name.
        /// </summary>
        /// <param name="fileName">The file name to get a Media object for.</param>
        /// <returns>A Media object with FileName set to the desired value.</returns>
        Media GetMediaObject(string fileName);
        /// <summary>
        /// Sets the next video from the list of suggestions.
        /// </summary>
        /// <param name="video">A Media from the list of suggestions.</param>
        Task SetNextVideoOptionAsync(Media video);
        /// <summary>
        /// Sets the next videos to egoless pause.
        /// </summary>
        /// <param name="enabled">True to enable this mode, false to restore normal session.</param>
        Task SetFunPauseAsync(bool enabled);
        /// <summary>
        /// Apply specified search and rating category filters to the next videos.
        /// </summary>
        /// <param name="request">The search filters being requested, or null to disable the request category mode.</param>
        Task SetRequestCategoryAsync(SearchSettings request);
        /// <summary>
        /// Sets the next videos to water videos for cool down.
        /// </summary>
        /// <param name="enabled">True to enable this mode, false to restore normal session.</param>
        Task SetWaterVideosAsync(bool enabled);
        /// <summary>
        /// Selects which video will be played next.
        /// </summary>
        /// <param name="queuePos">The video position to select. 0 for current, 1 for next.</param>
        /// <param name="maintainCurrent">True to keep the next video and only select alternate options, false to change next video.</param>
        /// <returns>Whether the file is downloading.</returns>
        Task<bool> SelectNextVideoAsync(int queuePos, bool maintainCurrent);
        /// <summary>
        /// Notifies that conditions changed, which will trigger a requery after an interval.
        /// </summary>
        void ChangeConditions();
        /// <summary>
        /// Skips the currently playing video and plays the next one.
        /// </summary>
        Task SkipVideoAsync();
        /// <summary>
        /// Replays the last media file that played before this one.
        /// </summary>
        Task ReplayLastAsync();
        /// <summary>
        /// Enable/Disable SVP and madVR if necessary.
        /// </summary>
        void ConfigurePlayer();
        /// <summary>
        /// Reloads the Media object in CurrentVideo from the database.
        /// </summary>
        void ReloadVideoInfo();
        /// <summary>
        /// Loads the list of rating categories for the Elements combobox.
        /// </summary>
        /// <returns>A list of RatingCategory objects.</returns>
        List<RatingCategory> GetFocusCategories();
        /// <summary>
        /// Loads the list of rating categories for the Elements combobox.
        /// </summary>
        /// <returns>A list of RatingCategory objects.</returns>
        Task<List<RatingCategory>> GetElementCategoriesAsync();
        /// <summary>
        /// If the next video is still downloading, returns its download information.
        /// </summary>
        /// <returns>An object containing the information about the download.</returns>
        DownloadMediaInfo GetNextVideoDownloading();
        /// <summary>
        /// Uses specified player business object to play the session.
        /// </summary>
        /// <param name="player">The player business object through which to play the session.</param>
        void SetPlayer(IMediaPlayer player);
    }

    #endregion

    /// <summary>
    /// Manages the Natural Groundiong playback session. This is the core of the application that plugs everything else together.
    /// </summary>
    public class PlayerBusiness : IPlayerBusiness {

        #region Declarations / Constructors

        private List<Guid> playedVideos = new List<Guid>();
        private Media nextVideo;
        private PlayerMode playMode = PlayerMode.Normal;
        private DispatcherTimer timerChangeConditions;
        private DispatcherTimer timerSession;
        private int sessionLengthSeconds;
        private int lastSearchResultCount;
        /// <summary>
        /// Returns whether the session has started.
        /// </summary>
        public bool IsStarted { get; private set; }
        /// <summary>
        /// Returns whether the session is on pause.
        /// </summary>
        public bool IsPaused { get; private set; }
        /// <summary>
        /// Contains all filter settings.
        /// </summary>
        public SearchSettings FilterSettings { get; set; } = new SearchSettings();
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

        private IMediaPlayer player;
        private IPlayerAccess playerAccess;
        private ISearchMediaAccess searchAccess;
        protected readonly IFileSystemService fileSystem;
        private IDownloadBusiness download;
        private IPlaybackConfiguration config;
        protected readonly ISvpConfiguration svp;
        private IAutoPitchBusiness autoPitch;
        protected readonly ISettings settings;

        public PlayerBusiness() { }

        public PlayerBusiness(IMediaPlayer player, IPlayerAccess playerAccess, ISearchMediaAccess searchAccess, IFileSystemService fileSystemService, IDownloadBusiness downloadBusiness, IPlaybackConfiguration playbackConfig, ISvpConfiguration svpConfig, IAutoPitchBusiness autoPitchBusiness, ISettings settings) {
            this.player = player ?? throw new ArgumentNullException(nameof(player));
            this.playerAccess = playerAccess ?? throw new ArgumentNullException(nameof(playerAccess));
            this.searchAccess = searchAccess ?? throw new ArgumentNullException(nameof(searchAccess));
            this.fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
            this.download = downloadBusiness ?? throw new ArgumentNullException(nameof(downloadBusiness));
            this.config = playbackConfig ?? throw new ArgumentNullException(nameof(playbackConfig));
            this.svp = svpConfig ?? throw new ArgumentNullException(nameof(svpConfig));
            this.autoPitch = autoPitchBusiness ?? throw new ArgumentNullException(nameof(autoPitchBusiness));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            timerSession = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, sessionTimer_Tick, null);
            timerChangeConditions = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.DataBind, timerChangeConditions_Tick, null);

            if (player != null)
                SetPlayer(player);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns whether the media player is currently playing.
        /// </summary>
        public bool IsPlaying => player.IsPlaying;
        
        /// <summary>
        /// Returns the playing session length.
        /// </summary>
        public TimeSpan SessionLength => TimeSpan.FromSeconds(sessionLengthSeconds);
        
        /// <summary>
        /// Returns the video currently playing, or null.
        /// </summary>
        public Media CurrentVideo => player?.CurrentVideo;
        
        /// <summary>
        /// Returns the video that will play next.
        /// </summary>
        public Media NextVideo => nextVideo;
        
        /// <summary>
        /// Gets or sets the playback mode, which alters the video selection filters.
        /// </summary>
        public PlayerMode PlayMode {
            get => playMode; 
            private set {
                playMode = value;
                PlaylistChanged?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Returns the amount of videos found by the last search query.
        /// </summary>
        public int LastSearchResultCount => lastSearchResultCount;

        /// <summary>
        /// Gets or sets whether to ignore start and end positions defined on the videos.
        /// </summary>
        public bool IgnorePos {
            get => player.IgnorePos;
            set => player.IgnorePos = value;
        }

        /// <summary>
        /// Returns true if play mode is WarmPause or RequestCategory.
        /// </summary>
        public bool IsSpecialMode() => playMode == PlayerMode.WarmPause || playMode == PlayerMode.RequestCategory;

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
                            IncreaseConditions?.Invoke(this, new EventArgs());
                        }
                        await PlayNextVideoAsync().ConfigureAwait(false);
                    }
                } else {
                    // If next video still downloading, restart current video.
                    // This method will be called again once download is completed.
                    if (player.CurrentVideo != null && player.StartPos.HasValue && !player.IgnorePos)
                        await player.SetPositionAsync(player.StartPos.Value).ConfigureAwait(false);
                    PlaylistChanged?.Invoke(this, new EventArgs());
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
            NowPlaying?.Invoke(this, new NowPlayingEventArgs());
        }

        /// <summary>
        /// When the player stops, stop the timer and notify the UI.
        /// </summary>
        private void player_Pause(object sender, EventArgs e) {
            timerSession.Stop();
            DisplayPlayTime?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// When the player resumes, start the timer and notify the UI.
        /// </summary>
        private void player_Resume(object sender, EventArgs e) {
            timerSession.Start();
            DisplayPlayTime?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// When the play timer is updated, notify the UI.
        /// </summary>
        private void sessionTimer_Tick(object sender, EventArgs e) {
            sessionLengthSeconds++;
            DisplayPlayTime?.Invoke(this, new EventArgs());
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

        #endregion

        #region Methods

        /// <summary>
        /// Shows the video player and plays specified media file.
        /// </summary>
        public async Task ShowPlayerAsync(string fileName) {
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
            player?.Close();
            svp.ClearLog();
        }

        /// <summary>
        /// Pauses the session and closes the player.
        /// </summary>
        public void PauseSession() {
            IsPaused = true;
            player.AllowClose = true;
            player.Hide();
        }

        /// <summary>
        /// Resumes the session and shows the player.
        /// </summary>
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
            Media Result = playerAccess.GetVideoById(videoId);
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
            config.AutoConfigure(video);
            // Auto-pitch to 432hz
            bool EnableAutoPitch = autoPitch.AppyAutoPitch(video);

            await player.PlayVideoAsync(video, EnableAutoPitch).ConfigureAwait(false);

            PlaylistChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Returns a Media object with specified file name from the database, or an empty object with that file name.
        /// </summary>
        /// <param name="fileName">The file name to get a Media object for.</param>
        /// <returns>A Media object with FileName set to the desired value.</returns>
        public Media GetMediaObject(string fileName) {
            Media Result = playerAccess.GetVideoByFileName(fileName);
            if (Result == null)
                Result = new Media() { FileName = fileName, Title = fileName };
            return Result;
        }

        /// <summary>
        /// Sets the next video to play and the playback mode.
        /// </summary>
        /// <param name="mode">The playback mode, taking effect immediately.</param>
        /// <param name="video">The video that will be played next.</param>
        private async Task SetNextVideoAsync(PlayerMode mode, Media video) {
            if (nextVideo != null)
                CancelNextDownload(nextVideo);
            if (NextVideoOptions.Any()) {
                NextVideoOptions[0] = video;
            }
            nextVideo = video;
            this.PlayMode = mode;
            await PrepareNextVideoAsync(1, 0).ConfigureAwait(false);
            PlaylistChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Sets the next video from the list of suggestions.
        /// </summary>
        /// <param name="video">A Media from the list of suggestions.</param>
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
            var VidDown = download.GetActiveDownloadByMediaId(video.MediaId);
            if (VidDown != null) {
                // Removes autoplay from the next video.
                VidDown.QueuePos = -1;
                // Cancel the download if progress is less than 40%
                if (VidDown.ProgressValue < 40 && playMode != PlayerMode.Manual)
                    VidDown.Status = DownloadStatus.Canceled;
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

            PlaylistChanged?.Invoke(this, new EventArgs());
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

            PlaylistChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Sets the next videos to water videos for cool down.
        /// </summary>
        /// <param name="enabled">True to enable this mode, false to restore normal session.</param>
        public async Task SetWaterVideosAsync(bool enabled) {
            PlayMode = enabled ? PlayerMode.Water : PlayerMode.Normal;
            await SelectNextVideoAsync(1, false).ConfigureAwait(false);

            PlaylistChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Plays the next video.
        /// </summary>
        private async Task PlayNextVideoAsync() {
            if (nextVideo == null)
                return;

            // Enable/Disable SVP if necessary.
            config.AutoConfigure(nextVideo);

            // If next video is still downloading, advance QueuePos. If QueuePos = 0 when download finishes, it will auto-play.
            var VidDown = GetNextVideoDownloading();
            if (VidDown != null) {
                if (VidDown.QueuePos > 0)
                    VidDown.QueuePos--;
                return;
            }

            // Auto-pitch to 432hz
            bool EnableAutoPitch = autoPitch.AppyAutoPitch(nextVideo);

            await player.PlayVideoAsync(nextVideo, EnableAutoPitch).ConfigureAwait(false);
            playedVideos.Add(nextVideo.MediaId);
            nextVideo = null;

            if (PlayMode == PlayerMode.SpecialRequest)
                PlayMode = PlayerMode.Normal;

            if (playMode != PlayerMode.Manual)
                await SelectNextVideoAsync(1, false).ConfigureAwait(false);

            if (PlayMode == PlayerMode.Fire)
                PlayMode = PlayerMode.SpecialRequest;

            PlaylistChanged?.Invoke(this, new EventArgs());
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
            GetConditions?.Invoke(this, e);

            // Select random video matching conditions.
            List<Media> Result = playerAccess.SelectVideo(FilterSettings.Update(playedVideos, settings.Data.AutoDownload), 3, maintainCurrent ? nextVideo : null);
            lastSearchResultCount = FilterSettings.TotalFound;

            // If no video is found, try again while increasing tolerance
            if (Result == null) {
                e = new GetConditionsEventArgs(FilterSettings);
                e.IncreaseTolerance = true;
                GetConditions?.Invoke(this, e);
                Result = playerAccess.SelectVideo(FilterSettings.Update(null, settings.Data.AutoDownload), 3, maintainCurrent ? nextVideo : null);
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
            PlaylistChanged?.Invoke(this, new EventArgs());

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
                FileExists = fileSystem.File.Exists(settings.NaturalGroundingFolder + nextVideo.FileName);

            if (!FileExists) {
                // If file doesn't exist and can't be downloaded, select another one.
                if (!settings.Data.AutoDownload || nextVideo == null || nextVideo.DownloadUrl.Length == 0)
                    await SelectNextVideoAsync(queuePos, false, attempts + 1).ConfigureAwait(false);
                // If file doesn't exist and can be downloaded, download it.
                else if (nextVideo != null && nextVideo.DownloadUrl.Length > 0) {
                    PlaylistChanged?.Invoke(this, new EventArgs());
                    await download.DownloadVideoAsync(nextVideo, queuePos, Download_Complete).ConfigureAwait(false);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Occurs when download is completed.
        /// </summary>
        private async void Download_Complete(object sender, DownloadTaskEventArgs args) {
            DownloadMediaInfo VidDown = args.DownloadTask as DownloadMediaInfo;
            if (args.DownloadTask.IsCompleted && (VidDown.QueuePos == 0 || player.CurrentVideo == null) && !player.AllowClose) {
                nextVideo = VidDown.Media;
                player_PlayNext(null, null);
            } else if (args.DownloadTask.IsCancelled && VidDown.QueuePos > -1 && playMode != PlayerMode.Manual) {
                nextVideo = null;
                await SelectNextVideoAsync(VidDown.QueuePos, false).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Notifies that conditions changed, which will trigger a requery after an interval.
        /// </summary>
        public void ChangeConditions() {
            if (player != null && player.CurrentVideo != null) {
                timerChangeConditions.Stop();
                timerChangeConditions.Start();
            }
        }

        /// <summary>
        /// Skips the currently playing video and plays the next one.
        /// </summary>
        public async Task SkipVideoAsync() {
            if (!player.IsAvailable)
                return;

            if (playedVideos.Count > 0)
                playedVideos.RemoveAt(playedVideos.Count - 1);
            // await EnsureNextVideoMatchesConditionsAsync(true);
            await PlayNextVideoAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Replays the last media file that played before this one.
        /// </summary>
        public async Task ReplayLastAsync() {
            if (!player.IsAvailable)
                return;

            if (playedVideos.Count > 1) {
                Media LastVideo = playerAccess.GetVideoById(playedVideos[playedVideos.Count - 2]);
                if (LastVideo.MediaId != player.CurrentVideo.MediaId) {
                    playedVideos.RemoveAt(playedVideos.Count - 1);
                    if (nextVideo != null)
                        CancelNextDownload(nextVideo);
                    nextVideo = player.CurrentVideo;

                    // Enable/Disable SVP if necessary.
                    config.AutoConfigure(nextVideo);

                    // Auto-pitch to 432hz
                    bool EnableAutoPitch = autoPitch.AppyAutoPitch(LastVideo);

                    await player.PlayVideoAsync(LastVideo, EnableAutoPitch).ConfigureAwait(false);
                    PlaylistChanged?.Invoke(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// Enable/Disable SVP and madVR if necessary.
        /// </summary>
        public void ConfigurePlayer() {
            if (settings.Data.MediaPlayerApp == MediaPlayerEnum.MpcHc)
                config.AutoConfigure(CurrentVideo);
        }

        /// <summary>
        /// Reloads the Media object in CurrentVideo from the database.
        /// </summary>
        public void ReloadVideoInfo() {
            if (player.CurrentVideo != null) {
                player.CurrentVideo = playerAccess.GetVideoById(player.CurrentVideo.MediaId);
                NowPlaying?.Invoke(this, new NowPlayingEventArgs(true));
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
            List<RatingCategory> DbResult = await Task.Run(() => searchAccess.GetCustomRatingCategories());
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
        public DownloadMediaInfo GetNextVideoDownloading() {
            if (nextVideo != null)
                return download.GetActiveDownloadByMediaId(nextVideo.MediaId) as DownloadMediaInfo;
            return null;
        }

        /// <summary>
        /// Uses specified player business object to play the session.
        /// </summary>
        /// <param name="player">The player business object through which to play the session.</param>
        public void SetPlayer(IMediaPlayer player) {
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

        /// <summary>
        /// Sets the player mode to Normal.
        /// </summary>
        public void SetPlayerModeNormal() {
            playMode = PlayerMode.Normal;
        }

        #endregion

    }
}

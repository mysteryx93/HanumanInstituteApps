using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    #region Interface

    public interface IMpvPlayer : IMediaPlayer { }

    #endregion

    public class MpvPlayer : IMpvPlayer, IMediaPlayer {

        #region Declarations / Constructor

        private string customFileName;
        public bool IsVisible;
        private DateTime lastStartTime;
        private double restorePosition;
        private bool timerGetPositionEnabled = false; // GetPosition is always being sent, but sets whether to listen to the response.
        private DispatcherTimer timerGetPosition;
        private DispatcherTimer timerPlayTimeout;
        private bool isInitialized = false;

        public event EventHandler Closed;

        protected readonly ISettings settings;
        private IMediaPlayerUI playerUI;
        private IAutoPitchBusiness autoPitch;

        public MpvPlayer(ISettings settings, IMediaPlayerUI mediaPlayerUI, IAutoPitchBusiness autoPitchBusiness) {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.playerUI = mediaPlayerUI ?? throw new ArgumentNullException(nameof(mediaPlayerUI));
            this.autoPitch = autoPitchBusiness ?? throw new ArgumentNullException(nameof(autoPitchBusiness));
        }

        #endregion

        #region IMediaPlayer

        public bool IsAutoPitchEnabled { get; set; }

        private bool allowClose = false;

        public bool AllowClose {
            get => allowClose;
            set {
                allowClose = value;
                if (playerUI != null)
                    playerUI.AllowClose = value;
            }
        }

        public bool IsPlaying { get; private set; }

        public bool IsAvailable => true;

        /// <summary>
        /// Gets or sets whether to ignore start/end positions.
        /// </summary>
        public bool IgnorePos { get; set; }

        /// <summary>
        /// Returns the current video's start position.
        /// </summary>
        public double? StartPos {
            get {
                if (CurrentVideo != null && CurrentVideo.StartPos != null) {
                    if (settings.Data.ChangeAudioPitch) {
                        return CurrentVideo.StartPos.Value + .5; // Added half-second to fill buffer
                    } else
                        return CurrentVideo.StartPos.Value;
                } else
                    return null;
            }
        }

        /// <summary>
        /// Returns the current video's end position.
        /// </summary>
        public double? EndPos {
            get {
                if (CurrentVideo != null && CurrentVideo.EndPos != null) {
                    if (settings.Data.ChangeAudioPitch) {
                        return CurrentVideo.EndPos.Value + .5; // Added half-second to fill buffer
                    } else
                        return CurrentVideo.EndPos.Value;

                } else
                    return null;
            }
        }

        public double Position { get; set; }

        public Media CurrentVideo { get; set; }

        public event EventHandler NowPlaying;
        public event EventHandler PositionChanged;
        public event EventHandler PlayNext;
        public event EventHandler Pause;
        public event EventHandler Resume;

        public async Task PlayVideoAsync(Media video, bool enableAutoPitch) {
            this.CurrentVideo = video;
            this.IsAutoPitchEnabled = enableAutoPitch;
            timerGetPositionEnabled = false;
            Position = 0;
            restorePosition = 0;
            lastStartTime = DateTime.Now;
            if (playerUI == null)
                Show();
            timerGetPositionEnabled = false;
            await playerUI.OpenFileAsync(MediaFileName);
            // Ensures timerGetPositionEnabled gets re-activated even if play failed, after 5 seconds.
            timerPlayTimeout.Stop();
            timerPlayTimeout.Start();
        }

        /// <summary>
        /// Plays specified video file. To use only when playing files outside the Natural Grounding folder.
        /// </summary>
        /// <param name="filePath">The absolute path of the file to play.</param>
        public async Task PlayVideoAsync(string filePath) {
            CurrentVideo = new Media() { };
            IsAutoPitchEnabled = false;
            customFileName = filePath;
            timerGetPositionEnabled = false;
            Position = 0;
            restorePosition = 0;
            lastStartTime = DateTime.Now;
            await playerUI.OpenFileAsync(filePath);
            // If video doesn't load after 5 seconds, send the play command again.
            timerPlayTimeout.Stop();
            timerPlayTimeout.Start();
        }

        public Task SetPositionAsync(double pos) {
            playerUI.Position = pos;
            return null;
        }

        public void Show() {
            Initialize();
            playerUI.Show();
            if (CurrentVideo != null) {
                timerGetPosition.Start();
                timerGetPositionEnabled = true;
            }
        }

        public void Hide() {
            timerGetPositionEnabled = false;
            timerGetPosition.Stop();
            playerUI?.Hide();
        }

        public void Close() {
            timerGetPositionEnabled = false;
            timerGetPosition.Stop();
            playerUI?.Close();
            Terminate();
        }

        public void SetPath() { }

        #endregion

        #region Events

        /// <summary>
        /// Raise the Resume event.
        /// </summary>
        private void player_MediaPause(object sender, EventArgs e) {
            IsPlaying = false;
            Pause?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raise the Resume event.
        /// </summary>
        private void player_MediaResume(object sender, EventArgs e) {
            IsPlaying = true;
            Resume?.Invoke(this, new EventArgs());
        }

        private void player_MediaOpened(object sender, EventArgs e) {
            timerGetPosition.Start();
            timerGetPositionEnabled = true;

            try {
                CurrentVideo.Length = (short)playerUI.Duration;
            } catch { }
            Position = 0;

            NowPlaying?.Invoke(this, new EventArgs());
        }

        private void player_Closed(object sender, EventArgs e) {
            Closed?.Invoke(sender, e);
        }

        #endregion

        #region Methods / Properties

        //public void Run() {
        //    // Initializes player in new window.
        //    MediaPlayerWindow NewForm = new MediaPlayerWindow();
        //    NewForm.Player.IsWindow = true;
        //NewForm.Closing += MediaPlayerWindow_Closing;
        //    NewForm.Show();
        //    Run(NewForm.Player);
        //}

        private void Initialize() {
            if (isInitialized)
                return;
            playerUI.MediaOpened += player_MediaOpened;
            playerUI.MediaResume += player_MediaResume;
            playerUI.MediaPause += player_MediaPause;
            playerUI.Closed += player_Closed;

            IsVisible = true;
            timerGetPosition = new DispatcherTimer();
            timerGetPosition.Interval = TimeSpan.FromSeconds(1);
            timerGetPosition.Tick += timerGetPosition_Tick;
            timerGetPosition.Start();
            timerPlayTimeout = new DispatcherTimer();
            timerPlayTimeout.Interval = TimeSpan.FromSeconds(5);
            timerPlayTimeout.Tick += timerPlayTimeout_Tick;
        }

        private void Terminate() {
            if (!isInitialized)
                return;
            playerUI.MediaOpened -= player_MediaOpened;
            playerUI.MediaResume -= player_MediaResume;
            playerUI.MediaPause -= player_MediaPause;
            playerUI.Closed -= player_Closed;
            timerGetPosition.Stop();
            timerPlayTimeout.Stop();
        }

        private string MediaFileName => customFileName != null ? customFileName : IsAutoPitchEnabled ? autoPitch.LastScriptPath : settings.NaturalGroundingFolder + CurrentVideo.FileName;

        #endregion

        #region Timers

        /// <summary>
        /// Occurs every second. Detects end position, start position or restore position.
        /// </summary>
        private void timerGetPosition_Tick(object sender, EventArgs e) {
            if (timerGetPositionEnabled) {
                if ((EndPos.HasValue && Position > EndPos && !IgnorePos) || Position > CurrentVideo.Length - 1) {
                    // End position reached.
                    if (PlayNext != null) {
                        //timerGetPositionEnabled = false;
                        playerUI.Dispatcher.Invoke(() => PlayNext(this, new EventArgs()));
                    }
                } else if (restorePosition == 0 && StartPos.HasValue && StartPos > 10 && Position < StartPos && !IgnorePos) {
                    // Skip to start position.
                    restorePosition = StartPos.Value;
                }

                if (restorePosition > 0) {
                    // Restore to specified position (usually after a crash).
                    if (restorePosition > 10) {
                        timerGetPositionEnabled = false;
                        Position = restorePosition;
                        playerUI.Position = restorePosition;
                        timerGetPositionEnabled = true;
                    }
                    restorePosition = 0;
                } else
                    TrackPosition();
            }
        }

        /// <summary>
        /// Occurs 5 seconds after the last video started to ensure the player returns into usable state if play failed.
        /// </summary>
        private void timerPlayTimeout_Tick(object sender, EventArgs e) {
            timerPlayTimeout.Stop();
            timerGetPositionEnabled = true;
        }

        private void TrackPosition() {
            Position = playerUI.Position;
            PositionChanged?.Invoke(this, new EventArgs());
        }

        #endregion

    }
}

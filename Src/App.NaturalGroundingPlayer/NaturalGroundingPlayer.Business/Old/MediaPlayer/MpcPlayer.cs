using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Threading;
using MPC_API_LIB;
using HanumanInstitute.NaturalGroundingPlayer.DataAccess;

namespace HanumanInstitute.NaturalGroundingPlayer.Business {

    #region Interface

    public interface IMpcPlayer : IMediaPlayer { }

    #endregion

    /// <summary>
    /// Provides API control over MPC-HC.
    /// </summary>
    /// <remarks>
    /// Make sure to change these MPC-HC settings (View | Options)
    /// - Player, disable "Remember file position"
    /// - Playback, check "Repeat forever"
    /// - If using madVR for enhanced quality, Playback Output, set DirectShow Video to madVR.
    /// </remarks>
    public class MpcPlayer : IMpcPlayer, IMediaPlayer {

        #region Declarations / Constructors

        private bool isInitialized = false;
        private string customFileName;
        private DateTime lastStartTime;
        private string nowPlayingPath;
        private double restorePosition;
        private bool timerGetPositionEnabledInternal = true; // GetPosition is always being sent, but sets whether to listen to the response.
        private DispatcherTimer timerGetPosition;
        private DispatcherTimer timerPlayTimeout;
        private DispatcherTimer timerDisablePosTimeout;

        protected readonly ISettings settings;
        private MPC mpcApi;
        private IMpcProcessWatcher watcher;
        private IAutoPitchBusiness autoPitch;

        public MpcPlayer(ISettings settings, MPC mpcApi, IMpcProcessWatcher watcher, IAutoPitchBusiness autoPitch) {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.mpcApi = mpcApi ?? throw new ArgumentNullException(nameof(mpcApi));
            this.watcher = watcher ?? throw new ArgumentNullException(nameof(watcher));
            this.autoPitch = autoPitch ?? throw new ArgumentNullException(nameof(autoPitch));
        }

        #endregion

        #region IMediaPlayer

        public bool IsAutoPitchEnabled { get; set; }

        public bool AllowClose {
            get => watcher.AllowClose;
            set => watcher.AllowClose = value;
        }

        public bool IsPlaying { get; private set; }

        public bool IsAvailable => TimerGetPositionEnabled;

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
            CurrentVideo = video;
            IsAutoPitchEnabled = enableAutoPitch;
            customFileName = null;
            TimerGetPositionEnabled = false;
            Position = 0;
            restorePosition = 0;
            nowPlayingPath = null;
            lastStartTime = DateTime.Now;
            watcher.EnsureRunning();
            await mpcApi.OpenFileAsync(MediaFileName);
            // If video doesn't load after 5 seconds, send the play command again.
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
            TimerGetPositionEnabled = false;
            Position = 0;
            restorePosition = 0;
            nowPlayingPath = null;
            lastStartTime = DateTime.Now;
            watcher.EnsureRunning();
            await mpcApi.OpenFileAsync(filePath);
            // If video doesn't load after 5 seconds, send the play command again.
            timerPlayTimeout.Stop();
            timerPlayTimeout.Start();
        }

        public async Task SetPositionAsync(double value) {
            TimerGetPositionEnabled = false;
            Position = value;
            await mpcApi.SetPositionAsync((int)value);
            await Task.Delay(1000);
            TimerGetPositionEnabled = true;
        }

        public void Show() {
            Initialize();
            mpcApi.Run(settings.Data.MpcPath);
            watcher.Start();
        }

        public void Hide() {
            watcher.Stop();
            Close();
        }

        public void Close() {
            AllowClose = true;
            TimerGetPositionEnabled = false;
            timerGetPosition.Stop();
            watcher.Stop();
            if (mpcApi.MpcProcess != null) {
                mpcApi.MpcProcess.CloseMainWindow();
                System.Threading.Thread.Sleep(200);
            }
            Terminate();
        }

        public void SetPath() {
            mpcApi.SetPath(settings.Data.MpcPath);
        }

        #endregion

        #region API Events

        /// <summary>
        /// When playing new file, update CurrentVideo.Length and raise NowPlaying event.
        /// </summary>
        /// <param name="msg"></param>
        private void Player_MPC_NowPlaying(MPC.MSGIN msg) {
            timerGetPosition.Start();

            string[] FileInfo = msg.Message.Split('|');
            CurrentVideo.Length = (short)double.Parse(FileInfo[4], CultureInfo.InvariantCulture);

            // In later version of MPC-HC, this event keeps firing repeatedly, so ignore when file path is the same.
            if (!string.IsNullOrEmpty(FileInfo[3]) && nowPlayingPath != FileInfo[3]) {
                nowPlayingPath = FileInfo[3];
                Position = 0;
                // restorePosition = 0;
                timerPlayTimeout.Stop();

                if (NowPlaying != null)
                    NowPlaying(this, new EventArgs());

                TimerGetPositionEnabled = true;
            }
        }

        /// <summary>
        /// Raise PositionChanged event.
        /// </summary>
        private void Player_MPC_CurrentPosition(MPC.MSGIN msg) {
            if (TimerGetPositionEnabled) {
                double PositionValue = 0;
                if (Double.TryParse(msg.Message, NumberStyles.AllowDecimalPoint, System.Globalization.CultureInfo.InvariantCulture, out PositionValue)) {
                    Position = PositionValue;
                    if (PositionChanged != null)
                        PositionChanged(this, new EventArgs());
                }
            }
        }

        /// <summary>
        /// When player is restored after closing, open previous file and restore position.
        /// </summary>
        private async void Player_MPC_Running() {
            if (CurrentVideo != null) {
                TimerGetPositionEnabled = false;
                restorePosition = (int)Position;
                nowPlayingPath = null;

                await Task.Delay(1000);
                await mpcApi.OpenFileAsync(MediaFileName);
            }
        }

        /// <summary>
        /// Raise the Resume event.
        /// </summary>
        private void player_MPC_Play(MPC.MSGIN msg) {
            IsPlaying = true;
            if (Resume != null)
                Resume(this, new EventArgs());
        }

        /// <summary>
        /// Raise the Pause event.
        /// </summary>
        private void player_MPC_Stop(MPC.MSGIN msg) {
            IsPlaying = false;
            if (Pause != null)
                Pause(this, new EventArgs());
        }

        #endregion

        #region Methods / Properties

        private void Initialize() {
            if (isInitialized)
                return;
            mpcApi.MPC_NowPlaying += Player_MPC_NowPlaying;
            mpcApi.MPC_CurrentPosition += Player_MPC_CurrentPosition;
            mpcApi.MPC_Running += Player_MPC_Running;
            mpcApi.MPC_Play += player_MPC_Play;
            mpcApi.MPC_Pause += player_MPC_Stop;
            mpcApi.MPC_Stop += player_MPC_Stop;
            timerGetPosition = new DispatcherTimer();
            timerGetPosition.Interval = TimeSpan.FromSeconds(1);
            timerGetPosition.Tick += timerGetPosition_Tick;
            timerPlayTimeout = new DispatcherTimer();
            timerPlayTimeout.Interval = TimeSpan.FromSeconds(5);
            timerPlayTimeout.Tick += timerPlayTimeout_Tick;
            timerDisablePosTimeout = new DispatcherTimer();
            timerDisablePosTimeout.Interval = TimeSpan.FromSeconds(5);
            timerDisablePosTimeout.Tick += timerDisablePosTimeout_Tick;
            isInitialized = true;
        }

        private void Terminate() {
            if (!isInitialized)
                return;
            watcher.Stop();
            watcher = null;
            mpcApi.MPC_NowPlaying -= Player_MPC_NowPlaying;
            mpcApi.MPC_CurrentPosition -= Player_MPC_CurrentPosition;
            mpcApi.MPC_Running -= Player_MPC_Running;
            mpcApi.MPC_Play -= player_MPC_Play;
            mpcApi.MPC_Pause -= player_MPC_Stop;
            mpcApi.MPC_Stop -= player_MPC_Stop;
            timerGetPosition.Stop();
            timerPlayTimeout.Stop();
            timerDisablePosTimeout.Stop();
            isInitialized = false;
        }

        private string MediaFileName => customFileName != null ? customFileName : IsAutoPitchEnabled ? autoPitch.LastScriptPath : settings.NaturalGroundingFolder + CurrentVideo.FileName;

        /// <summary>
        /// Causes timeGetPosition_Tick to be ignored for the next 5 seconds or until it is re-enabled.
        /// </summary>
        public bool TimerGetPositionEnabled {
            get => timerGetPositionEnabledInternal;
            set {
                timerDisablePosTimeout.Stop();
                // Ensure it doesn't stay disabled for more than 5 seconds.
                if (value == false)
                    timerDisablePosTimeout.Start();
                timerGetPositionEnabledInternal = value;
            }
        }

        #endregion

        #region Timers

        /// <summary>
        /// Occurs every second. Detects end position, start position or restore position.
        /// </summary>
        private async void timerGetPosition_Tick(object sender, EventArgs e) {
            if (TimerGetPositionEnabled) {
                if ((EndPos.HasValue && Position > EndPos && !IgnorePos) || Position > CurrentVideo.Length - 1) {
                    // End position reached.
                    if (PlayNext != null) {
                        PlayNext(this, new EventArgs());
                        return;
                    }
                } else if (restorePosition == 0 && Position < 10 && StartPos.HasValue && StartPos > 10 && Position < StartPos && !IgnorePos) {
                    // Skip to start position.
                    restorePosition = StartPos.Value;
                }

                if (restorePosition > 0) {
                    // Restore to specified position (usually after a crash).
                    if (restorePosition > 10) {
                        TimerGetPositionEnabled = false;
                        Position = restorePosition;
                        restorePosition = 0;

                        await Task.Delay(1000);
                        await mpcApi.SetPositionAsync((int)Position);
                        await Task.Delay(1000);
                        TimerGetPositionEnabled = true;
                    } else
                        restorePosition = 0;
                } else
                    await mpcApi.GetCurrentPositionAsync();
            }
        }

        /// <summary>
        /// Occurs 5 seconds after the last video started to ensure the player returns into usable state if play failed.
        /// </summary>
        private async void timerPlayTimeout_Tick(object sender, EventArgs e) {
            timerPlayTimeout.Stop();
            await mpcApi.OpenFileAsync(MediaFileName);
        }

        /// <summary>
        /// Ensures the position tracking timer isn't disabled for more than 5 seconds.
        /// </summary>
        private void timerDisablePosTimeout_Tick(object sender, EventArgs e) {
            timerDisablePosTimeout.Stop();
            timerGetPositionEnabledInternal = true;
        }

        #endregion

    }
}

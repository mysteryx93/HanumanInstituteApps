using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using MPC_API_LIB;
using DataAccess;
using System.Globalization;

namespace Business {
    /// <summary>
    /// Provides API control over MPC-HC.
    /// </summary>
    /// <remarks>
    /// Make sure to change these MPC-HC settings (View | Options)
    /// - Player, disable "Remember file position"
    /// - Playback, check "Repeat forever"
    /// - If using madVR for enhanced quality, Playback Output, set DirectShow Video to madVR.
    /// </remarks>
    public class MpcPlayerBusiness : Business.IMediaPlayerBusiness {

        #region Declarations / Constructor

        private MPC apiAccess = new MPC();
        private ProcessWatcher watcher;
        public Media CurrentVideo { get; set; }
        private bool isAutoPitchEnabled;
        private string customFileName;
        private double position;
        private DateTime lastStartTime;
        private string nowPlayingPath;
        private int restorePosition;
        private bool isPlaying;
        private bool timerGetPositionEnabledInternal = true; // GetPosition is always being sent, but sets whether to listen to the response.
        private DispatcherTimer timerGetPosition;
        private DispatcherTimer timerPlayTimeout;
        private DispatcherTimer timerDisablePosTimeout;
        /// <summary>
        /// Gets or sets whether to ignore start/end positions.
        /// </summary>
        public bool IgnorePos { get; set; }


        public event EventHandler NowPlaying;
        public event EventHandler PositionChanged;
        public event EventHandler PlayNext;
        public event EventHandler Pause;
        public event EventHandler Resume;

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// Initializes a new instance of the MpcPlayer class.
        /// </summary>
        public MpcPlayerBusiness() {
            watcher = new ProcessWatcher(apiAccess);
            apiAccess.MPC_NowPlaying += Player_MPC_NowPlaying;
            apiAccess.MPC_CurrentPosition += Player_MPC_CurrentPosition;
            apiAccess.MPC_Running += Player_MPC_Running;
            apiAccess.MPC_Play += player_MPC_Play;
            apiAccess.MPC_Pause += player_MPC_Stop;
            apiAccess.MPC_Stop += player_MPC_Stop;
            timerGetPosition = new DispatcherTimer();
            timerGetPosition.Interval = TimeSpan.FromSeconds(1);
            timerGetPosition.Tick += timerGetPosition_Tick;
            timerPlayTimeout = new DispatcherTimer();
            timerPlayTimeout.Interval = TimeSpan.FromSeconds(5);
            timerPlayTimeout.Tick += timerPlayTimeout_Tick;
            timerDisablePosTimeout = new DispatcherTimer();
            timerDisablePosTimeout.Interval = TimeSpan.FromSeconds(5);
            timerDisablePosTimeout.Tick += timerDisablePosTimeout_Tick;
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
                position = 0;
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
                    position = PositionValue;
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
                restorePosition = (int)position;
                nowPlayingPath = null;

                await Task.Delay(1000);
                await apiAccess.OpenFileAsync(MediaFileName);
            }
        }

        /// <summary>
        /// Raise the Resume event.
        /// </summary>
        private void player_MPC_Play(MPC.MSGIN msg) {
            isPlaying = true;
            if (Resume != null)
                Resume(this, new EventArgs());
        }

        /// <summary>
        /// Raise the Pause event.
        /// </summary>
        private void player_MPC_Stop(MPC.MSGIN msg) {
            isPlaying = false;
            if (Pause != null)
                Pause(this, new EventArgs());
        }

        #endregion

        #region Methods / Properties

        public void Show() {
            apiAccess.Run(MpcConfigBusiness.MpcPath());
            watcher.Start();
        }

        public void SetPath() {
            apiAccess.SetPath(MpcConfigBusiness.MpcPath());
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
            if (apiAccess.MpcProcess != null) {
                apiAccess.MpcProcess.CloseMainWindow();
                System.Threading.Thread.Sleep(200);
            }
        }

        public async Task PlayVideoAsync(Media video, bool enableAutoPitch) {
            CurrentVideo = video;
            isAutoPitchEnabled = enableAutoPitch;
            customFileName = null;
            TimerGetPositionEnabled = false;
            position = 0;
            nowPlayingPath = null;
            lastStartTime = DateTime.Now;
            watcher.EnsureRunning();
            await apiAccess.OpenFileAsync(MediaFileName);
            // If video doesn't load after 5 seconds, send the play command again.
            timerPlayTimeout.Stop();
            timerPlayTimeout.Start();
        }

        /// <summary>
        /// Plays specified video file. To use only when playing files outside the Natural Grounding folder.
        /// </summary>
        /// <param name="fileName">The path of the file to play.</param>
        public async Task PlayVideoAsync(string fileName) {
            CurrentVideo = new Media() { };
            isAutoPitchEnabled = false;
            customFileName = fileName;
            TimerGetPositionEnabled = false;
            position = 0;
            nowPlayingPath = null;
            lastStartTime = DateTime.Now;
            watcher.EnsureRunning();
            await apiAccess.OpenFileAsync(fileName);
            // If video doesn't load after 5 seconds, send the play command again.
            timerPlayTimeout.Stop();
            timerPlayTimeout.Start();
        }

        private string MediaFileName {
            get {
                return customFileName != null ? customFileName : isAutoPitchEnabled ? Settings.AutoPitchFile : Settings.NaturalGroundingFolder + CurrentVideo.FileName;
            }
        }

        public double Position {
            get { return position; }
        }

        public async Task SetPositionAsync(double value) {
            TimerGetPositionEnabled = false;
            position = value;
            await apiAccess.SetPositionAsync((int)value);
            await Task.Delay(1000);
            TimerGetPositionEnabled = true;
        }

        public bool IsPlaying {
            get { return isPlaying; }
        }

        public bool IsAvailable {
            get { return TimerGetPositionEnabled; }
        }

        public bool AllowClose {
            get { return watcher.AllowClose; }
            set { watcher.AllowClose = value; }
        }

        /// <summary>
        /// Causes timeGetPosition_Tick to be ignored for the next 5 seconds or until it is re-enabled.
        /// </summary>
        public bool TimerGetPositionEnabled {
            get {
                return timerGetPositionEnabledInternal;
            }
            set {
                timerDisablePosTimeout.Stop();
                // Ensure it doesn't stay disabled for more than 5 seconds.
                if (value == false)
                    timerDisablePosTimeout.Start();
                timerGetPositionEnabledInternal = value;
            }
        }

        #endregion

        /// <summary>
        /// Occurs every second. Detects end position, start position or restore position.
        /// </summary>
        private async void timerGetPosition_Tick(object sender, EventArgs e) {
            if (TimerGetPositionEnabled) {
                if ((CurrentVideo.EndPos.HasValue && position > CurrentVideo.EndPos && !IgnorePos) || position > CurrentVideo.Length - 1) {
                    // End position reached.
                    if (PlayNext != null) {
                        PlayNext(this, new EventArgs());
                        return;
                    }
                } else if (restorePosition == 0 && position < 10 && CurrentVideo.StartPos.HasValue && CurrentVideo.StartPos > 10 && position < CurrentVideo.StartPos && !IgnorePos) {
                    // Skip to start position.
                    restorePosition = CurrentVideo.StartPos.Value;
                }

                if (restorePosition > 0) {
                    // Restore to specified position (usually after a crash).
                    if (restorePosition > 10) {
                        TimerGetPositionEnabled = false;
                        position = restorePosition;
                        restorePosition = 0;

                        await Task.Delay(1000);
                        await apiAccess.SetPositionAsync((int)position);
                        await Task.Delay(1000);
                        TimerGetPositionEnabled = true;
                    } else
                        restorePosition = 0;
                } else
                    await apiAccess.GetCurrentPositionAsync();
            }
        }

        /// <summary>
        /// Occurs 5 seconds after the last video started to ensure the player returns into usable state if play failed.
        /// </summary>
        private async void timerPlayTimeout_Tick(object sender, EventArgs e) {
            timerPlayTimeout.Stop();
            await apiAccess.OpenFileAsync(MediaFileName);
        }

        /// <summary>
        /// Ensures the position tracking timer isn't disabled for more than 5 seconds.
        /// </summary>
        private void timerDisablePosTimeout_Tick(object sender, EventArgs e) {
            timerDisablePosTimeout.Stop();
            timerGetPositionEnabledInternal = true;
        }
    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DataAccess;
using MediaPlayer;

namespace Business {
    public class WmpPlayerBusiness : Business.IMediaPlayerBusiness {

        #region Declarations / Constructor

        public IMediaPlayerControl player;
        public Media CurrentVideo { get; set; }
        public bool IsAutoPitchEnabled { get; set; }
        public bool IsVisible;
        private double position;
        private DateTime lastStartTime;
        private int restorePosition;
        private bool isPlaying;
        private bool timerGetPositionEnabled = false; // GetPosition is always being sent, but sets whether to listen to the response.
        private bool allowClose = false;
        private DispatcherTimer timerGetPosition;
        private DispatcherTimer timerPlayTimeout;
        /// <summary>
        /// Gets or sets whether to ignore start/end positions.
        /// </summary>
        public bool IgnorePos { get; set; }

        public event EventHandler NowPlaying;
        public event EventHandler PositionChanged;
        public event EventHandler PlayNext;
        public event EventHandler Pause;
        public event EventHandler Resume;
        public event EventHandler Closed;

        /// <summary>
        /// Initializes a new instance of the MpcPlayer class.
        /// </summary>
        public WmpPlayerBusiness(IMediaPlayerControl player) {
            this.player = player;
            player.MediaOpened += player_MediaOpened;
            player.MediaResume += player_MediaResume;
            player.MediaPause += player_MediaPause;
            player.Closed += player_Closed;

            IsVisible = true;
            timerGetPosition = new DispatcherTimer();
            timerGetPosition.Interval = TimeSpan.FromSeconds(1);
            timerGetPosition.Tick += timerGetPosition_Tick;
            timerGetPosition.Start();
            timerPlayTimeout = new DispatcherTimer();
            timerPlayTimeout.Interval = TimeSpan.FromSeconds(5);
            timerPlayTimeout.Tick += timerPlayTimeout_Tick;
        }

        /// <summary>
        /// Raise the Resume event.
        /// </summary>
        private void player_MediaPause(object sender, EventArgs e) {
            isPlaying = false;
            if (Pause != null)
                Pause(this, new EventArgs());
        }

        /// <summary>
        /// Raise the Resume event.
        /// </summary>
        private void player_MediaResume(object sender, EventArgs e) {
            isPlaying = true;
            if (Resume != null)
                Resume(this, new EventArgs());
        }
        
        private void player_MediaOpened(object sender, EventArgs e) {
            timerGetPosition.Start();
            timerGetPositionEnabled = true;

            try {
                CurrentVideo.Length = (short)player.Duration;
            }
            catch { }
            position = 0;

            if (NowPlaying != null)
                NowPlaying(this, new EventArgs());
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

        public void Show() {
            player.Show();
            if (CurrentVideo != null) {
                timerGetPosition.Start();
                timerGetPositionEnabled = true;
            }
        }

        public void SetPath() {
        }

        public void Hide() {
            timerGetPositionEnabled = false;
            timerGetPosition.Stop();
            if (player != null)
                player.Hide();
        }

        public void Close() {
            timerGetPositionEnabled = false;
            timerGetPosition.Stop();
            if (player != null)
                player.Close();
        }

        public async Task PlayVideoAsync(Media video, bool enableAutoPitch) {
            this.CurrentVideo = video;
            this.IsAutoPitchEnabled = enableAutoPitch;
            timerGetPositionEnabled = false;
            position = 0;
            lastStartTime = DateTime.Now;
            if (player == null)
                Show();
            timerGetPositionEnabled = false;
            await player.OpenFileAsync(MediaFileName);
            // Ensures timerGetPositionEnabled gets re-activated even if play failed, after 5 seconds.
            timerPlayTimeout.Stop();
            timerPlayTimeout.Start();
        }

        private string MediaFileName {
            get {
                // customFileName != null ? customFileName : 
                return IsAutoPitchEnabled ? Settings.AutoPitchFile : Settings.NaturalGroundingFolder + CurrentVideo.FileName;
            }
        }

        public double Position {
            get { return position; }
        }

        public Task SetPositionAsync(double pos) {
            player.Position = pos;
            return null;
        }

        public bool IsPlaying {
            get { return isPlaying; }
        }

        public bool IsAvailable {
            get { return true; }
        }

        public bool AllowClose {
            get { return allowClose; }
            set {
                allowClose = value;
                if (player != null)
                    player.AllowClose = value;
            }
        }

        #endregion

        #region Events

        private void player_Closed(object sender, EventArgs e) {
            if (Closed != null)
                Closed(sender, e);
        }

        /// <summary>
        /// Occurs every second. Detects end position, start position or restore position.
        /// </summary>
        private void timerGetPosition_Tick(object sender, EventArgs e) {
            if (timerGetPositionEnabled) {
                if ((EndPos.HasValue && position > EndPos && !IgnorePos) || position > CurrentVideo.Length - 1) {
                    // End position reached.
                    if (PlayNext != null) {
                        //timerGetPositionEnabled = false;
                        player.Dispatcher.Invoke(() => PlayNext(this, new EventArgs()));
                    }
                } else if (restorePosition == 0 && StartPos.HasValue && StartPos > 10 && position < StartPos && !IgnorePos) {
                    // Skip to start position.
                    restorePosition = StartPos.Value;
                }

                if (restorePosition > 0) {
                    // Restore to specified position (usually after a crash).
                    if (restorePosition > 10) {
                        timerGetPositionEnabled = false;
                        position = restorePosition;
                        player.Position = restorePosition;
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
            position = player.Position;
            if (PositionChanged != null)
                PositionChanged(this, new EventArgs());
        }

        #endregion


        /// <summary>
        /// Returns the current video's start position, taking into account the slowdown when playing at 432hz.
        /// </summary>
        public short? StartPos {
            get {
                if (CurrentVideo != null && CurrentVideo.StartPos != null)
                    return (short)((double)CurrentVideo.StartPos.Value * 440 / 432);
                else
                    return null;
            }
        }

        /// <summary>
        /// Returns the current video's end position, taking into account the slowdown when playing at 432hz.
        /// </summary>
        public short? EndPos {
            get {
                if (CurrentVideo != null && CurrentVideo.EndPos != null)
                    return (short)((double)CurrentVideo.EndPos.Value * 440 / 432);
                else
                    return null;
            }
        }
    }
}

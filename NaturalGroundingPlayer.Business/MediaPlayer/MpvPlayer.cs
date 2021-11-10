using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using HanumanInstitute.NaturalGroundingPlayer.Configuration;
using HanumanInstitute.NaturalGroundingPlayer.Models;

namespace HanumanInstitute.NaturalGroundingPlayer.MediaPlayer
{
    public class MpvPlayer : IMediaPlayer
    {
        private string customFileName;
        public bool IsVisible;
        private DateTime lastStartTime;
        private double restorePosition;
        private bool timerGetPositionEnabled = false; // GetPosition is always being sent, but sets whether to listen to the response.
        private DispatcherTimer timerGetPosition;
        private DispatcherTimer timerPlayTimeout;
        private readonly bool isInitialized = false;

        public event EventHandler Closed;

        private readonly IAppSettingsProvider _settings;
        private readonly IMediaPlayerUI _playerUI;

        public MpvPlayer(IAppSettingsProvider settings, IMediaPlayerUI mediaPlayerUI)
        {
            _settings = settings;
            _playerUI = mediaPlayerUI;
        }

        public bool IsAutoPitchEnabled { get; set; }

        private bool _allowClose;

        public bool AllowClose
        {
            get => _allowClose;
            set
            {
                _allowClose = value;
                if (_playerUI != null)
                {
                    _playerUI.AllowClose = value;
                }
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
        public double? StartPos
        {
            get
            {
                if (CurrentVideo != null && CurrentVideo.StartPos != null)
                {
                    if (_settings.Value.ChangeAudioPitch)
                    {
                        return CurrentVideo.StartPos.Value + .5; // Added half-second to fill buffer
                    }
                    else
                    {
                        return CurrentVideo.StartPos.Value;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns the current video's end position.
        /// </summary>
        public double? EndPos
        {
            get
            {
                if (CurrentVideo != null && CurrentVideo.EndPos != null)
                {
                    if (_settings.Value.ChangeAudioPitch)
                    {
                        return CurrentVideo.EndPos.Value + .5; // Added half-second to fill buffer
                    }
                    else
                    {
                        return CurrentVideo.EndPos.Value;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public double Position { get; set; }

        public Media CurrentVideo { get; set; }

        public event EventHandler NowPlaying;
        public event EventHandler PositionChanged;
        public event EventHandler PlayNext;
        public event EventHandler Pause;
        public event EventHandler Resume;

        public async Task PlayVideoAsync(Media video, bool enableAutoPitch)
        {
            CurrentVideo = video;
            IsAutoPitchEnabled = enableAutoPitch;
            timerGetPositionEnabled = false;
            Position = 0;
            restorePosition = 0;
            lastStartTime = DateTime.Now;
            if (_playerUI == null)
            {
                Show();
            }

            timerGetPositionEnabled = false;
            await _playerUI.OpenFileAsync(MediaFileName).ConfigureAwait(false);
            // Ensures timerGetPositionEnabled gets re-activated even if play failed, after 5 seconds.
            timerPlayTimeout.Stop();
            timerPlayTimeout.Start();
        }

        /// <summary>
        /// Plays specified video file. To use only when playing files outside the Natural Grounding folder.
        /// </summary>
        /// <param name="filePath">The absolute path of the file to play.</param>
        public async Task PlayVideoAsync(string filePath)
        {
            CurrentVideo = new Media() { };
            IsAutoPitchEnabled = false;
            customFileName = filePath;
            timerGetPositionEnabled = false;
            Position = 0;
            restorePosition = 0;
            lastStartTime = DateTime.Now;
            await _playerUI.OpenFileAsync(filePath).ConfigureAwait(false);
            // If video doesn't load after 5 seconds, send the play command again.
            timerPlayTimeout.Stop();
            timerPlayTimeout.Start();
        }

        public Task SetPositionAsync(double pos)
        {
            _playerUI.Position = pos;
            return Task.CompletedTask;
        }

        public void Show()
        {
            Initialize();
            _playerUI.Show();
            if (CurrentVideo != null)
            {
                timerGetPosition.Start();
                timerGetPositionEnabled = true;
            }
        }

        public void Hide()
        {
            timerGetPositionEnabled = false;
            timerGetPosition.Stop();
            _playerUI?.Hide();
        }

        public void Close()
        {
            timerGetPositionEnabled = false;
            timerGetPosition.Stop();
            _playerUI?.Close();
            Terminate();
        }

        public void SetPath() { }

        /// <summary>
        /// Raise the Resume event.
        /// </summary>
        private void player_MediaPause(object? sender, EventArgs e)
        {
            IsPlaying = false;
            Pause?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raise the Resume event.
        /// </summary>
        private void player_MediaResume(object? sender, EventArgs e)
        {
            IsPlaying = true;
            Resume?.Invoke(this, new EventArgs());
        }

        private void player_MediaOpened(object? sender, EventArgs e)
        {
            timerGetPosition.Start();
            timerGetPositionEnabled = true;

            try
            {
                CurrentVideo.Length = (short)_playerUI.Duration;
            }
            catch { }
            Position = 0;

            NowPlaying?.Invoke(this, new EventArgs());
        }

        private void player_Closed(object? sender, EventArgs e)
        {
            Closed?.Invoke(sender, e);
        }

        //public void Run() {
        //    // Initializes player in new window.
        //    MediaPlayerWindow NewForm = new MediaPlayerWindow();
        //    NewForm.Player.IsWindow = true;
        //NewForm.Closing += MediaPlayerWindow_Closing;
        //    NewForm.Show();
        //    Run(NewForm.Player);
        //}

        private void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            _playerUI.MediaOpened += player_MediaOpened;
            _playerUI.MediaResume += player_MediaResume;
            _playerUI.MediaPause += player_MediaPause;
            _playerUI.Closed += player_Closed;

            IsVisible = true;
            timerGetPosition = new DispatcherTimer();
            timerGetPosition.Interval = TimeSpan.FromSeconds(1);
            timerGetPosition.Tick += timerGetPosition_Tick;
            timerGetPosition.Start();
            timerPlayTimeout = new DispatcherTimer();
            timerPlayTimeout.Interval = TimeSpan.FromSeconds(5);
            timerPlayTimeout.Tick += timerPlayTimeout_Tick;
        }

        private void Terminate()
        {
            if (!isInitialized)
            {
                return;
            }

            _playerUI.MediaOpened -= player_MediaOpened;
            _playerUI.MediaResume -= player_MediaResume;
            _playerUI.MediaPause -= player_MediaPause;
            _playerUI.Closed -= player_Closed;
            timerGetPosition.Stop();
            timerPlayTimeout.Stop();
        }

        private string MediaFileName => customFileName != null ? customFileName : IsAutoPitchEnabled ? autoPitch.LastScriptPath : _settings.NaturalGroundingFolder + CurrentVideo.FileName;

        /// <summary>
        /// Occurs every second. Detects end position, start position or restore position.
        /// </summary>
        private void timerGetPosition_Tick(object? sender, EventArgs e)
        {
            if (timerGetPositionEnabled)
            {
                if ((EndPos.HasValue && Position > EndPos && !IgnorePos) || Position > CurrentVideo.Length - 1)
                {
                    // End position reached.
                    if (PlayNext != null)
                    {
                        //timerGetPositionEnabled = false;
                        _playerUI.Dispatcher.Invoke(() => PlayNext(this, new EventArgs()));
                    }
                }
                else if (restorePosition == 0 && StartPos.HasValue && StartPos > 10 && Position < StartPos && !IgnorePos)
                {
                    // Skip to start position.
                    restorePosition = StartPos.Value;
                }

                if (restorePosition > 0)
                {
                    // Restore to specified position (usually after a crash).
                    if (restorePosition > 10)
                    {
                        timerGetPositionEnabled = false;
                        Position = restorePosition;
                        _playerUI.Position = restorePosition;
                        timerGetPositionEnabled = true;
                    }
                    restorePosition = 0;
                }
                else
                {
                    TrackPosition();
                }
            }
        }

        /// <summary>
        /// Occurs 5 seconds after the last video started to ensure the player returns into usable state if play failed.
        /// </summary>
        private void timerPlayTimeout_Tick(object? sender, EventArgs e)
        {
            timerPlayTimeout.Stop();
            timerGetPositionEnabled = true;
        }

        private void TrackPosition()
        {
            Position = _playerUI.Position;
            PositionChanged?.Invoke(this, new EventArgs());
        }
    }
}

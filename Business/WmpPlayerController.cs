using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using EmergenceGuardian.MediaPlayerUI;
using EmergenceGuardian.MpvPlayerUI;

namespace Business {
    public class WmpPlayerController : IMediaPlayerControl {
        public MpvMediaPlayer Player { get; private set; }
        public Window PlayerWindow { get; private set; }
        private PropertyChangeNotifier IsPlayingNotifier;

        public WmpPlayerController(MpvMediaPlayer player) : this(player, null) { }

        public WmpPlayerController(MpvMediaPlayer player, Window playerWindow) {
            this.Player = player;
            this.PlayerWindow = playerWindow;

            player.MediaPlayerInitialized += (o, e) => {
                Loop = true;

                Player.Host.OnMediaLoaded += Player_MediaOpened;
                if (playerWindow != null)
                    playerWindow.Closing += Window_Closing;
                IsPlayingNotifier = new PropertyChangeNotifier(player.Host, MpvMediaPlayerHost.IsPlayingProperty);
                IsPlayingNotifier.ValueChanged += IsPlayingNotifier_ValueChanged;
                //Player.LostFocus += Player_LostFocus;
            };
        }

        public bool AllowClose { get; set; } = true;
        private bool forceClose;

        public bool Loop {
            get => Player.Host.Loop;
            set => Player.Host.Loop = value;
        }
        public double Position {
            get => Player.Host != null ? Player.Host.Position.TotalSeconds : 0;
            set => Player.Host.Position = TimeSpan.FromSeconds(value);
        }

        public double Duration => Player.Host.Duration.TotalSeconds;

        public Dispatcher Dispatcher => Player.Dispatcher;

        public event EventHandler MediaOpened;
        public event EventHandler MediaResume;
        public event EventHandler MediaPause;
        public event EventHandler Closed;

        public void Player_MediaOpened(object sender, EventArgs e) {
            if (PlayerWindow?.IsVisible == true)
                PlayerWindow.Show();
            MediaOpened?.Invoke(this, new EventArgs());
        }

        private void IsPlayingNotifier_ValueChanged(object sender, EventArgs e) {
            if (Player.Host.IsPlaying)
                MediaResume?.Invoke(this, new EventArgs());
            else
                MediaPause?.Invoke(this, new EventArgs());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!forceClose) {
                if (!AllowClose)
                    e.Cancel = true;
                else {
                    Player.Host.Stop();
                    Player.Host.Source = null;
                    PlayerWindow?.Hide();
                    e.Cancel = true;
                    Closed?.Invoke(this, new EventArgs());
                }
            }
        }

        public void Show() {
            PlayerWindow?.Show();
            if (Player.Host != null)
                Player.Host.IsPlaying = true;
        }

        public void Hide() {
            if (Player.Host != null)
                Player.Host.IsPlaying = false;
            PlayerWindow?.Hide();
        }

        public void Close() {
            Player.Host.Stop();
            AllowClose = true;
            forceClose = true;
            PlayerWindow?.Close();
        }

        public Task OpenFileAsync(string fileName) {
            Dispatcher.Invoke(() => {
                Show();
                if (Player.Host != null) {
                    Player.Host.Source = null;
                    Player.Host.Source = fileName;
                } else
                    Player.MediaPlayerInitialized += (o, e) => Player.Host.Source = fileName;
            });
            return Task.CompletedTask;
        }
    }
}

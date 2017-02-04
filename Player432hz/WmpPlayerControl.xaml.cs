using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Business;

namespace Player432hz {
    /// <summary>
    /// Interaction logic for MediaPlayerControl.xaml
    /// </summary>
    public partial class WmpPlayerControl : UserControl, IMediaPlayerControl {
        public bool AllowClose { get; set; }
        public bool IsWindow { get; set; }
        private bool forceClose;
        public event EventHandler MediaOpened;
        public event EventHandler MediaResume;
        public event EventHandler MediaPause;
        public event EventHandler Closed;

        public WmpPlayerControl() {
            InitializeComponent();
            Player.Loop = true;
            AllowClose = true;
            Player.MediaOpened += Player_MediaOpened;
            Player.MediaResume += Player_MediaResume;
            Player.MediaPause += Player_MediaPause;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            if (IsWindow) {
                Window.GetWindow(this).Closing += Window_Closing;
            }
        }

        public void Player_MediaOpened(object sender, EventArgs e) {
            if (IsWindow && !Window.GetWindow(this).IsVisible)
                Window.GetWindow(this).Show();

            if (MediaOpened != null)
                MediaOpened(this, new EventArgs());
        }

        public void Player_MediaResume(object sender, EventArgs e) {
            if (MediaResume != null)
                MediaResume(this, new EventArgs());
        }

        public void Player_MediaPause(object sender, EventArgs e) {
            if (MediaPause != null)
                MediaPause(this, new EventArgs());
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!forceClose) {
                if (!AllowClose)
                    e.Cancel = true;
                else {
                    Player.Stop();
                    Player.Source = null;
                    Window.GetWindow(this).Hide();
                    e.Cancel = true;
                    if (Closed != null)
                        Closed(this, new EventArgs());
                }
            }
        }

        public double Position {
            get {
                return Player.Position;
            }
            set {
                Player.Position = value;
            }
        }

        public double Duration {
            get {
                return Player.Duration;
            }
        }

        public async Task OpenFileAsync(string fileName) {
            await Task.Run(() => {
                Player.Source = fileName;
                Player.Play();
            });
        }

        public void Show() {
            if (IsWindow) {
                Window.GetWindow(this).Show();
                Player.Play();
            }
        }

        public void Hide() {
            if (IsWindow) {
                Player.Pause();
                Window.GetWindow(this).Hide();
            }
        }

        public void Close() {
            Player.Stop();
            Player.Dispose();
            AllowClose = true;
            forceClose = true;
            if (IsWindow)
                Window.GetWindow(this).Close();
        }

        public bool Loop {
            get {
                return Player.Loop;
            }
            set {
                Player.Loop = value;
            }
        }
    }
}

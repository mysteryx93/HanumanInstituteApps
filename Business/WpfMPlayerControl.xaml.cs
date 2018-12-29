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
using System.Windows.Threading;

namespace Business {
    /// <summary>
    /// Interaction logic for WpfMPlayerControl.xaml
    /// </summary>
    public partial class WpfMPlayerControl : UserControl {
        public MPlayer Player;
        public string MediaPath { get; set; }

        public bool AllowClose { get; set; }
        public bool IsWindow { get; set; }
        private bool forceClose;
        //private DispatcherTimer reloadTimer { get; set; } // Adds a delay after changing settings before reloading video
        //private double newRate;
        public event EventHandler MediaOpened;
        public event EventHandler MediaResume;
        public event EventHandler MediaPause;
        public event EventHandler MediaStopped;
        public event MplayerEventHandler MediaClosed;
        public event MplayerEventHandler PositionChanged;
        public event EventHandler Closed;
        public bool isSeekBarButtonDown = false;

        public WpfMPlayerControl() {
            InitializeComponent();

            int handle = (int)Host.Handle;
            Player = new MPlayer(handle, MplayerBackends.Direct3D, PathManager.MPlayerPath, false, TimeSpan.FromSeconds(1));
            Player.MediaStarted += Player_MediaStarted;
            Player.MediaClosed += Player_MediaClosed;
            Player.CurrentPosition += Player_CurrentPosition;

            // Player.Loop = true;
            AllowClose = true;
            Player.MediaStarted += Player_MediaOpened;
            //Player.MediaResume += Player_MediaResume;
            //Player.MediaPause += Player_MediaPause;

            SetControlsEnabled(false);

            //reloadTimer = new DispatcherTimer();
            //reloadTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            //reloadTimer.Tick += ReloadTimer_Tick;
        }
        
        public bool IsVideoVisible {
            get {
                return Host.Visibility == Visibility.Visible;
            }
            set {
                Host.Visibility = value ? Visibility.Visible : Visibility.Hidden;
            }
        }

        private void Player_MediaStarted(object sender, EventArgs e) {
            MediaOpened?.Invoke(this, e);
            //Dispatcher.Invoke(() => {
            //    Player.Volume((int)GetValue(VolumeProperty));
            //});
        }

        private void Player_CurrentPosition(object sender, MplayerEvent e) {
            if (!isSeekBarButtonDown) {
                Dispatcher.Invoke(() => {
                    SeekBar.Value = e.Value;
                });
            }
            PositionChanged?.Invoke(this, e);
        }

        private void Player_MediaClosed(object sender, MplayerEvent e) {
            Dispatcher.Invoke(() => {
                SeekBar.Value = 0;
                SeekBar.Maximum = 1;
                SetControlsEnabled(false);
                MediaClosed?.Invoke(this, e);
            });
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            if (IsWindow) {
                Window.GetWindow(this).Closing += Window_Closing;
            }
        }

        public void Player_MediaOpened(object sender, EventArgs e) {
            Dispatcher.Invoke(() => {
                if (IsWindow && !Window.GetWindow(this).IsVisible)
                    Window.GetWindow(this).Show();

                SeekBar.Maximum = Duration;
                SetControlsEnabled(true);
                MediaOpened?.Invoke(this, new EventArgs());
            });
        }

        public void SetControlsEnabled(bool value) {
            SeekBar.IsEnabled = value;
            PauseButton.IsEnabled = value;
            StopButton.IsEnabled = value;
        }
            
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (!forceClose) {
                if (!AllowClose)
                    e.Cancel = true;
                else {
                    //Player.Stop();
                    Player.Quit();
                    Window.GetWindow(this).Hide();
                    e.Cancel = true;
                    if (Closed != null)
                        Closed(this, new EventArgs());
                }
            }
        }

        public float Position {
            get {
                return Player?.GetCurrentPosition() ?? 0;
            }
            set {
                Player.MovePosition((int)value);
            }
        }

        public double Duration {
            get {
                return Player.CurrentPlayingFileLength();
            }
        }

        public void Play(string fileName) {
            //if (System.IO.File.Exists(fileName) == false) {
            //    throw new System.IO.FileNotFoundException("File not found", fileName);
            //}
            MediaPath = fileName;
            Player.Quit();
            Player.Play(fileName);
        }

        public async Task OpenFileAsync(string fileName) {
            await Task.Run(() => {
                Player.Quit();
                Player.Play(fileName);
            });
        }

        public void Show() {
            if (IsWindow) {
                Window.GetWindow(this).Show();
                if (Player.CurrentStatus == MediaStatus.Paused)
                    Player.Pause();
            }
        }

        public void Hide() {
            if (IsWindow) {
                Player.Pause();
                Window.GetWindow(this).Hide();
            }
        }

        public void Close() {
            //Player.Stop();
            Player.Quit();
            AllowClose = true;
            forceClose = true;
            if (IsWindow)
                Window.GetWindow(this).Close();
        }

        public bool Loop {
            get {
                return false;
                //return Player.Loop;
            }
            set {
                //Player.Loop = value;
            }
        }

        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(int),
            typeof(WpfMPlayerControl), new PropertyMetadata(100, OnVolumeChanged));

        public int Volume {
            get {
                return (int)GetValue(VolumeProperty);
            }
            set {
                VolumeBar.Value = value;
                SetValue(VolumeProperty, value);
            }
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var P = ((WpfMPlayerControl)d);
            P.Player.Volume = (int)e.NewValue;
        }
        
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string),
            typeof(WpfMPlayerControl), new PropertyMetadata("", OnSourceChanged));

        public string Source {
            get {
                return (string)GetValue(SourceProperty);
            }
            set {
                SetValue(SourceProperty, value);
            }
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var P = ((WpfMPlayerControl)d);
            P.MediaPath = (string)e.NewValue;
            P.Player.Quit();
            P.Player.Play(P.MediaPath);
        }

        public static readonly DependencyProperty RateProperty =
            DependencyProperty.Register("Rate", typeof(double),
            typeof(WpfMPlayerControl), new PropertyMetadata(1.0, OnRateChanged));

        public double Rate {
            get {
                return (double)GetValue(RateProperty);
            }
            set {
                SetValue(RateProperty, value);
            }
        }

        private static void OnRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var P = ((WpfMPlayerControl)d);
            P.Player.Speed = (double)e.NewValue;
            //P.newRate = (double)e.NewValue;
            //P.reloadTimer.Stop();
            //P.reloadTimer.Start();
        }

        private void SeekBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            isSeekBarButtonDown = true;
        }

        private void SeekBar_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            // Move the video to the new selected postion.
            int NewPos = (int)SeekBar.Value;
            this.Player.Seek(NewPos, Seek.Absolute);
            Player_CurrentPosition(this, new MplayerEvent(NewPos));
            isSeekBarButtonDown = false;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e) {
            Player.Pause();
            if (Player.CurrentStatus == MediaStatus.Playing) {
                PauseButton.Content = "Pause";
                MediaResume?.Invoke(this, new EventArgs());
            } else {
                PauseButton.Content = "Resume";
                MediaPause?.Invoke(this, new EventArgs());
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e) {
            Player?.Stop();
            PauseButton.Content = "Pause";
            MediaStopped?.Invoke(this, new EventArgs());
        }

        private void VolumeBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            Volume = (int)VolumeBar.Value;
        }
        

        //private void ReloadTimer_Tick(object sender, EventArgs e) {
        //    reloadTimer.Stop();
        //    string Src = Player.Source;
        //    double Pos = Player.Position;
        //    Player.Source = "";
        //    Player.Rate = newRate;
        //    Player.Source = Src;
        //    Player.Position = Pos;
        //}
    }
}
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using EmergenceGuardian.NaturalGroundingPlayer.Business;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace NaturalGroundingPlayer {
    /// <summary>
    /// Interaction logic for LayerControl.xaml
    /// </summary>
    public partial class LayerLucidVideoControl : UserControl, ILayer {
        private ImageViewerWindow viewer;
        private MediaElement player;
        private bool isPlaying = true;
        private DispatcherTimer positionTimer;
        public event EventHandler Closing;
        private bool isClosed;

        public Media Item { get; private set; }

        public LayerLucidVideoControl() {
            InitializeComponent();
        }

        public void OpenMedia(Media item) {
            this.Item = item;

            TitleText.Text = Item.Title;
            TitleText.ToolTip = Item.Title;

            player = new MediaElement();
            player.LoadedBehavior = MediaState.Manual;
            player.UnloadedBehavior = MediaState.Manual;
            player.MediaEnded += player_MediaEnded;
            player.IsMuted = true;
            player.Source = new Uri(Settings.I.NaturalGroundingFolder + Item.FileName);
            player.Position = TimeSpan.FromSeconds(Item.StartPos.HasValue ? Item.StartPos.Value : 0);
            player.Play();

            viewer = ImageViewerWindow.Instance(player);
            viewer.Closed += viewer_Closed;

            positionTimer = new DispatcherTimer();
            positionTimer.Interval = TimeSpan.FromSeconds(1);
            positionTimer.Tick += positionTimer_Tick;
            positionTimer.Start();
        }

        private void player_MediaEnded(object sender, RoutedEventArgs e) {
            player.Position = TimeSpan.FromSeconds(Item.StartPos.HasValue ? Item.StartPos.Value : 0);
            player.Play();
        }

        private void positionTimer_Tick(object sender, EventArgs e) {
            Business.MediaTimeConverter Conv = new Business.MediaTimeConverter();
            if (player.NaturalDuration.HasTimeSpan) {
                PositionText.Text = string.Format("{0} / {1}",
                    Conv.Convert((decimal)player.Position.TotalSeconds, typeof(string), null, null),
                    Conv.Convert((decimal)player.NaturalDuration.TimeSpan.TotalSeconds, typeof(string), null, null));
            } else {
                PositionText.Text = Conv.Convert((decimal)player.Position.TotalSeconds, typeof(string), null, null).ToString();
            }
        }

        private void viewer_Closed(object sender, EventArgs e) {
            if (!isClosed) {
                isClosed = true;
                Close();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (viewer != null)
                viewer.Opacity = e.NewValue;
        }

        private void BackwardButton_Click(object sender, RoutedEventArgs e) {
            if (player.NaturalDuration.HasTimeSpan) {
                TimeSpan NewPosition = player.Position.Subtract(TimeSpan.FromSeconds(10));
                if (NewPosition.TotalSeconds < 0)
                    NewPosition = TimeSpan.Zero;
                player.Position = NewPosition;
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e) {
            if (player.NaturalDuration.HasTimeSpan) {
                TimeSpan NewPosition = player.Position.Add(TimeSpan.FromSeconds(10));
                if (NewPosition > player.NaturalDuration.TimeSpan)
                    NewPosition = player.NaturalDuration.TimeSpan;
                player.Position = NewPosition;
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e) {
            if (isPlaying) {
                isPlaying = false;
                player.Pause();
                positionTimer.Stop();
                PauseButtonImage.Source = new BitmapImage(new Uri(@"/NaturalGroundingPlayer;component/Icons/play.png", UriKind.Relative));
                PauseButton.ToolTip = "Play";
            } else {
                isPlaying = true;
                player.Play();
                positionTimer.Start();
                PauseButtonImage.Source = new BitmapImage(new Uri(@"/NaturalGroundingPlayer;component/Icons/pause.png", UriKind.Relative));
                PauseButton.ToolTip = "Pause";
            }
        }

        public void Close() {
            if (!isClosed) {
                isClosed = true;
                player.Stop();
                viewer.Close();
            }
            if (Closing != null)
                Closing(this, new EventArgs());
        }

        public void Hide() {
            viewer.Visibility = System.Windows.Visibility.Hidden;
        }

        public void Show() {
            viewer.Visibility = System.Windows.Visibility.Visible;
        }
    }
}

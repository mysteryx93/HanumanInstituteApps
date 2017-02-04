using System;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows;
using Business;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace Player432hz {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        internal ConfigFile config;
        internal PlaylistItem currentPlaylist;
        internal ObservableCollection<string> files = new ObservableCollection<string>();
        DispatcherTimer posTimer = new DispatcherTimer();
        internal PlayingInfo displayInfo = new PlayingInfo();

        PlayerManager playManager = new PlayerManager();

        public MainWindow() {
            InitializeComponent();
            playManager.StartPlaying += PlayManager_StartPlaying;
            AudioPlayer.Player.MediaOpened += Player_MediaOpened;
            AudioPlayer.Player.MediaStop += Player_MediaStop;
            AudioPlayer.Player.PositionChanged += Player_PositionChanged;

            FilesList.DataContext = files;
            GridPlaying.DataContext = displayInfo;

            posTimer.Tick += PosTimer_Tick;
            posTimer.Interval = new TimeSpan(0, 0, 1);

            config = ConfigFile.Load();
            if (config.Width > 0)
                this.Width = config.Width;
            if (config.Height > 0)
                this.Height = config.Height;
            if (config.Volume > 0 && config.Volume <= 100)
                AudioPlayer.Player.Volume = config.Volume;
            PlaylistsList.DataContext = config.Playlists;
            if (config.Playlists.Count == 0) {
                AddPlaylistButton_Click(null, null);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            PlaylistsList.SelectedIndex = 0;
            PlaylistsList.Focus();
            ((ListBoxItem)PlaylistsList.ItemContainerGenerator.ContainerFromIndex(0)).Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            config.Width = this.Width;
            config.Height = this.Height;
            config.Volume = AudioPlayer.Player.Volume;
            config.Save();
        }

        private void AddPlaylistButton_Click(object sender, RoutedEventArgs e) {
            PlaylistItem NewPlaylist = new PlaylistItem("New Playlist");
            config.Playlists.Add(NewPlaylist);
            PlaylistsList.SelectedItem = NewPlaylist;
            SetCurrentPlaylist(NewPlaylist);
        }

        private void RemovePlaylistButton_Click(object sender, RoutedEventArgs e) {
            if (PlaylistsList.SelectedIndex > -1) {
                int SelPos = PlaylistsList.SelectedIndex;
                config.Playlists.RemoveAt(SelPos);
                if (config.Playlists.Count == 0)
                    AddPlaylistButton_Click(sender, e);
                if (SelPos < config.Playlists.Count - 1)
                    PlaylistsList.SelectedIndex = SelPos;
                else
                    PlaylistsList.SelectedIndex = config.Playlists.Count - 1;
            }
        }

        private void PlaylistsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            if (PlaylistsList.SelectedIndex > -1) {
                SetCurrentPlaylist(config.Playlists[PlaylistsList.SelectedIndex]);
            }
        }

        private void SetCurrentPlaylist(PlaylistItem item) {
            currentPlaylist = item;
            GridFolders.DataContext = item;
            if (FoldersList.SelectedIndex < 0 && item.Folders.Count > 0)
                FoldersList.SelectedIndex = 0;
            RefreshFiles();
        }

        private void AddFolderButton_Click(object sender, RoutedEventArgs e) {
            string NewPath = FileFolderDialog.ShowFolderDialog(null, false);
            if (NewPath != null) {
                currentPlaylist.Folders.Add(NewPath);
                RefreshFiles();
            }
        }

        private void RemoveFolderButton_Click(object sender, RoutedEventArgs e) {
            if (FoldersList.SelectedIndex > -1) {
                currentPlaylist.Folders.RemoveAt(FoldersList.SelectedIndex);
                RefreshFiles();
            }
        }

        private void RefreshFiles() {
            files.Clear();
            foreach (string item in currentPlaylist.Folders) {
                foreach (string file in PlayerManager.GetFiles(item, new string[] { "*.mp3", "*.flac" }, SearchOption.AllDirectories)) {
                    files.Add(file);
                }
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e) {
            if (FilesList.SelectedIndex > -1) {
                string CurrentFile = files[FilesList.SelectedIndex];
                playManager.Play(files, CurrentFile);
            }
        }

        private void FilesList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dataContext is string && e.LeftButton == MouseButtonState.Pressed) {
                PlayButton_Click(null, null);
            }
        }

        private void PlayManager_StartPlaying(object sender, PlayingEventArgs e) {
            NowPlayingLabel.Text = e.FileName;
            MediaInfoReader infoReader = new MediaInfoReader();
            infoReader.LoadInfo(e.FileName);
            AutoPitchBusiness.CreateScript(e.FileName, infoReader, Settings.Player432hzScriptFile);
            AudioPlayer.Player.Source = Settings.Player432hzScriptFile;
            posTimer.Stop();
            NowPlayingPos.Visibility = Visibility.Hidden;
        }

        private void Player_MediaOpened(object sender, EventArgs e) {
            PosTimer_Tick(null, null);
            posTimer.Start();
            NowPlayingPos.Visibility = Visibility.Visible;
        }

        private async void Player_MediaStop(object sender, EventArgs e) {
            await System.Threading.Tasks.Task.Delay(100);
            playManager.PlayNext();
        }

        private void Player_PositionChanged(object sender, EventArgs e) {
            PosTimer_Tick(null, null);
        }

        private void PosTimer_Tick(object sender, EventArgs e) {
            displayInfo.Position = AudioPlayer.Position;
            displayInfo.Duration = AudioPlayer.Duration;
        }
    }
}

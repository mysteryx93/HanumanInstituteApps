using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows;
using Business;
using System.Windows.Controls;
using System.Windows.Input;
using EmergenceGuardian.WpfCommon;
using EmergenceGuardian.FFmpeg;
using System.Threading.Tasks;

namespace Player432hz {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        internal ConfigFile config;
        internal PlaylistItem currentPlaylist;
        internal ObservableCollection<string> files = new ObservableCollection<string>();
        internal PlayingInfo displayInfo = new PlayingInfo();

        AudioPlayerManager playManager = new AudioPlayerManager();

        public MainWindow() {
            InitializeComponent();
            AppPaths.ConfigureFFmpegPaths(this);

            playManager.StartPlaying += PlayManager_StartPlaying;
            FilesList.DataContext = files;

            config = ConfigFile.Load();
            if (config.Width > 0)
                this.Width = config.Width;
            if (config.Height > 0)
                this.Height = config.Height;
            PlaylistsList.DataContext = config.Playlists;
            if (config.Playlists.Count == 0) {
                AddPlaylistButton_Click(null, null);
            }

            Player.MediaPlayerInitialized += (s, e) => {
                Player.Host.Player.MediaUnloaded += Player_MediaUnloaded;
                Player.Host.Player.MediaFinished += Player_MediaFinished;
                if (config.Volume > 0 && config.Volume <= 100)
                    Player.Host.Volume = config.Volume;
            };
        }

        private void Player_MediaFinished(object sender, EventArgs e) {
            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            PlaylistsList.SelectedIndex = 0;
            PlaylistsList.Focus();
            ((ListBoxItem)PlaylistsList.ItemContainerGenerator.ContainerFromIndex(0)).Focus();

            SplashWindow F = SplashWindow.Instance(this, Properties.Resources.AppIcon);
            F.CanClose();
            F.ShowDialog();

            var a = MediaInfo.GetVersion();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            config.Width = this.Width;
            config.Height = this.Height;
            config.Volume = Player.Host.Volume;
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
                foreach (string file in AudioPlayerManager.GetAudioFiles(item)) {
                    files.Add(file);
                }
            }
        }

        // Play selected file and then the playlist.
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

        // Start playing the playlist.
        private void PlaylistsList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dataContext is PlaylistItem && e.LeftButton == MouseButtonState.Pressed) {
                if (PlaylistsList.SelectedIndex > -1) {
                    playManager.Play(files, null);
                }
            }
        }

        private void PlayManager_StartPlaying(object sender, PlayingEventArgs e) {
            if (isStartingPlay)
                return;
            Dispatcher.Invoke(() => {
                if (Player.Host.IsMediaLoaded) {
                    isStartingPlay = true;
                    Player.Host.Player.Stop();
                } else
                    PlayMedia();
                //FFmpegProcess infoReader = MediaInfo.GetFileInfo(e.FileName);
                //AutoPitchBusiness.CreateScript(e.FileName, infoReader, AppPaths.Player432hzScriptFile);
                //Player.Host.Player.Load(AppPaths.Player432hzScriptFile);
                //isStartingPlay = false;
            });
        }

        private void PlayMedia() {
            FFmpegProcess infoReader = MediaInfo.GetFileInfo(playManager.NowPlaying);
            AutoPitchBusiness.CreateScript(playManager.NowPlaying, infoReader, AppPaths.Player432hzScriptFile);
            Player.Host.Source = null;
            Player.Host.Source = AppPaths.Player432hzScriptFile;
            Player.Host.Title = Path.GetFileName(playManager.NowPlaying);
        }

        private bool isStartingPlay = false;
        private void Player_MediaUnloaded(object sender, EventArgs e) {
            if (isStartingPlay) {
                Dispatcher.Invoke(() => PlayMedia());
                isStartingPlay = false;
            } else
                playManager.PlayNext();
            //playManager.PlayNext();
            //if (!isStartingPlay)
            //    playManager.PlayNext();
            //else
            //    isStartingPlay = false;
        }

        //private void Player_MediaOpened(object sender, EventArgs e) {
        //    Dispatcher.Invoke(() => {
        //        Player_PositionChanged(null, new MplayerEvent(0));
        //        NowPlayingPos.Visibility = Visibility.Visible;
        //    });
        //}

        //private void Player_MediaClosed(object sender, MplayerEvent e) {
        //    // Regular file end(1) or process closed(-1)
        //    if (e.Value == 1 || e.Value == -1) {
        //        playManager.PlayNext();
        //    }
        //}

        //private void Player_MediaStopped(object sender, EventArgs e) {
        //    // Stopped manually (MediaClosed has status 4)
        //    playManager.PlayNext();
        //}

        //private void Player_PositionChanged(object sender, MplayerEvent e) {
        //    displayInfo.Position = e.Value;
        //    displayInfo.Duration = Player.Duration;
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Business;
using System.Windows.Input;
using System.Diagnostics;
using EmergenceGuardian.WpfCommon;

namespace PowerliminalsPlayer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        internal ConfigFile config = new ConfigFile();
        private bool isPaused = false;

        public MainWindow() {
            InitializeComponent();

            AppPaths.ConfigureFFmpegPaths(this);
            config = ConfigFile.Load();
            RefreshFiles();
            this.DataContext = config;
            FoldersList.DataContext = config.Folders;
            FilesList.DataContext = config.Files;
            NowPlayingList.DataContext = config.Current.Files;
            VolumeSlider.DataContext = config.Current;

            if (config.Width > 0)
                this.Width = config.Width;
            if (config.Height > 0)
                this.Height = config.Height;
            FoldersExpander_Collapsed(null, null);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            SplashWindow F = SplashWindow.Instance(this, Properties.Resources.AppIcon);
            F.CanClose();
            F.ShowDialog();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            config.Width = this.Width;
            config.Height = this.Height;
            config.Save();
        }

        private void AddFolderButton_Click(object sender, RoutedEventArgs e) {
            string NewPath = FileFolderDialog.ShowFolderDialog(null, false);
            if (NewPath != null) {
                config.Folders.Add(NewPath);
                RefreshFiles();
            }
        }

        private void RemoveFolderButton_Click(object sender, RoutedEventArgs e) {
            if (FoldersList.SelectedIndex > -1) {
                config.Folders.RemoveAt(FoldersList.SelectedIndex);
                RefreshFiles();
            }
        }

        private void RefreshFiles() {
            List<string> files = new List<string>();
            foreach (string item in config.Folders) {
                foreach (string file in AudioPlayerManager.GetAudioFiles(item)) {
                    files.Add(file);
                }
            }
            var Query = files.Where(f => f.IndexOf(SearchBox.Text, StringComparison.CurrentCultureIgnoreCase) != -1).OrderBy(f => f);
            config.Files = new ObservableCollection<string>(Query);
            FilesList.DataContext = config.Files;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e) {
            if (FilesList.SelectedIndex > -1) {
                string CurrentFile = config.Files[FilesList.SelectedIndex];
                config.Current.Files.Add(new FileItem(CurrentFile, config.Current.MasterVolume));
            }
        }

        private void FilesList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dataContext is string && e.LeftButton == MouseButtonState.Pressed) {
                PlayButton_Click(null, null);
            }
        }
        
        private void FoldersExpander_Collapsed(object sender, RoutedEventArgs e) {
            FoldersList.Visibility = Visibility.Hidden;
            AddFolderButton.Visibility = Visibility.Hidden;
            RemoveFolderButton.Visibility = Visibility.Hidden;
            FilesGrid.Margin = new Thickness(FilesGrid.Margin.Left, FilesGrid.Margin.Top - FoldersList.Height, FilesGrid.Margin.Right, FilesGrid.Margin.Bottom);
        }

        private void FoldersExpander_Expanded(object sender, RoutedEventArgs e) {
            FoldersList.Visibility = Visibility.Visible;
            AddFolderButton.Visibility = Visibility.Visible;
            RemoveFolderButton.Visibility = Visibility.Visible;
            FilesGrid.Margin = new Thickness(FilesGrid.Margin.Left, FilesGrid.Margin.Top + FoldersList.Height, FilesGrid.Margin.Right, FilesGrid.Margin.Bottom);
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) {
            RefreshFiles();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e) {
            foreach (var item in NowPlayingList.FindVisualChildren<PlayerInstance>()) {
                if (isPaused)
                    item.WmpPlayer.Player.Play();
                else
                    item.WmpPlayer.Player.Pause();
            }
            PauseButton.Content = isPaused ? "Pause All" : "Resume";
            isPaused = !isPaused;
        }

        private void SavePresetButton_Click(object sender, RoutedEventArgs e) {
            string Result = SelectPresetWindow.InstanceSave(this, config.Presets);
            if (!string.IsNullOrEmpty(Result)) {
                PresetItem Preset = GetPresetByName(Result);
                if (Preset == null) {
                    Preset = new PresetItem();
                    config.Presets.Add(Preset);
                }
                config.Current.SaveAs(Preset);
                Preset.Name = Result;
                config.Save();
            }
        }

        private void LoadPresetButton_Click(object sender, RoutedEventArgs e) {
            string Result = SelectPresetWindow.InstanceLoad(this, config.Presets);
            PresetItem Preset = GetPresetByName(Result);
            if (Preset != null) {
                Preset.SaveAs(config.Current);
                config.Current.MasterVolume = -1;
                config.Current.MasterVolume = Preset.MasterVolume;
            }
        }

        private PresetItem GetPresetByName(string name) {
            if (!string.IsNullOrEmpty(name))
                return config.Presets.FirstOrDefault(p => p.Name == name);
            else
                return null;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}

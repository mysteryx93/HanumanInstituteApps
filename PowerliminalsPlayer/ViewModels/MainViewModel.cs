using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using HanumanInstitute.CommonServices;
using HanumanInstitute.MpvPlayerUI;
using HanumanInstitute.PowerliminalsPlayer.Business;
using HanumanInstitute.CommonWpf;
using HanumanInstitute.CommonWpfApp;
using GalaSoft.MvvmLight;
using MvvmDialogs;
using PropertyChanged;

namespace HanumanInstitute.PowerliminalsPlayer.ViewModels
{
    [AddINotifyPropertyChangedInterface()]
    public class MainViewModel : ViewModelBase
    {
        protected readonly IAppPathService appPath;
        protected readonly AppSettingsProvider appSettings;
        protected readonly IFileSystemService fileSystem;
        protected readonly IDialogService dialogService;

        public AppSettingsFile AppData => appSettings?.Current;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IAppPathService appPath, AppSettingsProvider appSettings, IFileSystemService fileSystem, IDialogService dialogService)
        {
            this.appPath = appPath;
            this.appSettings = appSettings;
            this.fileSystem = fileSystem;
            this.dialogService = dialogService; 

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real"
            }
        }

        public string SearchText { get; set; }

        private void OnSearchTextChanged() => ReloadFiles();

        public int MasterVolume { get; set; }
        public int SelectedFolderIndex { get; set; } = -1;
        public int SelectedFileIndex { get; set; } = -1;
        public bool IsPaused { get; set; }

        /// <summary>
        /// Gets or sets the currently selected preset.
        /// </summary>
        public PresetItem Playlist { get; set; } = new PresetItem();
        /// <summary>
        /// Gets or sets the list of files currently playing.
        /// </summary>
        public ObservableCollection<string> Files { get; set; } = new ObservableCollection<string>();

        public void Load()
        {
            appSettings.Load();
            RaisePropertyChanged(() => AppData);
            ReloadFiles();
        }

        public void Save() => appSettings.Save();

        // public void ReloadFiles() => appSettings.LoadFiles(SearchText);

        /// <summary>
        /// Loads the list of files contained in Folders.
        /// </summary>
        /// <param name="filter">If specified, filters file names containing this value.</param>
        public void ReloadFiles()
        {
            List<string> files = new List<string>();
            foreach (string item in AppData.Folders)
            {
                foreach (string file in GetAudioFiles(item))
                {
                    files.Add(file);
                }
            }

            IEnumerable<string> Query = string.IsNullOrEmpty(SearchText) ? files :
                files.Where(f => f.IndexOf(SearchText, StringComparison.CurrentCultureIgnoreCase) != -1);
            Query = Query.OrderBy(f => f);
            Files = new ObservableCollection<string>(Query);
        }

        /// <summary>
        /// Returns a list of all audio files in specified directory, searching recursively.
        /// </summary>
        /// <param name="path">The path to seasrch for audio files.</param>
        /// <returns>A list of audio files.</returns>
        public IEnumerable<string> GetAudioFiles(string path)
        {
            return fileSystem.GetFilesByExtensions(path, appPath.AudioExtensions, System.IO.SearchOption.AllDirectories);
        }

        public void MediaUnloaded(FileItem item)
        {
            Playlist.Files.Remove(item);
            if (!Playlist.Files.Any())
            {
                IsPaused = false;
            }
        }

        private ICommand addFolderCommand;
        public ICommand AddFolderCommand => CommandHelper.InitCommand(ref addFolderCommand, OnAddFolder);
        private void OnAddFolder()
        {
            string NewPath = FileFolderDialog.ShowFolderDialog(null, false);
            if (NewPath != null)
            {
                AppData.Folders.Add(NewPath);
                ReloadFiles();
            }
        }

        private ICommand removeFolderCommand;
        public ICommand RemoveFolderCommand => CommandHelper.InitCommand(ref removeFolderCommand, OnRemoveFolder, () => CanRemoveFolder);
        private bool CanRemoveFolder => SelectedFolderIndex > -1;
        private void OnRemoveFolder()
        {
            if (CanRemoveFolder)
            {
                AppData.Folders.RemoveAt(SelectedFolderIndex);
                ReloadFiles();
            }
        }

        public void FilesList_MouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dataContext is string && e.LeftButton == MouseButtonState.Pressed)
            {
                OnPlay();
            }
        }

        private ICommand playCommand;
        public ICommand PlayCommand => CommandHelper.InitCommand(ref playCommand, OnPlay, () => CanPlay);
        private bool CanPlay => (SelectedFileIndex > -1);

        private void OnPlay()
        {
            if (CanPlay)
            {
                string CurrentFile = Files[SelectedFileIndex];
                Playlist.Files.Add(new FileItem(CurrentFile, Playlist.MasterVolume));
            }
        }

        private ICommand loadPresetCommand;
        public ICommand LoadPresetCommand => CommandHelper.InitCommand(ref loadPresetCommand, OnLoadPreset, () => CanLoadPreset);
        private bool CanLoadPreset => AppData?.Presets?.Any() == true;
        private void OnLoadPreset()
        {
            if (CanLoadPreset)
            {
                var selectPreset = ViewModelLocator.Instance.SelectPreset.Load(false);
                var result = dialogService.ShowDialog(this, selectPreset);
                var preset = selectPreset.SelectedItem;
                if (result == true && preset != null)
                {
                    preset.SaveAs(Playlist);
                    Playlist.MasterVolume = -1;
                    Playlist.MasterVolume = preset.MasterVolume;
                }
            }
        }

        private ICommand savePresetCommand;
        public ICommand SavePresetCommand => CommandHelper.InitCommand(ref savePresetCommand, OnSavePreset, () => CanSavePreset);
        private bool CanSavePreset => Playlist?.Files?.Any() == true;
        private void OnSavePreset()
        {
            if (CanSavePreset)
            {
                var selectPreset = ViewModelLocator.Instance.SelectPreset.Load(true);
                var result = dialogService.ShowDialog(this, selectPreset);
                var presetName = selectPreset.PresetName;
                if (result == true && !string.IsNullOrWhiteSpace(presetName))
                {
                    var preset = GetPresetByName(presetName);
                    if (preset == null)
                    {
                        preset = new PresetItem();
                        AppData.Presets.Add(preset);
                    }
                    Playlist.SaveAs(preset);
                    preset.Name = presetName;
                    Save();
                }
            }
        }

        private PresetItem GetPresetByName(string name)
        {
            if (!string.IsNullOrEmpty(name))
                return AppData.Presets.FirstOrDefault(p => p.Name == name);
            else
                return null;
        }

        private ICommand pauseCommand;
        public ICommand PauseCommand => CommandHelper.InitCommand(ref pauseCommand, OnPause, () => CanPause);
        private bool CanPause => Playlist.Files?.Any() == true;
        private void OnPause()
        {
            if (CanPause)
            {
                foreach (var item in Playlist.Files)
                {
                    item.IsPlaying = IsPaused;
                }
                IsPaused = !IsPaused;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using HanumanInstitute.CommonServices;
using HanumanInstitute.CommonWpfApp;
using HanumanInstitute.PowerliminalsPlayer.Business;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.FolderBrowser;
using PropertyChanged;

namespace HanumanInstitute.PowerliminalsPlayer.ViewModels
{
    [AddINotifyPropertyChangedInterface()]
    public class MainViewModel : ViewModelBase
    {
        private readonly IAppPathService _appPath;
        private readonly ISettingsProvider<AppSettingsData> _appSettings;
        private readonly IFileSystemService _fileSystem;
        private readonly IDialogService _dialogService;

        public AppSettingsData AppData => _appSettings.Value;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IAppPathService appPath, ISettingsProvider<AppSettingsData> appSettings, IFileSystemService fileSystem, IDialogService dialogService)
        {
            this._appPath = appPath;
            this._appSettings = appSettings;
            this._fileSystem = fileSystem;
            this._dialogService = dialogService;
        }

        public string SearchText { get; set; } = string.Empty;

        //private void OnSearchTextChanged() => ReloadFiles();

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
        public ObservableCollection<string> Files { get; private set; } = new ObservableCollection<string>();

        public void Load()
        {
            _appSettings.Load();
            RaisePropertyChanged(() => AppData);
            ReloadFiles();
        }

        public void Save() => _appSettings.Save();

        /// <summary>
        /// Loads the list of files contained in Folders.
        /// </summary>
        /// <param name="filter">If specified, filters file names containing this value.</param>
        public void ReloadFiles()
        {
            var files = new List<string>();
            foreach (var item in AppData.Folders)
            {
                foreach (var file in GetAudioFiles(item))
                {
                    files.Add(file);
                }
            }

            var query = string.IsNullOrEmpty(SearchText) ? files :
                files.Where(f => f.IndexOf(SearchText, StringComparison.CurrentCultureIgnoreCase) != -1);
            query = query.OrderBy(f => f);
            // Recreate new list to avoid refreshing for each item.
            Files = new ObservableCollection<string>(query);
        }

        /// <summary>
        /// Returns a list of all audio files in specified directory, searching recursively.
        /// </summary>
        /// <param name="path">The path to seasrch for audio files.</param>
        /// <returns>A list of audio files.</returns>
        public IEnumerable<string> GetAudioFiles(string path)
        {
            return _fileSystem.GetFilesByExtensions(path, _appPath.AudioExtensions, System.IO.SearchOption.AllDirectories);
        }

        public void MediaUnloaded(FileItem item)
        {
            Playlist.Files.Remove(item);
            if (!Playlist.Files.Any())
            {
                IsPaused = false;
            }
        }

        public ICommand AddFolderCommand => CommandHelper.InitCommand(ref _addFolderCommand, OnAddFolder);
        private RelayCommand? _addFolderCommand;
        private void OnAddFolder()
        {
            var dialogSettings = new FolderBrowserDialogSettings()
            {
                ShowNewFolderButton = false
            };
            if (_dialogService.ShowFolderBrowserDialog(this, dialogSettings) == true)
            {
                if (!string.IsNullOrEmpty(dialogSettings.SelectedPath))
                {
                    AppData.Folders.Add(dialogSettings.SelectedPath);
                    ReloadFiles();
                }
            }
        }

        public ICommand RemoveFolderCommand => CommandHelper.InitCommand(ref _removeFolderCommand, OnRemoveFolder, () => CanRemoveFolder);
        private RelayCommand? _removeFolderCommand;
        private bool CanRemoveFolder => SelectedFolderIndex > -1;
        private void OnRemoveFolder()
        {
            if (CanRemoveFolder)
            {
                AppData.Folders.RemoveAt(SelectedFolderIndex);
                ReloadFiles();
            }
        }

        public void OnFilesListMouseDoubleClick(MouseButtonEventArgs e)
        {
            e.CheckNotNull(nameof(e));

            var dataContext = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dataContext is string && e.LeftButton == MouseButtonState.Pressed)
            {
                OnPlay();
            }
        }

        public ICommand PlayCommand => CommandHelper.InitCommand(ref _playCommand, OnPlay, () => CanPlay);
        private RelayCommand? _playCommand;
        private bool CanPlay => (SelectedFileIndex > -1);

        private void OnPlay()
        {
            if (CanPlay)
            {
                var currentFile = Files[SelectedFileIndex];
                Playlist.Files.Add(new FileItem(currentFile, Playlist.MasterVolume));
            }
        }

        public ICommand LoadPresetCommand => CommandHelper.InitCommand(ref _loadPresetCommand, OnLoadPreset, () => CanLoadPreset);
        private RelayCommand? _loadPresetCommand;
        private bool CanLoadPreset => AppData?.Presets?.Any() == true;
        private void OnLoadPreset()
        {
            if (CanLoadPreset)
            {
                var selectPreset = ViewModelLocator.SelectPreset.Load(false);
                var result = _dialogService.ShowDialog(this, selectPreset);
                var preset = selectPreset.SelectedItem;
                if (result == true && preset != null)
                {
                    preset.SaveAs(Playlist);
                    Playlist.MasterVolume = -1;
                    Playlist.MasterVolume = preset.MasterVolume;
                }
            }
        }

        public ICommand SavePresetCommand => CommandHelper.InitCommand(ref _savePresetCommand, OnSavePreset, () => CanSavePreset);
        private RelayCommand? _savePresetCommand;
        private bool CanSavePreset => Playlist?.Files?.Any() == true;
        private void OnSavePreset()
        {
            if (CanSavePreset)
            {
                // _dialogService.ShowDialog(this, );

                var selectPreset = ViewModelLocator.SelectPreset.Load(true);
                var result = _dialogService.ShowDialog(this, selectPreset);
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

        private PresetItem? GetPresetByName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return AppData.Presets.FirstOrDefault(p => p.Name == name);
            }
            return null;
        }

        public ICommand PauseCommand => CommandHelper.InitCommand(ref _pauseCommand, OnPause, () => CanPause);
        private RelayCommand? _pauseCommand;
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

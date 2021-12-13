using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using HanumanInstitute.Common.Avalonia;
using HanumanInstitute.MediaPlayer.Avalonia.Helpers.Mvvm;
using HanumanInstitute.Player432hz.Business;
using ReactiveUI;

namespace HanumanInstitute.Player432hz.ViewModels;

/// <summary>
/// Represents a list of files to view the content of specified folders.
/// </summary>
public class FilesListViewModel : ReactiveObject, IFilesListViewModel
{
    private readonly IFileLocator _fileLocator;
    private readonly IPlaylistPlayer _playlistPlayer;

    public FilesListViewModel(IFileLocator fileLocator, IPlaylistPlayer playlistPlayer)
    {
        _fileLocator = fileLocator;
        _playlistPlayer = playlistPlayer;
    }

    /// <summary>
    /// Gets the list of files and selection properties.
    /// </summary>
    public ICollectionView<string> Files
    {
        get
        {
            if (!_loaded)
            {
                Load();
                _loaded = true;
            }
            return _files;
        }
    }
    private readonly ICollectionView<string> _files = new CollectionView<string>();

    /// <summary>
    /// Sets the folder paths from which to load files.
    /// </summary>
    /// <param name="paths">The list of folder paths to load.</param>
    public void SetPaths(IEnumerable<string>? paths)
    {
        _paths = paths?.ToList();
        _files.Source.Clear();
        _loaded = false;
    }
    private List<string>? _paths;
    private bool _loaded;

    /// <summary>
    /// Loads the list of audio files contained within the list of folders.
    /// </summary>
    private void Load()
    {
        _files.Source.Clear();
        if (_paths != null)
        {
            foreach (var item in _fileLocator.GetAudioFiles(_paths))
            {
                _files.Source.Add(item);
            }
        }
    }

    /// <summary>
    /// Starts playing the selected playlist. If string parameter is specified, the specified file path will be played first.
    /// </summary>
    public ICommand PlayCommand => _playCommand ??= new RelayCommand(OnPlay, CanPlay);
    private RelayCommand? _playCommand;
    private bool CanPlay() => Files.Any();
    private void OnPlay()
    {
        if (CanPlay())
        {
            _playlistPlayer.Play(Files, Files.CurrentItem);
        }
    }
}

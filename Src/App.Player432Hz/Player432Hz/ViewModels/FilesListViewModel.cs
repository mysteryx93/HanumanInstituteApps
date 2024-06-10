using System.Linq;
using System.Reactive.Linq;
using DynamicData.Binding;
using HanumanInstitute.Avalonia;
using ReactiveUI;

namespace HanumanInstitute.Player432Hz.ViewModels;

/// <inheritdoc cref="IFilesListViewModel" />
public class FilesListViewModel : ReactiveObject, IFilesListViewModel
{
    private readonly IFileLocator _fileLocator;
    private readonly IPlaylistPlayer _playlistPlayer;

    public FilesListViewModel(IFileLocator fileLocator, IPlaylistPlayer playlistPlayer)
    {
        _fileLocator = fileLocator;
        _playlistPlayer = playlistPlayer;
    }

    /// <inheritdoc />
    public string Search
    {
        get => _search;
        set
        {
            _search = value;
            this.RaisePropertyChanged();
            _filesFiltered.Source.Clear();
            _filesFiltered.AddRange(_files.Source
                .Where(x => x.Name.Contains(_search, StringComparison.CurrentCultureIgnoreCase)));
            this.RaisePropertyChanged(nameof(FilesFiltered));
        }
    }
    private string _search = string.Empty;
    
    /// <inheritdoc />
    public ICollectionView<FileItem> Files
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
    private readonly ICollectionView<FileItem> _files = new CollectionView<FileItem>();

    /// <inheritdoc />
    public ICollectionView<FileItem> FilesFiltered
    {
        get
        {
            if (!_loaded)
            {
                Load();
                _loaded = true;
            }
            return _filesFiltered;
        }
    }
    private readonly ICollectionView<FileItem> _filesFiltered = new CollectionView<FileItem>();

    /// <inheritdoc />
    public void SetPaths(IEnumerable<string>? paths)
    {
        _paths = paths?.ToList();
        _files.Source.Clear();
        _loaded = false;
        this.RaisePropertyChanged(nameof(Files));
        this.RaisePropertyChanged(nameof(FilesFiltered));
    }
    private List<string>? _paths;
    private bool _loaded;

    /// <summary>
    /// Loads the list of audio files contained within the list of folders.
    /// </summary>
    private void Load()
    {
        _files.Source.Clear();
        _filesFiltered.Source.Clear();
        if (_paths != null)
        {
            _files.Source.AddRange(_fileLocator.GetAudioFiles(_paths));
            _filesFiltered.AddRange(_files.Source
                .Where(x => x.Name.Contains(_search, StringComparison.CurrentCultureIgnoreCase)));
        }
    }
    
    /// <inheritdoc />
    public RxCommandUnit Play => _play ??= ReactiveCommand.CreateFromTask(PlayImplAsync,
        this.Files.Source.ToObservableChangeSet().Select(x => x.Any()));
    private RxCommandUnit? _play;
    private Task PlayImplAsync() =>
        _playlistPlayer.PlayAsync(Files.Select(x => x.Path), Files.CurrentItem?.Path);
}

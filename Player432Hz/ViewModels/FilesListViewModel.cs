using System.Linq;
using System.Reactive.Linq;
using DynamicData.Binding;
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
    public void SetPaths(IEnumerable<string>? paths)
    {
        _paths = paths?.ToList();
        _files.Source.Clear();
        _loaded = false;
        this.RaisePropertyChanged(nameof(Files));
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
            _files.Source.AddRange(_fileLocator.GetAudioFiles(_paths));
        }
    }
    
    /// <inheritdoc />
    public RxCommandUnit Play => _play ??= ReactiveCommand.CreateFromTask(PlayImplAsync,
        this.Files.Source.ToObservableChangeSet().Select(x => x.Any()));
    private RxCommandUnit? _play;
    private Task PlayImplAsync() =>
        _playlistPlayer.PlayAsync(Files.Select(x => x.Path), Files.CurrentItem?.Path);
}

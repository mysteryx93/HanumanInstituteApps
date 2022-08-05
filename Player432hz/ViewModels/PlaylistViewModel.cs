using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using ReactiveUI;

namespace HanumanInstitute.Player432hz.ViewModels;

/// <summary>
/// Represents a playlist for viewing and editing.
/// </summary>
public class PlaylistViewModel : ReactiveObject, IPlaylistViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IFilesListViewModel _fileListViewModel;
    /// <summary>
    /// The ViewModel of the View hosting this instance. 
    /// </summary>
    private readonly INotifyPropertyChanged? _owner;

    public PlaylistViewModel(IDialogService dialogService, IFilesListViewModel fileListViewModel,
        SettingsPlaylistItem? data, INotifyPropertyChanged? owner)
    {
        _dialogService = dialogService;
        _fileListViewModel = fileListViewModel;
        _owner = owner;

        if (data != null)
        {
            Name = data.Name;
            Folders.Source.Clear();
            Folders.Source.AddRange(data.Folders);
        }
    }

    /// <summary>
    /// Gets or sets the name of this playlist.
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value, nameof(Name));
    }
    private string _name = string.Empty;

    /// <summary>
    /// Gets the list of folders in the playlist and provides selection properties.
    /// </summary>
    public ICollectionView<string> Folders { get; } = new CollectionView<string>();

    /// <summary>
    /// Shows a folder picker and adds selected folder to the list.
    /// </summary>
    public RxCommandUnit AddFolderCommand => _addFolderCommand ??= ReactiveCommand.CreateFromTask(OnAddFolder);
    private RxCommandUnit? _addFolderCommand;
    private async Task OnAddFolder()
    {
        var folderSettings = new OpenFolderDialogSettings();
        var result = await _dialogService.ShowOpenFolderDialogAsync(_owner!, folderSettings).ConfigureAwait(true);
        if (!string.IsNullOrEmpty(result) && !Folders.Source.Contains(result))
        {
            Folders.Source.Add(result);
            Folders.MoveCurrentToLast();
            _fileListViewModel.SetPaths(Folders.Source);
        }
    }

    /// <summary>
    /// Removes selected folder from the list.
    /// </summary>
    public RxCommandUnit RemoveFolderCommand =>
        _removeFolderCommand ??= ReactiveCommand.Create(OnRemoveFolder,
            this.WhenAnyValue(x => x.Folders.CurrentItem).Select(x => x != null));
    private RxCommandUnit? _removeFolderCommand;
    private void OnRemoveFolder()
    {
        if (Folders.CurrentPosition > -1)
        {
            var selection = Folders.CurrentPosition;
            Folders.Source.RemoveAt(Folders.CurrentPosition);
            Folders.CurrentPosition = selection;
            _fileListViewModel.SetPaths(Folders.Source);
        }
    }
}

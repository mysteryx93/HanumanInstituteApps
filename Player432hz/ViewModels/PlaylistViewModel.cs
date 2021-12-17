using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HanumanInstitute.Common.Avalonia;
using HanumanInstitute.Player432hz.Business;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs;
using ReactiveUI;

namespace HanumanInstitute.Player432hz.ViewModels;

/// <summary>
/// Represents a playlist for viewing and editing.
/// </summary>
public class PlaylistViewModel : ReactiveObject, IPlaylistViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IFilesListViewModel _fileListViewModel;

    public PlaylistViewModel(IDialogService dialogService, IFilesListViewModel fileListViewModel) :
        this(dialogService, fileListViewModel, null)
    {
    }

    public PlaylistViewModel(IDialogService dialogService, IFilesListViewModel fileListViewModel,
        SettingsPlaylistItem? data)
    {
        _dialogService = dialogService;
        _fileListViewModel = fileListViewModel;

        if (data != null)
        {
            Name = data.Name;
            Folders.Source.Clear();
            foreach (var item in data.Folders)
            {
                Folders.Source.Add(item);
            }
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
    public ICommand AddFolderCommand => _addFolderCommand ??= ReactiveCommand.CreateFromTask(OnAddFolder);
    private ICommand? _addFolderCommand;
    private async Task OnAddFolder()
    {
        var folderSettings = new OpenFolderDialogSettings();
        var result = await _dialogService.ShowOpenFolderDialogAsync(this, folderSettings).ConfigureAwait(true);
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
    public ICommand RemoveFolderCommand =>
        _removeFolderCommand ??= ReactiveCommand.Create(OnRemoveFolder,
            this.WhenAnyValue(x => x.Folders.CurrentItem).Select(x => x != null));
    private ICommand? _removeFolderCommand;
    private void OnRemoveFolder()
    {
        var selection = Folders.CurrentPosition;
        Folders.Source.RemoveAt(Folders.CurrentPosition);
        Folders.MoveCurrentToPosition(selection);
        _fileListViewModel.SetPaths(Folders.Source);
    }
}

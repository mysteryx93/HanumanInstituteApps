using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.Player432hz.Models;
using HanumanInstitute.Player432hz.Properties;

namespace HanumanInstitute.Player432hz.ViewModels;

/// <summary>
/// Creates new instances of IPlaylistViewModel.
/// </summary>
public class PlaylistViewModelFactory : IPlaylistViewModelFactory
{
    private readonly IDialogService _dialogService;
    private readonly IFilesListViewModel _fileListViewModel;

    public PlaylistViewModelFactory(IDialogService dialogService, IFilesListViewModel fileListViewModel)
    {
        _dialogService = dialogService;
        _fileListViewModel = fileListViewModel;
    }

    /// <summary>
    /// Returns a new instance of PlaylistViewModel with default playlist name.
    /// </summary>
    /// <returns>A new PlaylistViewModel instance.</returns>
    public IPlaylistViewModel Create() => Create(Resources.NewPlaylistName);

    /// <summary>
    /// Returns a new instance of PlaylistViewModel with specified playlist name.
    /// </summary>
    /// <param name="name">The name of the new playlist.</param>
    /// <returns>A new PlaylistViewModel instance.</returns>
    public IPlaylistViewModel Create(string name)
    {
        return new PlaylistViewModel(_dialogService, _fileListViewModel)
        {
            Name = name
        };
    }

    /// <summary>
    /// Returns a new instance of PlaylistViewModel from settings data.
    /// </summary>
    /// <param name="data">A playlist element within the settings file.</param>
    /// <returns>A new PlaylistViewModel instance.</returns>
    public IPlaylistViewModel Create(SettingsPlaylistItem data)
    {
        return new PlaylistViewModel(_dialogService, _fileListViewModel, data);
    }
}

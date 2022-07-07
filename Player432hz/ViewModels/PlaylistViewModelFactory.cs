using System.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.Player432hz.Properties;

namespace HanumanInstitute.Player432hz.ViewModels;

/// <inheritdoc />
public class PlaylistViewModelFactory : IPlaylistViewModelFactory
{
    private readonly IDialogService _dialogService;
    private readonly IFilesListViewModel _fileListViewModel;

    /// <inheritdoc />
    public INotifyPropertyChanged? OwnerViewModel { get; set; }

    private INotifyPropertyChanged Owner => OwnerViewModel ?? throw new ArgumentNullException(nameof(OwnerViewModel),
        @"OwnerViewModel must be set before calling IPlaylistViewModelFactory.Create");

    public PlaylistViewModelFactory(IDialogService dialogService, IFilesListViewModel fileListViewModel)
    {
        _dialogService = dialogService;
        _fileListViewModel = fileListViewModel;
    }

    /// <inheritdoc />
    public IPlaylistViewModel Create() => Create(Resources.NewPlaylistName);

    /// <inheritdoc />
    public IPlaylistViewModel Create(string name) =>
        new PlaylistViewModel(_dialogService, _fileListViewModel, null, Owner) { Name = name };

    /// <inheritdoc />
    public IPlaylistViewModel Create(SettingsPlaylistItem data) =>
        new PlaylistViewModel(_dialogService, _fileListViewModel, data, Owner);
}

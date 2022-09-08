using System.Linq;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using ReactiveUI;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.Player432hz.Tests.ViewModels;

public class MainViewModelTests
{
    public Mock<IFilesListViewModel> MockFileList => _mockFileList ??= new Mock<IFilesListViewModel>();
    private Mock<IFilesListViewModel> _mockFileList;

    public ISettingsProvider<AppSettingsData> MockSettings => _mockSettings ??= new FakeSettingsProvider<AppSettingsData>();
    private ISettingsProvider<AppSettingsData> _mockSettings;

    public PlaylistViewModelFactory Factory => _factory ??= new PlaylistViewModelFactory(Mock.Of<IDialogService>(), Mock.Of<IFilesListViewModel>());
    private PlaylistViewModelFactory _factory;

    public MainViewModel Model => _model ??= new MainViewModel(MockSettings, MockAppUpdate.Object, Factory, MockFileList.Object, DialogService);
    private MainViewModel _model;
    
    public Mock<IDialogManager> MockDialogManager => _mockDialogManager ??= new Mock<IDialogManager>();
    private Mock<IDialogManager> _mockDialogManager;

    public Mock<IAppUpdateService> MockAppUpdate => _mockAppUpdate ??= new Mock<IAppUpdateService>();
    private Mock<IAppUpdateService> _mockAppUpdate;

    public IDialogService DialogService => _dialogService ??= new DialogService(MockDialogManager.Object);
    private IDialogService _dialogService;

    private void AddPlaylists(int count)
    {
        for (var i = 0; i < count; i++)
        {
            Model.Playlists.Source.Add(_factory!.Create());
        }
    }

    [Fact]
    public void CanAddPlaylistCommand_ReturnsTrue()
    {
        var result = Model.AddPlaylist.CanExecute();

        Assert.True(result);
    }

    [Fact]
    public void AddPlaylistCommand_Execute_SelectedIndexSet()
    {
        Model.AddPlaylist.Execute().Subscribe();

        Assert.Equal(0, Model.Playlists.CurrentPosition);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(int.MinValue, false)]
    [InlineData(int.MaxValue, true)]
    public void CanDeletePlaylistCommand_WithSelectedIndex_ReturnsTrueIfSelectedIndexValid(int selectedIndex, bool expected)
    {
        AddPlaylists(2); // List contains 2 elements.
        Model.Playlists.CurrentPosition = selectedIndex;

        var result = Model.DeletePlaylist.CanExecute();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void DeletePlaylistCommand_NoSelectedIndex_NoAction()
    {
        var listCount = 2;
        AddPlaylists(listCount);

        Model.DeletePlaylist.Execute().Subscribe();

        Assert.Equal(listCount, Model.Playlists.Source.Count);
    }

    [Fact]
    public void DeletePlaylistCommand_EmptyList_NoAction()
    {
        Model.DeletePlaylist.Execute().Subscribe();

        Assert.Empty(Model.Playlists.Source);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void DeletePlaylistCommand_ValidSelectedIndex_RemoveAtSelectedIndex(int selectedIndex)
    {
        var listCount = 3;
        AddPlaylists(listCount);
        Model.Playlists.CurrentPosition = selectedIndex;
        var selectedItem = Model.Playlists.CurrentItem;

        Model.DeletePlaylist.Execute().Subscribe();

        Assert.Equal(listCount - 1, Model.Playlists.Source.Count);
        Assert.DoesNotContain(selectedItem, Model.Playlists.Source);
    }

    [Theory]
    [InlineData(1, 0, -1)]
    [InlineData(2, 0, 0)]
    [InlineData(3, 2, 1)]
    public void DeletePlaylistCommand_LastSelected_SetValidSelectedIndex(int count, int sel, int newSel)
    {
        AddPlaylists(count);
        Model.Playlists.CurrentPosition = sel;

        Model.DeletePlaylist.Execute().Subscribe();

        Assert.Equal(newSel, Model.Playlists.CurrentPosition);
    }

    [Fact]
    public void DeletePlaylistCommand_ValidSelectedIndex_FilesListSetPathsCalled()
    {
        AddPlaylists(1);
        Model.Playlists.MoveCurrentToFirst();
        MockFileList.Reset();

        Model.DeletePlaylist.Execute().Subscribe();

        MockFileList.Verify(x => x.SetPaths(It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Fact]
    public void Playlists_ValidSelectedIndex_FilesListSetPathsCalled()
    {
        AddPlaylists(1);

        Model.Playlists.MoveCurrentToFirst();

        MockFileList.Verify(x => x.SetPaths(Model.Playlists.First().Folders.Source), Times.Once);
    }

    [Fact]
    public void Playlists_RemoveSelection_FilesListSetPathsCalled()
    {
        AddPlaylists(1);
        Model.Playlists.MoveCurrentToFirst();
        MockFileList.Reset();

        Model.Playlists.CurrentPosition = -1;

        MockFileList.Verify(x => x.SetPaths(It.IsAny<IEnumerable<string>>()), Times.Once);
    }

    [Fact]
    public void Settings_Loaded_FillPlaylists()
    {
        AddPlaylists(1); // make sure it gets removed
        MockSettings.Value.Playlists.Add(new SettingsPlaylistItem("P1"));
        MockSettings.Value.Playlists.Add(new SettingsPlaylistItem("P2"));

        MockSettings.Load();

        Assert.Equal(2, Model.Playlists.Source.Count);
    }

    [Fact]
    public void Settings_LoadedWithFolders_FillPlaylistFolders()
    {
        var folders = new List<string>() { "a", "b", "c" };
        MockSettings.Value.Playlists.Add(new SettingsPlaylistItem("P1", folders));

        MockSettings.Load();

        Assert.NotEmpty(Model.Playlists.Source);
        Assert.Equal(3, Model.Playlists.Source[0].Folders.Source.Count);
    }

    [Fact]
    public void ViewClosed_WithPlaylists_FillSettings()
    {
        AddPlaylists(2);

        Model.OnClosed();

        Assert.Equal(2, MockSettings.Value.Playlists.Count);
    }

    [Fact]
    public void ViewClosed_WithFolders_FillSettingsFolders()
    {
        AddPlaylists(1);
        Model.Playlists.Source[0].Folders.Source.Add("a");

        Model.OnClosed();

        Assert.NotNull(MockSettings.Value.Playlists);
        Assert.Single(MockSettings.Value.Playlists[0].Folders);
    }
}

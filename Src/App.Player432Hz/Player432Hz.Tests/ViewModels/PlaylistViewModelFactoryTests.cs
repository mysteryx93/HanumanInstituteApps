using System.ComponentModel;
using HanumanInstitute.MvvmDialogs;

namespace HanumanInstitute.Player432Hz.Tests.ViewModels;

public class PlaylistViewModelFactoryTests : TestsBase
{
    public PlaylistViewModelFactoryTests(ITestOutputHelper output) : base(output)
    {
    }
    
    public IPlaylistViewModelFactory Model => _model ??= new PlaylistViewModelFactory(Mock.Of<IDialogService>(), Mock.Of<IFilesListViewModel>())
    {
        OwnerViewModel = Mock.Of<INotifyPropertyChanged>()
    };
    private IPlaylistViewModelFactory _model;

    private const string DefaultPlaylistName = "New Playlist";

    [Fact]
    public void Create_NoParam_ReturnsNewInstanceWithDefaultName()
    {
        var obj = Model.Create();

        Assert.IsAssignableFrom<PlaylistViewModel>(obj);
        Assert.Equal(DefaultPlaylistName, obj.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("a")]
    [InlineData(DefaultPlaylistName)]
    public void Create_WithName_ReturnsNewInstanceWithName(string name)
    {
        var obj = Model.Create(name);

        Assert.IsAssignableFrom<PlaylistViewModel>(obj);
        Assert.Equal(name, obj.Name);
    }
}

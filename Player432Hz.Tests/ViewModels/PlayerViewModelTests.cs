using HanumanInstitute.BassAudio;
// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.Player432Hz.Tests.ViewModels;

public class PlayerViewModelTests
{
    public FakeFileSystemService MockFileSystem => _mockFileSystem ??= new FakeFileSystemService();
    private FakeFileSystemService _mockFileSystem;

    public IPlaylistPlayer PlaylistPlayer => _playlistPlayer ??= new PlaylistPlayer(MockPitchDetector.Object, MockFileSystem, MockSettings);
    private IPlaylistPlayer _playlistPlayer;

    public ISettingsProvider<AppSettingsData> MockSettings => _mockSettings ??= new FakeSettingsProvider<AppSettingsData>();
    private ISettingsProvider<AppSettingsData> _mockSettings;

    public Mock<IPitchDetectorWithCache> MockPitchDetector => _mockPitchDetector ??= CreatePitchDetector();
    private Mock<IPitchDetectorWithCache> _mockPitchDetector;
    private Mock<IPitchDetectorWithCache> CreatePitchDetector()
    {
        var mock = new Mock<IPitchDetectorWithCache>();
        mock.Setup(x => x.GetPitch(It.IsAny<string>())).Returns(440f);
        mock.Setup(x => x.GetPitchAsync(It.IsAny<string>())).Returns(Task.FromResult(440f));
        return mock;
    }

    public IPlayerViewModel Model => _model ??= SetupModel();
    private IPlayerViewModel _model;

    private const string FileName1 = "file1", FileName2 = "file2", FileName3 = "file3";
    private int _nowPlayingChanged;

    private IPlayerViewModel SetupModel()
    {
        var result = new PlayerViewModel(PlaylistPlayer);
        result.Player.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(result.Player.NowPlaying))
            {
                _nowPlayingChanged++;
            }
        };
        return result;
    }

    [Fact]
    public void Constructor_VerifyInitialState()
    {
        Assert.NotNull(Model.Player);
        Assert.NotNull(Model.Player.Files);
        Assert.Empty(Model.Player.Files);
        Assert.Empty(Model.Player.NowPlaying);
        Assert.Empty(Model.Player.NowPlayingTitle);
    }

    [Fact]
    public async Task PlayAsync_ListNoCurrent_StartPlayback()
    {
        await Model.Player.PlayAsync(new[] { FileName1 }, null);

        Assert.Equal(1, Model.Player.Files.Count);
        Assert.NotEmpty(Model.Player.NowPlaying);
        Assert.EndsWith(FileName1, Model.Player.NowPlayingTitle);
        Assert.Equal(1, _nowPlayingChanged);
    }

    [Fact]
    public async Task Play_ListSetCurrent_StartPlayback()
    {
        await Model.Player.PlayAsync(new[] { FileName1 }, FileName2);

        Assert.Equal(1, Model.Player.Files.Count);
        Assert.NotEmpty(Model.Player.NowPlaying);
        Assert.EndsWith(FileName2, Model.Player.NowPlayingTitle);
        Assert.Equal(1, _nowPlayingChanged);
    }

    [Fact]
    public async Task Play_NoListSetCurrent_StartPlayback()
    {
        await Model.Player.PlayAsync(null, FileName2);

        Assert.Empty(Model.Player.Files);
        Assert.NotEmpty(Model.Player.NowPlaying);
        Assert.EndsWith(FileName2, Model.Player.NowPlayingTitle);
        Assert.Equal(1, _nowPlayingChanged);
    }

    [Fact]
    public async Task Play_NoListNoCurrent_NowPlayingEmpty()
    {
        await Model.Player.PlayAsync(null, null);

        Assert.Empty(Model.Player.Files);
        Assert.Empty(Model.Player.NowPlaying);
        Assert.Empty(Model.Player.NowPlayingTitle);
        Assert.Equal(0, _nowPlayingChanged);
    }

    [Fact]
    public async Task PlayNext_ListNoCurrent_StartPlayback()
    {
        await Model.Player.PlayAsync(new[] { FileName1 }, null);
        await Model.Player.PlayNextAsync();

        Assert.Equal(1, Model.Player.Files.Count);
        Assert.NotEmpty(Model.Player.NowPlaying);
        Assert.EndsWith(FileName1, Model.Player.NowPlayingTitle);
        Assert.Equal(3, _nowPlayingChanged);
    }

    [Fact]
    public async Task PlayNext_ListSetCurrent_StartPlayback()
    {
        await Model.Player.PlayAsync(new[] { FileName1, FileName2, FileName3 }, FileName1);
        await Model.Player.PlayNextAsync();

        Assert.Equal(3, Model.Player.Files.Count);
        Assert.NotEmpty(Model.Player.NowPlaying);
        await Task.Yield();
        Assert.NotEqual(FileName1, Model.Player.NowPlayingTitle);
        Assert.Equal(3, _nowPlayingChanged);
    }

    [Fact]
    public async Task PlayNext_NoListSetCurrent_StartPlayback()
    {
        await Model.Player.PlayAsync(null, FileName2);
        await Model.Player.PlayNextAsync();

        Assert.Empty(Model.Player.Files);
        Assert.NotEmpty(Model.Player.NowPlaying);
        Assert.EndsWith(FileName2, Model.Player.NowPlayingTitle);
        Assert.Equal(1, _nowPlayingChanged);
    }

    [Fact]
    public async Task PlayNext_NoListNoCurrent_NowPlayingEmpty()
    {
        await Model.Player.PlayAsync(null, null);
        await Model.Player.PlayNextAsync();

        Assert.Empty(Model.Player.Files);
        Assert.Empty(Model.Player.NowPlaying);
        Assert.Empty(Model.Player.NowPlayingTitle);
        Assert.Equal(0, _nowPlayingChanged);
    }

    [Fact]
    public async Task PlayTwice_List_RestartsPlayback()
    {
        await Model.Player.PlayAsync(new[] { FileName1 }, null);
        await Model.Player.PlayAsync(new[] { FileName2 }, null);

        Assert.Equal(1, Model.Player.Files.Count);
        Assert.NotEmpty(Model.Player.NowPlaying);
        Assert.EndsWith(FileName2, Model.Player.NowPlayingTitle);
        Assert.Equal(3, _nowPlayingChanged);
    }
}

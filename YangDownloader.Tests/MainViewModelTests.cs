using System.Collections.Generic;
using System.Net.Http;
using HanumanInstitute.YangDownloader.Models;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using Container = YoutubeExplode.Videos.Streams.Container;

// ReSharper disable MemberCanBePrivate.Global

namespace YangDownloader.Tests;

public class MainViewModelTests : TestsBase
{
    public MainViewModelTests(ITestOutputHelper output) : base(output) { }

    public MainViewModel Model => _model ??= Init(() =>
    {
        SetTitle();
        return new MainViewModel(MockDownloadManager.Object, StreamSelector, DialogService, FakeFileSystem, FakeSettings)
        {
            DownloadUrl = "https://www.youtube.com/watch?v=4OqXWzekVw4"
        };
    });
    private MainViewModel _model;

    public Mock<IDownloadManager> MockDownloadManager => _downloadManager ??= Init(() =>
    {
        var mock =  new Mock<IDownloadManager>();
        mock.Setup(x =>
                x.SelectStreams(It.IsAny<StreamManifest>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<DownloadOptions>()))
            .Returns<StreamManifest, bool, bool, DownloadOptions>((streams, downloadVideo, downloadAudio, options) =>
                StreamSelector.SelectStreams(streams, downloadVideo, downloadAudio, options));
        return mock;
    });
    private Mock<IDownloadManager> _downloadManager;

    public IYouTubeStreamSelector StreamSelector => _streamSelector ??= new YouTubeStreamSelector();
    private IYouTubeStreamSelector _streamSelector;

    public IDialogService DialogService => _dialogService ??= new DialogService(MockDialogManager.Object);
    private IDialogService _dialogService;

    public Mock<IDialogManager> MockDialogManager => _mockDialogManager ??= new Mock<IDialogManager>();
    private Mock<IDialogManager> _mockDialogManager;

    public IFileSystemService FakeFileSystem => _fakeFileSystem ??= new FakeFileSystemService();
    private IFileSystemService _fakeFileSystem;

    public ISettingsProvider<AppSettingsData> FakeSettings => _fakeSettings ??= new FakeSettingsProvider<AppSettingsData>();
    private ISettingsProvider<AppSettingsData> _fakeSettings;

    protected void SetQueryError(Exception ex) =>
        MockDownloadManager.Setup(x => x.QueryStreamInfoAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromException<StreamManifest>(ex));

    protected void SetTitle(string title = "Title") =>
        MockDownloadManager.Setup(x => x.QueryVideoAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new Video(new VideoId(), title, new Author(new ChannelId(), "ChannelTitle"), DateTimeOffset.Now,
                "Description", TimeSpan.FromMinutes(1), Array.Empty<Thumbnail>(), Array.Empty<string>(), new Engagement(1000, 100, 10))));

    protected void SetStreamInfo(IReadOnlyList<IStreamInfo> streams) =>
        MockDownloadManager.Setup(x => x.QueryStreamInfoAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new StreamManifest(streams)));

    protected void SetVideoStream(Container? container = null) =>
        SetStreamInfo(new List<IStreamInfo>()
        {
            new VideoOnlyStreamInfo(string.Empty, container ?? Container.WebM, new FileSize(1000), new Bitrate(1000), "vp9",
                new VideoQuality(720, 30), new Resolution(720, 960))
        });

    protected void SetAudioStream(Container? container = null) =>
        SetStreamInfo(new List<IStreamInfo>()
        {
            new AudioOnlyStreamInfo(string.Empty, container ?? Container.WebM, new FileSize(1000), new Bitrate(1000), "opus")
        });

    protected void SetMuxedStream(Container? container = null) =>
        SetStreamInfo(new List<IStreamInfo>()
        {
            new MuxedStreamInfo(string.Empty, container ?? Container.Mp4,
                new FileSize(1000), new Bitrate(1000), "acc", "vp9", new VideoQuality(720, 30), new Resolution(720, 960))
        });

    protected void AssertMessage(bool hideInfo = true)
    {
        Assert.NotEmpty(Model.ErrorMessage);
        Output.WriteLine(Model.ErrorMessage);
        if (hideInfo)
        {
            Assert.False(Model.DisplayDownloadInfo);
        }
    }

    [Fact]
    public void Constructor_Valid_HasInitialState()
    {
        _model = new MainViewModel(MockDownloadManager.Object, StreamSelector, DialogService, FakeFileSystem, FakeSettings);

        Assert.NotEmpty(Model.PreferredVideo);
        Assert.Equal(0, Model.PreferredVideo.CurrentPosition);
        Assert.NotEmpty(Model.PreferredAudio);
        Assert.Equal(0, Model.PreferredAudio.CurrentPosition);
        Assert.NotEmpty(Model.MaxQuality);
        Assert.Equal(0, Model.MaxQuality.CurrentPosition);
        Assert.False(Model.HasDownloads);
        Assert.Empty(Model.DownloadUrl);
        Assert.False(Model.Query.CanExecute());
        Assert.False(Model.Download.CanExecute());
    }

    [Fact]
    public void Query_PrefNone_DisplayError()
    {
        Model.PreferredVideo.SelectedValue = StreamContainerOption.None;
        Model.PreferredAudio.SelectedValue = StreamContainerOption.None;

        Model.Query.ExecuteIfCan();

        AssertMessage();
    }

    [Fact]
    public void Query_InvalidUrl_DisplayError()
    {
        Model.DownloadUrl = "Invalid";

        Model.Query.ExecuteIfCan();

        AssertMessage();
    }

    [Fact]
    public void Query_Valid_SetVideoTitle()
    {
        var title = "My video title";
        SetTitle(title);
        SetVideoStream();

        Model.Query.ExecuteIfCan();

        Assert.NotEmpty(Model.VideoTitle);
        Output.WriteLine(Model.VideoTitle);
        Assert.True(Model.DisplayDownloadInfo);
    }

    [Fact]
    public void Query_Cancelled_HideInfo()
    {
        SetQueryError(new TaskCanceledException());

        Model.Query.ExecuteIfCan();

        Assert.False(Model.DisplayDownloadInfo);
    }

    [Fact]
    public void Query_HttpRequestException_DisplayError()
    {
        SetQueryError(new HttpRequestException());

        Model.Query.ExecuteIfCan();

        AssertMessage();
    }

    [Fact]
    public void Query_UriFormatException_DisplayError()
    {
        SetQueryError(new UriFormatException());

        Model.Query.ExecuteIfCan();

        AssertMessage();
    }

    [Fact]
    public void Query_NoStream_DisplayError()
    {
        SetStreamInfo(Array.Empty<IStreamInfo>());

        Model.Query.ExecuteIfCan();

        AssertMessage();
    }

    [Fact]
    public void Query_NoMatchingStream_DisplayError()
    {
        Model.PreferredVideo.SelectedValue = StreamContainerOption.MP4;
        Model.PreferredAudio.SelectedValue = StreamContainerOption.None;
        SetMuxedStream(Container.WebM);

        Model.Query.ExecuteIfCan();

        AssertMessage();
    }

    [Fact]
    public void Query_MuxedStream_DisplayStreamInfo()
    {
        SetMuxedStream();

        Model.Query.ExecuteIfCan();

        Assert.Empty(Model.ErrorMessage);
        Assert.NotEmpty(Model.VideoStreamInfo);
        Output.WriteLine(Model.VideoStreamInfo);
    }

    [Fact]
    public void Query_VideoStream_DisplayStreamInfo()
    {
        SetVideoStream();

        Model.Query.ExecuteIfCan();

        Assert.Empty(Model.ErrorMessage);
        Assert.NotEmpty(Model.VideoStreamInfo);
        Output.WriteLine(Model.VideoStreamInfo);
        Assert.Empty(Model.AudioStreamInfo);
    }

    [Fact]
    public void Query_AudioStream_DisplayStreamInfo()
    {
        SetAudioStream();

        Model.Query.ExecuteIfCan();

        Assert.Empty(Model.ErrorMessage);
        Assert.NotEmpty(Model.AudioStreamInfo);
        Output.WriteLine(Model.AudioStreamInfo);
        Assert.Empty(Model.VideoStreamInfo);
    }

    [Fact]
    public void Download_NoStreamSelection_DisplayError()
    {
        SetVideoStream();

        Model.Query.ExecuteIfCan();
        Model.DownloadAudio = false;
        Model.DownloadVideo = false;
        Model.Download.ExecuteIfCan();

        AssertMessage(false);
    }


    [Fact]
    public void Download_Cancelled_HideInfo()
    {
        Model.Query.ExecuteIfCan();
        
        SetQueryError(new TaskCanceledException());
        Model.Download.ExecuteIfCan();

        Assert.False(Model.DisplayDownloadInfo);
    }

    [Fact]
    public void Download_HttpRequestException_DisplayError()
    {
        Model.Query.ExecuteIfCan();
        
        SetQueryError(new HttpRequestException());
        Model.Download.ExecuteIfCan();

        AssertMessage();
    }

    [Fact]
    public void Download_UriFormatException_DisplayError()
    {
        Model.Query.ExecuteIfCan();

        SetQueryError(new UriFormatException());
        Model.Download.ExecuteIfCan();

        AssertMessage();
    }

    [Theory]
    [InlineData("", "/Out/Video.mp4")]
    [InlineData("Yay", "/Out/Yay.mp4")]
    [InlineData("Video Title", "/Out/Video Title.mp4")]
    [InlineData("/Video/Title/", "/Out/_Video_Title_.mp4")]
    [InlineData("///", "/Out/___.mp4")]
    [InlineData("Video.mp4", "/Out/Video.mp4.mp4")]
    public void Download_WithTitle_SetDestination(string title, string expectedDest)
    {
        Model.Settings.DestinationFolder = "/Out";
        SetVideoStream(Container.Mp4);
        SetTitle(title);
        FakeFileSystem.Directory.CreateDirectory(Model.Settings.DestinationFolder);
        var outDest = string.Empty;
        MockDownloadManager.Setup(x =>
                x.DownloadAsync(It.IsAny<StreamQueryInfo>(), It.IsAny<string>(), It.IsAny<DownloadTaskEventHandler>()))
            .Returns<StreamQueryInfo, string, DownloadTaskEventHandler>((_, dest, _) =>
            {
                outDest = dest;
                return Task.FromResult(DownloadStatus.Success);
            });

        Model.Query.ExecuteIfCan();
        Model.Download.ExecuteIfCan();

        Output.WriteLine(outDest);
        Assert.Equal(expectedDest, outDest);
    }
    
    [Theory]
    [InlineData("File1", "/Out/File1.mp4", null, "/Out/File1 (2).mp4")]
    [InlineData("File", "/Out/File.mp4", "/Out/File (2).mp4", "/Out/File (3).mp4")]
    [InlineData("My File.mp4", "/Out/My File.mp4.mp4", null, "/Out/My File.mp4 (2).mp4")]
    public void Download_FileExists_SetDestinationWithNumber(string title, string exists1, string exists2, string expectedDest)
    {
        Model.Settings.DestinationFolder = "/Out";
        SetVideoStream(Container.Mp4);
        SetTitle(title);
        FakeFileSystem.Directory.CreateDirectory(Model.Settings.DestinationFolder);
        FakeFileSystem.File.Create(exists1);
        if (exists2 != null)
        {
            FakeFileSystem.File.Create(exists2);
        }
        var outDest = string.Empty;
        MockDownloadManager.Setup(x =>
                x.DownloadAsync(It.IsAny<StreamQueryInfo>(), It.IsAny<string>(), It.IsAny<DownloadTaskEventHandler>()))
            .Returns<StreamQueryInfo, string, DownloadTaskEventHandler>((_, dest, _) =>
            {
                outDest = dest;
                return Task.FromResult(DownloadStatus.Success);
            });

        Model.Query.ExecuteIfCan();
        Model.Download.ExecuteIfCan();

        Output.WriteLine(outDest);
        Assert.Equal(expectedDest, outDest);
    }
}

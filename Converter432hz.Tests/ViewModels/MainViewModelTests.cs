using System.ComponentModel;
using System.Linq;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.Converter432hz.Tests.ViewModels;

public class MainViewModelTests : TestsBase
{
    public MainViewModelTests(ITestOutputHelper output) : base(output)
    {
    }

    public MainViewModel Model => _model ??=
        new MainViewModel(MockSettingsProvider, Encoder, DialogService, FileSystem,
            FileLocator, MockAppPath.Object, new FakeEnvironmentService(), MockPitchDetector.Object);
    private MainViewModel _model;

    public IEncoderService Encoder =>
        _encoder ??= new EncoderService(FileSystem, DialogService, MockAudioEncoder.Object, new FakeDispatcher(), MockSettingsProvider);
    private IEncoderService _encoder;

    public IFileLocator FileLocator => _fileLocator ??= new FileLocator(MockAppPath.Object, FileSystem);
    private IFileLocator _fileLocator;

    public AppSettingsData AppSettings { get; set; } = new();

    public EncodeSettings EncodeSettings { get; set; } = new();

    public Mock<IAudioEncoder> MockAudioEncoder => _mockAudioEncoder ??= new Mock<IAudioEncoder>();
    private Mock<IAudioEncoder> _mockAudioEncoder;

    public Mock<IAppPathService> MockAppPath => _mockAppPath ??= InitMock<IAppPathService>(mock =>
    {
        mock.Setup(x => x.AudioExtensions).Returns(new[] { "mp3" });
    });
    private Mock<IAppPathService> _mockAppPath;

    public ISettingsProvider<AppSettingsData> MockSettingsProvider => _mockSettingsProvider ??=
        Mock.Of<ISettingsProvider<AppSettingsData>>(x => x.Value == AppSettings);
    private ISettingsProvider<AppSettingsData> _mockSettingsProvider;

    public IDialogService DialogService => _dialogService ??= new DialogService(MockDialogManager.Object);
    private IDialogService _dialogService;

    public Mock<IDialogManager> MockDialogManager => _mockDialogManager ??= new Mock<IDialogManager>();
    private Mock<IDialogManager> _mockDialogManager;

    public IFileSystemService FileSystem => _fileSystem ??= new FakeFileSystemService();
    private IFileSystemService _fileSystem;

    public Mock<IPitchDetector> MockPitchDetector => _mockPitchDetector ??= Init(() =>
    {
        var mock = new Mock<IPitchDetector>();
        mock.Setup(x => x.GetPitchAsync(It.IsAny<string>())).Returns(Task.FromResult(440f));
        return mock;
    });
    private Mock<IPitchDetector> _mockPitchDetector;

    protected void SetOpenFilesResult(string[] values) =>
        MockDialogManager.Setup(x => x.ShowFrameworkDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<OpenFileDialogSettings>(),
                It.IsAny<AppDialogSettingsBase>(), It.IsAny<Func<object, string>>()))
            .Returns(() => Task.FromResult<object>(values));
    
    protected void SetOpenFolderResult(string value) =>
        MockDialogManager.Setup(x => x.ShowFrameworkDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<OpenFolderDialogSettings>(),
                It.IsAny<AppDialogSettingsBase>(), It.IsAny<Func<object, string>>()))
            .Returns(() => Task.FromResult<object>(value));

    [Fact]
    public void FileExistsAction_SetOnVM_SetOnEncoder()
    {
        Model.FileExistsActionList.SelectedValue = FileExistsAction.Rename;

        Assert.Equal(FileExistsAction.Rename, Model.Encoder.FileExistsAction);
    }

    [Fact]
    public void FileExistsAction_SetOnEncoder_SetOnVM()
    {
        Model.Encoder.FileExistsAction = FileExistsAction.Rename;

        Assert.Equal(FileExistsAction.Rename, Model.FileExistsActionList.SelectedValue);
    }

    [Fact]
    public void FileExistsAction_SetByIndex_SetOnEncoder()
    {
        Model.FileExistsActionList.CurrentPosition = 1;

        Assert.Equal(Model.FileExistsActionList.Source[1].Value, Model.Encoder.FileExistsAction);
    }

    [Theory]
    [InlineData(EncodeFormat.Flac, false)]
    [InlineData(EncodeFormat.Mp3, true)]
    [InlineData(EncodeFormat.Ogg, true)]
    [InlineData(EncodeFormat.Opus, true)]
    [InlineData(EncodeFormat.Wav, false)]
    public void IsBitrateVisible_SetFormat_IsExpectedValue(EncodeFormat format, bool value)
    {
        Model.FormatsList.SelectedValue = format;

        Assert.Equal(Model.IsBitrateVisible, value);
    }

    [Theory]
    [InlineData(EncodeFormat.Flac, true)]
    [InlineData(EncodeFormat.Mp3, true)]
    [InlineData(EncodeFormat.Ogg, true)]
    [InlineData(EncodeFormat.Opus, false)]
    [InlineData(EncodeFormat.Wav, true)]
    public void IsSampleRateVisible_SetFormat_IsExpectedValue(EncodeFormat format, bool value)
    {
        Model.FormatsList.SelectedValue = format;

        Assert.Equal(Model.IsSampleRateVisible, value);
    }

    [Theory]
    [InlineData(EncodeFormat.Flac, true)]
    [InlineData(EncodeFormat.Mp3, true)]
    [InlineData(EncodeFormat.Ogg, false)]
    [InlineData(EncodeFormat.Opus, false)]
    [InlineData(EncodeFormat.Wav, false)]
    public void IsQualitySpeedVisible_SetFormat_IsExpectedValue(EncodeFormat format, bool value)
    {
        Model.FormatsList.SelectedValue = format;

        Assert.Equal(Model.IsQualitySpeedVisible, value);
    }

    [Fact]
    public void AddFiles_Execute_ShowDialog()
    {
        SetOpenFilesResult(Array.Empty<string>());

        Model.AddFiles.Execute().Subscribe();

        MockDialogManager.Verify(
            x => x.ShowFrameworkDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<OpenFileDialogSettings>(),
                It.IsAny<AppDialogSettingsBase>(), It.IsAny<Func<object, string>>()),
            Times.Once);
        Assert.Empty(Model.Encoder.Sources);
    }
    
    [Fact]
    public void AddFiles_TwoValidFiles_AddFilesToSource()
    {
        var file1 = "/file1.mp3";
        var file2 = "/file2.mp3";
        FileSystem.File.WriteAllText(file1, string.Empty);
        FileSystem.File.WriteAllText(file2, string.Empty);
        SetOpenFilesResult(new[] { file1, file2 });

        Model.AddFiles.Execute().Subscribe();

        Assert.Equal(2, Model.Encoder.Sources.Count);
        Assert.Equal(file1, Encoder.Sources[0].Path);
        Assert.Equal(file2, Encoder.Sources[1].Path);
    }
    
    [Fact]
    public void AddFiles_OneValidOneInvalid_AddValidFileToSource()
    {
        var file1 = "/folder";
        var file2 = "/file1.mp3";
        FileSystem.File.WriteAllText(file2, string.Empty);
        SetOpenFilesResult(new[] { file1, file2 });

        Model.AddFiles.Execute().Subscribe();

        Assert.Single(Model.Encoder.Sources);
        Assert.Equal(file2, Encoder.Sources[0].Path);
    }
    
    [Fact]
    public void AddFiles_GetPitchException_DoNotThrow()
    {
        var file1 = "/file1.mp3";
        FileSystem.File.WriteAllText(file1, string.Empty);
        SetOpenFilesResult(new[] { file1 });
        MockPitchDetector.Setup(x => x.GetPitchAsync(It.IsAny<string>()))
            .Returns(Task.FromException<float>(new Exception()));

        Model.AddFiles.Execute().Subscribe();

        Assert.Single(Model.Encoder.Sources);
        Assert.Equal(file1, Encoder.Sources[0].Path);
    }
    
    [Fact]
    public void AddFolder_Execute_ShowDialog()
    {
        SetOpenFolderResult(null);

        Model.AddFolder.Execute().Subscribe();

        MockDialogManager.Verify(
            x => x.ShowFrameworkDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<OpenFolderDialogSettings>(),
                It.IsAny<AppDialogSettingsBase>(), It.IsAny<Func<object, string>>()),
            Times.Once);
        Assert.Empty(Model.Encoder.Sources);
    }
    
    [Fact]
    public void AddFolder_Valid_AddFilesInFolder()
    {
        var folder = "/folder";
        var file1 = "/folder/file1.mp3";
        var file2 = "/folder/sub/file2.mp3";
        FileSystem.Directory.CreateDirectory("/folder/sub");
        FileSystem.File.WriteAllText(file1, string.Empty);
        FileSystem.File.WriteAllText(file2, string.Empty);
        SetOpenFolderResult(folder);

        Model.AddFolder.Execute().Subscribe();

        Assert.Single(Model.Encoder.Sources);
        var f = (FolderItem)Encoder.Sources[0];
        Assert.Equal(file1, f.Files[0].Path);
        Assert.Equal(file2, f.Files[1].Path);
    }
    
    [Fact]
    public void RemoveFile_NoSelection_DoNothing()
    {
        var file1 = "/file1.mp3";
        Model.Encoder.Sources.Add(new FileItem(file1, file1));
        SetOpenFilesResult(new[] { file1 });
        Model.SourcesSelectedIndex = -1;

        Model.RemoveFile.Execute().Subscribe();

        Assert.Single(Model.Encoder.Sources);
    }

    [Fact]
    public void RemoveFile_Init_CanExecuteFalse()
    {
        Assert.False(Model.RemoveFile.CanExecute());
    }
    
    [Fact]
    public void RemoveFile_AddAndSelectItem_CanExecuteTrue()
    {
        var file1 = "/file1.mp3";
        Model.Encoder.Sources.Add(new FileItem(file1, file1));
        SetOpenFilesResult(new[] { file1 });
        Model.SourcesSelectedIndex = 0;

        Assert.True(Model.RemoveFile.CanExecute());
    }
    
    [Fact]
    public void RemoveFile_RemoveLast_CanExecuteFalse()
    {
        var file1 = "/file1.mp3";
        Model.Encoder.Sources.Add(new FileItem(file1, file1));
        SetOpenFilesResult(new[] { file1 });
        Model.SourcesSelectedIndex = 0;
        
        Model.RemoveFile.Execute().Subscribe();

        Assert.False(Model.RemoveFile.CanExecute());
    }
    
    [Fact]
    public void RemoveFile_RemoveLast_ListEmpty()
    {
        var file1 = "/file1.mp3";
        Model.Encoder.Sources.Add(new FileItem(file1, file1));
        SetOpenFilesResult(new[] { file1 });
        Model.SourcesSelectedIndex = 0;

        Model.RemoveFile.Execute().Subscribe();

        Assert.Empty(Model.Encoder.Sources);
        Assert.Equal(-1, Model.SourcesSelectedIndex);
    }
    
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    public void RemoveFile_Selection_RemoveAndSetSelection(int selection, int newSelection)
    {
        var files = new[] { "/file1.mp3", "/file2.mp3", "/file3.mp3" };
        files.ToList().ForEach(x => Model.Encoder.Sources.Add(new FileItem(x, x)));
        SetOpenFilesResult(files);
        Model.SourcesSelectedIndex = selection;

        Model.RemoveFile.Execute().Subscribe();

        Assert.Equal(2, Model.Encoder.Sources.Count);
        Assert.Equal(newSelection, Model.SourcesSelectedIndex);
    }

    [Fact]
    public void BrowseDestination_Execute_ShowDialog()
    {
        SetOpenFilesResult(Array.Empty<string>());
        var dest = "/dest";
        Model.Encoder.Destination = dest;
        SetOpenFolderResult(null);

        Model.BrowseDestination.Execute().Subscribe();

        MockDialogManager.Verify(
            x => x.ShowFrameworkDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<OpenFolderDialogSettings>(),
                It.IsAny<AppDialogSettingsBase>(), It.IsAny<Func<object, string>>()),
            Times.Once);
        Assert.Equal(dest, Model.Encoder.Destination);
    }
    
    [Fact]
    public void BrowseDestination_Select_SetDestination()
    {
        var folder = "/output";
        SetOpenFolderResult(folder);

        Model.BrowseDestination.Execute().Subscribe();

        Assert.Equal(folder, Model.Encoder.Destination);
    }

    // [Fact]
    // public void FilesLeft_AddFile_Returns1()
    // {
    //     var file1 = "/file1.mp3";
    //
    //     Model.Encoder.Sources.Add(new FileItem(file1, file1));
    //     
    //     Assert.Equal(1, Model.FilesLeft);
    // }
}

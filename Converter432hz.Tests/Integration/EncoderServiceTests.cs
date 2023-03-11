using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using LazyCache;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.Converter432hz.Tests.Integration;

public class EncoderServiceTests : TestsBase
{
    public EncoderServiceTests(ITestOutputHelper output) : base(output)
    { }

    public EncoderService Model => _model ??= new EncoderService(FileSystem, DialogService, BassEncoder, new FakeDispatcher(), MockSettingsProvider);
    private EncoderService _model;
    
    public AppSettingsData AppSettings { get; set; } = new();
    
    public ISettingsProvider<AppSettingsData> MockSettingsProvider => _mockSettingsProvider ??=
        Mock.Of<ISettingsProvider<AppSettingsData>>(x => x.Value == AppSettings);
    private ISettingsProvider<AppSettingsData> _mockSettingsProvider;
    
    public IAudioEncoder BassEncoder => _bassEncoder ??= new AudioEncoder(PitchDetector, FileSystem);
    private IAudioEncoder _bassEncoder;

    public IPitchDetector PitchDetector => _pitchDetector ??= new PitchDetector(FileSystem);
    private IPitchDetector _pitchDetector;

    public FileSystemService FileSystem => _fileSystem ??= new FileSystemService(new FileSystem(), new WindowsApiService());
    private FileSystemService _fileSystem;

    public DialogService DialogService =>
        _dialogService ??= new DialogService(MockDialogManager.Object, viewModelFactory: ViewModelFactory);
    private DialogService _dialogService;

    public Mock<IDialogManager> MockDialogManager => _mockDialogManager ??= new Mock<IDialogManager>();
    private Mock<IDialogManager> _mockDialogManager;
    
    private object ViewModelFactory(Type type) => (type) switch
    {
        _ when type == typeof(AskFileActionViewModel) => new AskFileActionViewModel(),
        _ => null
    };
    
    
    [Fact]
    public async Task RunAsync_CancelFirstFile_ProcessedOneFile()
    {
        Model.Sources.Add(new FileItem("SourceLong.mp3", "SourceLong.mp3"));
        Model.Sources.Add(new FileItem("SourceShort.mp3", "SourceShort.mp3"));
        Model.Destination = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location)!, "out");
        if (!Directory.Exists(Model.Destination))
        {
            Directory.CreateDirectory(Model.Destination);
        }
        Model.FileExistsAction = FileExistsAction.Overwrite;
    
        var t1 = Model.RunAsync();
        Model.Cancel();
        await t1;
    
        Assert.Single(Model.ProcessingFiles);
        Assert.Equal(EncodeStatus.Cancelled, Model.ProcessingFiles.First().Status);
    }
    
    [Fact]
    public async Task RunAsync_CancelFirstFolder_ProcessedOneFile()
    {
        var folder = new FolderItem("/test", "/test");
        folder.Files.Add(new FileItem("SourceLong.mp3", "SourceLong.mp3"));
        folder.Files.Add(new FileItem("SourceShort.mp3", "SourceShort.mp3"));
        Model.Sources.Add(folder);
        Model.Destination = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location)!, "out");
        Model.FileExistsAction = FileExistsAction.Overwrite;
    
        var t1 = Model.RunAsync();
        Model.Cancel();
        await t1;
    
        Assert.Single(Model.ProcessingFiles);
        Assert.Equal(EncodeStatus.Cancelled, Model.ProcessingFiles.First().Status);
    }
    
    [Fact]
    public async Task RunAsync_TwoFiles_TwoProcessedFiles()
    {
        Model.Sources.Add(new FileItem("SourceLong.mp3", "SourceLong.mp3"));
        Model.Sources.Add(new FileItem("SourceShort.mp3", "SourceShort.mp3"));
        Model.Destination = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly()!.Location)!, "out");
        Model.FileExistsAction = FileExistsAction.Overwrite;
    
        await Model.RunAsync();
    
        Assert.Equal(2, Model.ProcessingFiles.Count);
        Assert.Equal(EncodeStatus.Completed, Model.ProcessingFiles.ElementAt(0).Status);
        Assert.Equal(EncodeStatus.Completed, Model.ProcessingFiles.ElementAt(1).Status);
    }
}

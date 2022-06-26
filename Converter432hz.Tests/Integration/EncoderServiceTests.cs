using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using HanumanInstitute.Common.Avalonia.App.Tests;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Xunit.Abstractions;
// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.Converter432hz.Tests.Integration;

public class EncoderServiceTests
{
    public EncoderServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private readonly ITestOutputHelper _output;

    public EncoderService Model => _model ??= new EncoderService(FileSystem, DialogService, BassEncoder, new FakeDispatcher());
    private EncoderService _model;
    
    public IBassEncoder BassEncoder => _bassEncoder ??= new BassEncoder(FileSystem);
    private IBassEncoder _bassEncoder;

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

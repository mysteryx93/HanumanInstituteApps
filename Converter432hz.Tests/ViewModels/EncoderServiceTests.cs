using System.Linq;
using HanumanInstitute.Common.Avalonia.App.Tests;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Xunit.Abstractions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PossibleMultipleEnumeration
namespace HanumanInstitute.Converter432hz.Tests.ViewModels;

public class EncoderServiceTests
{
    public EncoderServiceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private readonly ITestOutputHelper _output;

    public EncoderService Model => _model ??= new EncoderService(FakeFileSystem, DialogService);
    private EncoderService _model;

    public FakeFileSystemService FakeFileSystem => _fakeFileSystem ??= new FakeFileSystemService();
    private FakeFileSystemService _fakeFileSystem;

    public IDialogService DialogService => _dialogService ??= new DialogService(MockDialogManager.Object);
    private IDialogService _dialogService;

    public Mock<IDialogManager> MockDialogManager => _mockDialogManager ??= new Mock<IDialogManager>();
    private Mock<IDialogManager> _mockDialogManager;

    private void OutputErrors(IEnumerable errors)
    {
        foreach (var item in errors)
        {
            _output.WriteLine(item.ToString());
        }
    }

    private FileItem AddSourceFile(int number = 1, FolderItem folder = null)
    {
        var result = new FileItem($"/File{number}.mp3", $"File{number}.mp3");
        if (folder == null)
        {
            Model.Sources.Add(result);
        }
        else
        {
            folder.Files.Add(result);
        }
        return result;
    }

    private FolderItem AddSourceFolder(int number = 1)
    {
        var result = new FolderItem($"/Folder{number}", $"Folder{number}");
        Model.Sources.Add(result);
        return result;
    }

    private void SetValidDestination(string path = "/Output")
    {
        FakeFileSystem.Directory.CreateDirectory(path);
        Model.Destination = path;
    }

    [Fact]
    public void GetErrors_SourceEmpty_Error()
    {
        var errors = Model.GetErrors(nameof(Model.Sources));

        OutputErrors(errors);
        Assert.Single(errors);
        Assert.NotEmpty(errors.Cast<string>().First());
    }

    [Fact]
    public void GetErrors_SourceAddAndRemove_Error()
    {
        AddSourceFile();
        Model.Sources.Clear();

        var errors = Model.GetErrors(nameof(Model.Sources));

        OutputErrors(errors);
        Assert.Single(errors);
        var error = errors.Cast<string>().First();
        Assert.NotEmpty(error);
    }

    [Fact]
    public void GetErrors_SourceFile_NoError()
    {
        AddSourceFile();

        var errors = Model.GetErrors(nameof(Model.Sources));

        OutputErrors(errors);
        Assert.Empty(errors);
    }

    [Fact]
    public void GetErrors_SourceFolder_NoError()
    {
        AddSourceFolder();

        var errors = Model.GetErrors(nameof(Model.Sources));

        OutputErrors(errors);
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetErrors_DestinationEmpty_Error(string value)
    {
        Model.Destination = value;

        var errors = Model.GetErrors(nameof(Model.Destination));

        OutputErrors(errors);
        Assert.Single(errors);
        var error = errors.Cast<string>().First();
        Assert.NotEmpty(error);
    }

    [Fact]
    public void GetErrors_DestinationDoesNotExist_Error()
    {
        Model.Destination = "/Output";

        var errors = Model.GetErrors(nameof(Model.Destination));

        OutputErrors(errors);
        Assert.Single(errors);
        var error = errors.Cast<string>().First();
        Assert.NotEmpty(error);
    }

    [Fact]
    public void GetErrors_DestinationExists_NoError()
    {
        SetValidDestination();

        var errors = Model.GetErrors(nameof(Model.Destination));

        OutputErrors(errors);
        Assert.Empty(errors);
    }

    [Fact]
    public void IsValid_MissingData_ReturnsFalse()
    {
        var result = Model.IsValidValue;

        Assert.False(result);
    }

    [Fact]
    public void IsValid_ValidData_ReturnsTrue()
    {
        AddSourceFile();
        SetValidDestination();

        var result = Model.IsValidValue;

        Assert.True(result);
    }

    [Fact]
    public async Task RunAsync_InvalidData_DoNothing()
    {
        await Model.RunAsync();
        
        Assert.Empty(Model.ProcessingFiles);
    }
    
    [Fact]
    public async Task RunAsync_SingleFile_CompleteFilesContainsOne()
    {
        var file1 = AddSourceFile();
        SetValidDestination();
        
        await Model.RunAsync();

        Assert.Single(Model.ProcessingFiles);
        Assert.Equal(_model.ProcessingFiles.First().Path, file1.Path);
    }
    
    [Fact]
    public async Task RunAsync_TwoFiles_CompleteFilesContainsTwo()
    {
        var file1 = AddSourceFile(1);
        var file2 = AddSourceFile(2);
        SetValidDestination();
        
        await Model.RunAsync();

        Assert.Equal(2, Model.ProcessingFiles.Count);
        Assert.Equal(_model.ProcessingFiles[0].Path, file1.Path);
        Assert.Equal(_model.ProcessingFiles[1].Path, file2.Path);
    }
    
    [Fact]
    public async Task RunAsync_RunTwice_ProcessOnce()
    {
        var file1 = AddSourceFile();
        SetValidDestination();
        
        var t1 = Model.RunAsync();
        await Model.RunAsync();
        await t1;

        Assert.Single(Model.ProcessingFiles);
    }

        
    [Fact]
    public async Task RunAsync_EmptyFolder_CompleteFilesEmpty()
    {
        var folder1 = AddSourceFolder(1);
        SetValidDestination();
        
        await Model.RunAsync();

        Assert.Empty(Model.ProcessingFiles);
    }
    
    [Fact]
    public async Task RunAsync_FolderSingleFile_CompleteFilesContainsOne()
    {
        var folder1 = AddSourceFolder(1);
        var file1 = AddSourceFile(1, folder1);
        SetValidDestination();
        
        await Model.RunAsync();

        Assert.Single(Model.ProcessingFiles);
        Assert.Equal(_model.ProcessingFiles[0].Path, file1.Path);
    }
    
    [Fact]
    public async Task RunAsync_FolderTwoFiles_CompleteFilesContainsTwo()
    {
        var folder1 = AddSourceFolder(1);
        var file1 = AddSourceFile(1, folder1);
        var file2 = AddSourceFile(2, folder1);
        SetValidDestination();
        
        await Model.RunAsync();

        Assert.Equal(2, Model.ProcessingFiles.Count);
        Assert.Equal(_model.ProcessingFiles[0].Path, file1.Path);
        Assert.Equal(_model.ProcessingFiles[1].Path, file2.Path);
    }
    
        
    [Fact]
    public async Task RunAsync_TwoFoldersWithSingleFile_CompleteFilesContainsTwo()
    {
        var folder1 = AddSourceFolder(1);
        var folder2 = AddSourceFolder(2);
        var file1 = AddSourceFile(1, folder1);
        var file2 = AddSourceFile(2, folder2);
        SetValidDestination();
        
        await Model.RunAsync();

        Assert.Equal(2, Model.ProcessingFiles.Count);
        Assert.Equal(_model.ProcessingFiles[0].Path, file1.Path);
        Assert.Equal(_model.ProcessingFiles[1].Path, file2.Path);
    }
}

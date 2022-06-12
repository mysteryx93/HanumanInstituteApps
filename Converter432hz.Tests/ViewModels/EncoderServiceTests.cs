using System.ComponentModel;
using System.Linq;
using System.Threading;
using DynamicData;
using HanumanInstitute.Common.Avalonia.App.Tests;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Xunit.Abstractions;
// ReSharper disable RedundantArgumentDefaultValue
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

    public EncoderService Model => _model ??= 
        new EncoderService(FakeFileSystem, DialogService, MockBassEncoder.Object, new FakeDispatcher())
        {
            Owner = Mock.Of<INotifyPropertyChanged>(),
            DelayBeforeStart = 5
        };
    private EncoderService _model;

    public Mock<IBassEncoder> MockBassEncoder => _mockBassEncoder ??= SetupMockBassEncoder();
    private Mock<IBassEncoder> _mockBassEncoder;
    private Mock<IBassEncoder> SetupMockBassEncoder()
    {
        var result = new Mock<IBassEncoder>();
        result.Setup(x => x.StartAsync(It.IsAny<ProcessingItem>(), It.IsAny<EncodeSettings>(), It.IsAny<CancellationToken>()))
            .Returns<ProcessingItem, EncodeSettings, CancellationToken>(async (file, _, token) =>
            {
                _output.WriteLine("Start");
                await _fakeFileSystem.File.WriteAllTextAsync(file.Destination, "file content", token);
                await Task.Delay(100, token);
                _output.WriteLine("Callback");
                file.Status = token.IsCancellationRequested ? EncodeStatus.Cancelled : EncodeStatus.Completed;
            });
        return result;
    }

    public FakeFileSystemService FakeFileSystem => _fakeFileSystem ??= new FakeFileSystemService();
    private FakeFileSystemService _fakeFileSystem;

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

    private async Task CreateFileAsync(string relativePath)
    {
        await FakeFileSystem.File.WriteAllTextAsync(FakeFileSystem.Path.Combine(Model.Destination, relativePath), string.Empty);
    }
    
    // [Fact]
    // public void GetErrors_SourceEmpty_Error()
    // {
    //     var errors = Model.GetErrors(nameof(Model.Sources));
    //
    //     OutputErrors(errors);
    //     Assert.Single(errors);
    //     Assert.NotEmpty(errors.Cast<string>().First());
    // }
    //
    // [Fact]
    // public void GetErrors_SourceAddAndRemove_Error()
    // {
    //     AddSourceFile();
    //     Model.Sources.Clear();
    //
    //     var errors = Model.GetErrors(nameof(Model.Sources));
    //
    //     OutputErrors(errors);
    //     Assert.Single(errors);
    //     var error = errors.Cast<string>().First();
    //     Assert.NotEmpty(error);
    // }
    //
    // [Fact]
    // public void GetErrors_SourceFile_NoError()
    // {
    //     AddSourceFile();
    //
    //     var errors = Model.GetErrors(nameof(Model.Sources));
    //
    //     OutputErrors(errors);
    //     Assert.Empty(errors);
    // }
    //
    // [Fact]
    // public void GetErrors_SourceFolder_NoError()
    // {
    //     AddSourceFolder();
    //
    //     var errors = Model.GetErrors(nameof(Model.Sources));
    //
    //     OutputErrors(errors);
    //     Assert.Empty(errors);
    // }
    //
    // [Theory]
    // [InlineData(null)]
    // [InlineData("")]
    // public void GetErrors_DestinationEmpty_Error(string value)
    // {
    //     Model.Destination = value;
    //
    //     var errors = Model.GetErrors(nameof(Model.Destination));
    //
    //     OutputErrors(errors);
    //     Assert.Single(errors);
    //     var error = errors.Cast<string>().First();
    //     Assert.NotEmpty(error);
    // }
    //
    // [Fact]
    // public void GetErrors_DestinationDoesNotExist_Error()
    // {
    //     Model.Destination = "/Output";
    //
    //     var errors = Model.GetErrors(nameof(Model.Destination));
    //
    //     OutputErrors(errors);
    //     Assert.Single(errors);
    //     var error = errors.Cast<string>().First();
    //     Assert.NotEmpty(error);
    // }
    //
    // [Fact]
    // public void GetErrors_DestinationExists_NoError()
    // {
    //     SetValidDestination();
    //
    //     var errors = Model.GetErrors(nameof(Model.Destination));
    //
    //     OutputErrors(errors);
    //     Assert.Empty(errors);
    // }

    // [Fact]
    // public void IsValid_MissingData_ReturnsFalse()
    // {
    //     var result = Model.IsValidValue;
    //
    //     Assert.False(result);
    // }
    //
    // [Fact]
    // public void IsValid_ValidData_ReturnsTrue()
    // {
    //     AddSourceFile();
    //     SetValidDestination();
    //
    //     var result = Model.IsValidValue;
    //
    //     Assert.True(result);
    // }

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
        var proc = _model.ProcessingFiles.First();
        Assert.Equal(proc.Path, file1.Path);
        Assert.Equal(EncodeStatus.Completed, proc.Status);
        Assert.True(_fakeFileSystem.File.Exists(proc.Destination));
    }

    [Fact]
    public async Task RunAsync_TwoFiles_CompleteFilesContainsTwo()
    {
        var file1 = AddSourceFile(1);
        var file2 = AddSourceFile(2);
        SetValidDestination();

        await Model.RunAsync();

        Assert.Equal(2, Model.ProcessingFiles.Count);
        Assert.Equal(_model.ProcessingFiles.ElementAt(0).Path, file1.Path);
        Assert.Equal(_model.ProcessingFiles.ElementAt(1).Path, file2.Path);
    }

    [Fact]
    public async Task RunAsync_RunTwice_ProcessOnce()
    {
        AddSourceFile();
        SetValidDestination();

        var t1 = Model.RunAsync();
        await Model.RunAsync();
        await t1;

        Assert.Single(Model.ProcessingFiles);
    }


    [Fact]
    public async Task RunAsync_EmptyFolder_CompleteFilesEmpty()
    {
        AddSourceFolder(1);
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
        Assert.Equal(_model.ProcessingFiles.ElementAt(0).Path, file1.Path);
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
        Assert.Equal(_model.ProcessingFiles.ElementAt(0).Path, file1.Path);
        Assert.Equal(_model.ProcessingFiles.ElementAt(1).Path, file2.Path);
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
        Assert.Equal(_model.ProcessingFiles.ElementAt(0).Path, file1.Path);
        Assert.Equal(_model.ProcessingFiles.ElementAt(1).Path, file2.Path);
    }

    [Theory]
    [InlineData("/Output", "/File1.mp3")]
    [InlineData("/Output", "/MyDownloads/File2.mp3")]
    [InlineData("/Output/", "/File3")]
    [InlineData("/Output/", "/MyDownloads/File4")]
    public async Task RunAsync_SingleFile_DestinationSet(string dest, string filePath)
    {
        var file = new FileItem(filePath, FakeFileSystem.Path.GetFileName(filePath));
        Model.Sources.Add(file);
        SetValidDestination(dest);

        await Model.RunAsync();

        var output = FakeFileSystem.Path.Combine(dest, file.RelativePath);
        output = output.EndsWith(".mp3") ? output : output + ".mp3"; 
        Assert.Single(Model.ProcessingFiles);
        Assert.Equal(output, Model.ProcessingFiles.First().Destination);
    }
    
    [Theory]
    [InlineData("/File1.MP3", EncodeFormat.Mp3, "/File1.mp3")]
    [InlineData("/MyDownloads/File2.mp3", EncodeFormat.Flac, "/MyDownloads/File2.flac")]
    [InlineData("/File3", EncodeFormat.Ogg, "/File3.ogg")]
    [InlineData("/MyDownloads/File4", EncodeFormat.Opus, "/MyDownloads/File4.opus")]
    [InlineData("/File5.mp3", EncodeFormat.Wav, "/File5.wav")]
    public async Task RunAsync_FileAndFormat_DestinationHasNewExtension(string filePath, EncodeFormat format, string expectedDest)
    {
        var file = new FileItem(filePath, filePath);
        Model.Sources.Add(file);
        Model.Settings.Format = format;
        SetValidDestination("/");

        await Model.RunAsync();

        Assert.Single(Model.ProcessingFiles);
        Assert.Equal(expectedDest, Model.ProcessingFiles.First().Destination);
    }

    [Theory]
    [InlineData("/Output", "/Folder1", "File1.mp3")]
    [InlineData("/Output", "/Folder1/", "Sup/File2.mp3")]
    [InlineData("/Output/", "/Folder1/Folder2", "File3")]
    [InlineData("/Output/", "/Folder1/Folder2/", "Sup/File4")]
    public async Task RunAsync_FileInFolder_DestinationSet(string dest, string folderPath, string filePath)
    {
        var folder = new FolderItem(folderPath, FakeFileSystem.Path.GetFileName(folderPath.TrimEnd('/')));
        var file = new FileItem(FakeFileSystem.Path.Combine(folderPath, filePath),
            FakeFileSystem.Path.Combine(folder.RelativePath, filePath));
        folder.Files.Add(file);
        Model.Sources.Add(folder);
        SetValidDestination(dest);

        await Model.RunAsync();

        var output = FakeFileSystem.Path.Combine(dest, file.RelativePath);
        output = output.EndsWith(".mp3") ? output : output + ".mp3";
        Assert.Single(Model.ProcessingFiles);
        Assert.Equal(output, Model.ProcessingFiles.First().Destination);
    }

    [Fact]
    public async Task RunAsync_ActionCancel_StatusCancelled()
    {
        var file = AddSourceFile();
        SetValidDestination();
        await CreateFileAsync(file.RelativePath);
        Model.FileExistsAction = FileExistsAction.Cancel;

        await Model.RunAsync();

        Assert.Equal(EncodeStatus.Cancelled, Model.ProcessingFiles.First().Status);
    }

    [Fact]
    public async Task RunAsync_ActionCancel_DoNotProcessSecondFile()
    {
        var file = AddSourceFile(1);
        AddSourceFile(2);
        SetValidDestination();
        await CreateFileAsync(file.RelativePath);
        Model.FileExistsAction = FileExistsAction.Cancel;

        await Model.RunAsync();

        Assert.Single(Model.ProcessingFiles);
    }

    [Fact]
    public async Task RunAsync_ActionSkip_StatusSkip()
    {
        var file = AddSourceFile();
        SetValidDestination();
        await CreateFileAsync(file.RelativePath);
        Model.FileExistsAction = FileExistsAction.Skip;

        await Model.RunAsync();

        Assert.Equal(EncodeStatus.Skip, Model.ProcessingFiles.First().Status);
        Assert.Empty(Model.ProcessingFiles.First().Destination);
    }

    [Fact]
    public async Task RunAsync_ActionSkip_ProcessSecondFile()
    {
        var file = AddSourceFile(1);
        AddSourceFile(2);
        SetValidDestination();
        await CreateFileAsync(file.RelativePath);
        Model.FileExistsAction = FileExistsAction.Skip;

        await Model.RunAsync();

        Assert.Equal(2, Model.ProcessingFiles.Count);
    }

    [Fact]
    public async Task RunAsync_ActionRename_DestinationSet()
    {
        var output = "/Output";
        var file = new FileItem("/File1.mp3", "File1.mp3");
        Model.Sources.Add(file);
        SetValidDestination(output);
        await CreateFileAsync(file.RelativePath);
        Model.FileExistsAction = FileExistsAction.Rename;

        await Model.RunAsync();

        Assert.Equal("/Output/File1 (2).mp3", Model.ProcessingFiles.First().Destination);
    }

    [Fact]
    public async Task RunAsync_ActionRenameTwice_DestinationSet()
    {
        var output = "/Output";
        var file = new FileItem("/File1.mp3", "File1.mp3");
        Model.Sources.Add(file);
        SetValidDestination(output);
        await CreateFileAsync(file.RelativePath);
        await CreateFileAsync("File1 (2).mp3");
        Model.FileExistsAction = FileExistsAction.Rename;

        await Model.RunAsync();

        Assert.Equal("/Output/File1 (3).mp3", Model.ProcessingFiles.First().Destination);
    }

    [Fact]
    public async Task RunAsync_ActionOverwrite_DestinationSet()
    {
        var file = AddSourceFile();
        SetValidDestination();
        await CreateFileAsync(file.RelativePath);
        Model.FileExistsAction = FileExistsAction.Overwrite;

        await Model.RunAsync();

        Assert.Equal(FakeFileSystem.Path.Combine(Model.Destination, file.RelativePath), Model.ProcessingFiles.First().Destination);
    }

    [Fact]
    public async Task RunAsync_ActionAskCancel_CancelOperation()
    {
        var file = AddSourceFile();
        SetValidDestination();
        await CreateFileAsync(file.RelativePath);
        Model.FileExistsAction = FileExistsAction.Ask;
        MockDialogManager.Setup(x =>
                x.ShowDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<AskFileActionViewModel>()))
            .Callback((INotifyPropertyChanged _, IModalDialogViewModel viewModel) =>
            {
                var vm = (AskFileActionViewModel)viewModel;
                vm.Items.SelectedValue = FileExistsAction.Cancel;
                vm.DialogResult = true;
            });

        await Model.RunAsync();

        Assert.Equal(EncodeStatus.Cancelled, Model.ProcessingFiles.First().Status);
    }

    [Fact]
    public async Task RunAsync_ActionAskClose_SkipFile()
    {
        var file = AddSourceFile();
        SetValidDestination();
        await CreateFileAsync(file.RelativePath);
        Model.FileExistsAction = FileExistsAction.Ask;
        MockDialogManager.Setup(x =>
                x.ShowDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<AskFileActionViewModel>()))
            .Callback<INotifyPropertyChanged, IModalDialogViewModel>((_, viewModel) =>
            {
                var vm = (AskFileActionViewModel)viewModel;
                vm.Items.SelectedValue = FileExistsAction.Overwrite;
                vm.DialogResult = null;
            });

        await Model.RunAsync();

        Assert.Equal(EncodeStatus.Skip, Model.ProcessingFiles.First().Status);
    }

    [Fact]
    public async Task RunAsync_TwoFiles_BassEncoderCalledTwice()
    {
        AddSourceFile(1);
        AddSourceFile(2);
        SetValidDestination();

        await Model.RunAsync();

        MockBassEncoder.Verify(
            x => x.StartAsync(It.IsAny<ProcessingItem>(), It.IsAny<EncodeSettings>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task RunAsync_CancelFirst_DoNotProcessSecond()
    {
        AddSourceFile(1);
        AddSourceFile(2);
        SetValidDestination();

        var t1 = Model.RunAsync();
        Model.Cancel();
        await t1;

        Assert.Single(Model.ProcessingFiles);
        Assert.Equal(EncodeStatus.Cancelled, Model.ProcessingFiles.First().Status);
    }
    
    [Fact]
    public async Task RunAsync_Cancel_DeleteFile()
    {
        AddSourceFile(1);
        SetValidDestination();

        var t1 = Model.RunAsync();
        Model.Cancel();
        await t1;

        var proc = Model.ProcessingFiles.Single();
        Assert.False(_fakeFileSystem.File.Exists(proc.Destination));
    }
    
    [Fact]
    public async Task RunAsync_MultiThreaded_ProcessAll()
    {
        for (var i = 0; i < 20; i++)
        {
            AddSourceFile(i);
        }
        SetValidDestination();
        Model.Settings.MaxThreads = 8;

        await Model.RunAsync();

        Assert.Equal(20, Model.ProcessingFiles.Count);
        Assert.All(Model.ProcessingFiles, x =>
        {
            Assert.Equal(EncodeStatus.Completed, x.Status);
        });
    }
    
    [Fact]
    public async Task RunAsync_MultiThreaded_AskActionOnce()
    {
        SetValidDestination();
        for (var i = 0; i < 4; i++)
        {
            var file = AddSourceFile(i);
            await CreateFileAsync(file.RelativePath);
        }
        Model.Settings.MaxThreads = 8;
        Model.FileExistsAction = FileExistsAction.Ask;
        var callCount = 0;
        MockDialogManager.Setup(x =>
                x.ShowDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<AskFileActionViewModel>()))
            .Returns(async (INotifyPropertyChanged _, IModalDialogViewModel viewModel) =>
            {
                callCount++;     
                await Task.Delay(1000);
            });
        
        var t1 = Model.RunAsync();
        await Task.Delay(200);

        Assert.Equal(1, callCount);
        Model.Cancel();
    }
    
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RunAsync_MultiThreadedCancelFirst_DoNotAskAgain(bool applyToAll)
    {
        SetValidDestination();
        for (var i = 0; i < 14; i++)
        {
            var file = AddSourceFile(i);
            await CreateFileAsync(file.RelativePath);
        }
        Model.Settings.MaxThreads = 8;
        Model.FileExistsAction = FileExistsAction.Ask;
        var callCount = 0;
        MockDialogManager.Setup(x =>
                x.ShowDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<AskFileActionViewModel>()))
            .Returns(async (INotifyPropertyChanged _, IModalDialogViewModel viewModel) =>
            {
                callCount++;
                await Task.Delay(200);
                var vm = (AskFileActionViewModel)viewModel;
                vm.Items.SelectedValue = FileExistsAction.Cancel;
                vm.ApplyToAll = applyToAll;
                vm.DialogResult = true;
            });
        
        await Model.RunAsync();

        Assert.Equal(1, callCount);
        Assert.True(Model.ProcessingFiles.Count > 1);
        Assert.All(Model.ProcessingFiles, x =>
        {
            _output.WriteLine(x.Status.ToString());
            Assert.Equal(EncodeStatus.Cancelled, x.Status);
        });
    }
    
    [Fact]
    public async Task RunAsync_MultiThreadedCancel_DoNotDeleteExistingFiles()
    {
        SetValidDestination();
        for (var i = 0; i < 4; i++)
        {
            var file = AddSourceFile(i);
            await CreateFileAsync(file.RelativePath);
        }
        Model.Settings.MaxThreads = 8;
        Model.FileExistsAction = FileExistsAction.Ask;
        MockDialogManager.Setup(x =>
                x.ShowDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<AskFileActionViewModel>()))
            .Returns(async (INotifyPropertyChanged _, IModalDialogViewModel viewModel) =>
            {
                await Task.Delay(100);
                var vm = (AskFileActionViewModel)viewModel;
                vm.Items.SelectedValue = FileExistsAction.Cancel;
                vm.DialogResult = true;
            });
        
        await Model.RunAsync();

        for (var i = 0; i < 4; i++)
        {
            Assert.True(_fakeFileSystem.File.Exists($"/Output/File{i}.mp3"));
        }
    }
    
    [Fact]
    public async Task RunAsync_MultiThreadedSkipAll_DoNotAskAgain()
    {
        SetValidDestination();
        for (var i = 0; i < 4; i++)
        {
            var file = AddSourceFile(i);
            await CreateFileAsync(file.RelativePath);
        }
        Model.Settings.MaxThreads = 8;
        Model.FileExistsAction = FileExistsAction.Ask;
        var callCount = 0;
        MockDialogManager.Setup(x =>
                x.ShowDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<AskFileActionViewModel>()))
            .Returns(async (INotifyPropertyChanged _, IModalDialogViewModel viewModel) =>
            {
                callCount++;
                _output.WriteLine("ShowAskFileAction");
                await Task.Delay(100);
                var vm = (AskFileActionViewModel)viewModel;
                vm.Items.SelectedValue = FileExistsAction.Skip;
                vm.ApplyToAll = true;
                vm.DialogResult = true;
            });
        
        await Model.RunAsync();

        Assert.Equal(1, callCount);
        Assert.All(Model.ProcessingFiles, x =>
        {
            _output.WriteLine(x.Status.ToString());
            Assert.Equal(EncodeStatus.Skip, x.Status);
        });
    }
}

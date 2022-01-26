using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using HanumanInstitute.Common.Avalonia.App.Tests;
using HanumanInstitute.Common.Services;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.PowerliminalsPlayer.Business;
using HanumanInstitute.PowerliminalsPlayer.Models;
using HanumanInstitute.PowerliminalsPlayer.ViewModels;
using Moq;
using MvvmDialogs;
using MvvmDialogs.Avalonia;
using MvvmDialogs.DialogTypeLocators;
using MvvmDialogs.FrameworkDialogs;
using Xunit;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.PowerliminalsPlayer.Tests;

public class MainViewModelTests
{
    protected AppSettingsData Settings { get; set; } = new();

    protected Mock<ISettingsProvider<AppSettingsData>> MockAppSettings => _mockAppSettings ??= SetupAppSettings();
    private Mock<ISettingsProvider<AppSettingsData>> _mockAppSettings;
    private Mock<ISettingsProvider<AppSettingsData>> SetupAppSettings()
    {
        var mock = new Mock<ISettingsProvider<AppSettingsData>>();
        mock.Setup(x => x.Value).Returns(Settings);
        mock.Setup(x => x.Load()).Returns(Settings);
        return mock;
    }

    protected Mock<IDialogManager> MockDialogManager => _mockDialogManager ??= new Mock<IDialogManager>();
    private Mock<IDialogManager> _mockDialogManager;

    protected IDialogService DialogService => _dialogService ??= new DialogService(null, MockDialogManager.Object);
    private IDialogService _dialogService;

    protected IDictionary<string, MockFileData> Files { get; set; } = new Dictionary<string, MockFileData>();
    protected string CurrentDirectory { get; set; } = string.Empty;
    protected void AddFile(string fileName) => Files.Add(fileName.ReplaceDirectorySeparator(), MockFileData.NullObject);

    protected FakeFileSystemService MockFileSystem => _mockFileSystem ??= new FakeFileSystemService(Files, CurrentDirectory);
    private FakeFileSystemService _mockFileSystem;

    protected Mock<FakeFileSystemService> SetupMockFileSystem()
    {
        var mockFile = new Mock<FakeFileSystemService>() { CallBase = true };
        _mockFileSystem = mockFile.Object;
        return mockFile;
    }

    // ReSharper disable once PossibleUnintendedReferenceComparison
    protected IBassDevice MockBassDevice => _mockBassDevice ??= Mock.Of<IBassDevice>(x =>
        x.SupportedExtensions == new List<FileExtension>() { new FileExtension("Default", new[] { ".mp3", ".mp4" }) });
    private IBassDevice _mockBassDevice;

    protected IAppPathService AppPath => _appPath ??= new AppPathService(new FakeEnvironmentService(), MockFileSystem, MockBassDevice);
    private IAppPathService _appPath;

    protected MainViewModel Model => _model ??= new MainViewModel(AppPath, MockAppSettings.Object, MockFileSystem, DialogService);
    private MainViewModel _model;
    
    protected void SetDialogManagerOpenFolder(string result) =>
        MockDialogManager.Setup(x => x.ShowFrameworkDialogAsync<OpenFolderDialogSettings, string>(
                It.IsAny<INotifyPropertyChanged>(), It.IsAny<OpenFolderDialogSettings>(), It.IsAny<AppDialogSettingsBase>()))
            .Returns(Task.FromResult(result?.ReplaceDirectorySeparator()));


    [Fact]
    public void SearchText_ChangeOnce_DoNotCallFileSystemImmediately()
    {
        var mockFile = SetupMockFileSystem();
        Model.AppData.Folders.Add("D:\\");

        Model.SearchText = "text";

        mockFile.Verify(x => x.GetFilesByExtensions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<SearchOption>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("abc")]
    public async Task SearchText_ChangeOnce_CallFileSystemAfterDelay(string text)
    {
        var mockFile = SetupMockFileSystem();
        Model.AppData.Folders.Add("D:\\");

        Model.SearchText = text;

        await Task.Delay(220);
        mockFile.Verify(x => x.GetFilesByExtensions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<SearchOption>()), Times.Once);
    }

    [Fact]
    public async Task SearchText_ChangeMultipleTimes_CallFileSystemOnceAfterDelay()
    {
        var mockFile = SetupMockFileSystem();
        Model.AppData.Folders.Add("D:\\");

        Model.SearchText = "a";
        Model.SearchText = "";
        Model.SearchText = "b";

        await Task.Delay(220);
        mockFile.Verify(x => x.GetFilesByExtensions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<SearchOption>()), Times.Once);
    }

    [Theory]
    [InlineData("", 6)]
    [InlineData(null, 6)]
    [InlineData("aa", 2)]
    [InlineData("bb", 2)]
    [InlineData("abc", 2)]
    [InlineData("$", 0)]
    public async Task SearchText_SetText_LoadMatchingFiles(string text, int count)
    {
        MockFileSystem.EnsureDirectoryExists("/1/sub/".ReplaceDirectorySeparator());
        MockFileSystem.EnsureDirectoryExists("/2/sub/".ReplaceDirectorySeparator());
        MockFileSystem.File.Create("/1/aaa.mp3".ReplaceDirectorySeparator());
        MockFileSystem.File.Create("/1/bbb.mp3".ReplaceDirectorySeparator());
        MockFileSystem.File.Create("/1/sub/abc.mp3".ReplaceDirectorySeparator());
        MockFileSystem.File.Create("/2/aaa.mp3".ReplaceDirectorySeparator());
        MockFileSystem.File.Create("/2/bbb.mp3".ReplaceDirectorySeparator());
        MockFileSystem.File.Create("/2/sub/abc.mp3".ReplaceDirectorySeparator());
        Model.AppData.Folders.Add("/1".ReplaceDirectorySeparator());
        Model.AppData.Folders.Add("/2".ReplaceDirectorySeparator());

        Model.SearchText = text;

        await Task.Delay(220);
        Assert.Equal(count, Model.Files.Count());
    }

    [Fact]
    public async Task SearchText_Empty_IgnoreNonAudioFiles()
    {
        MockFileSystem.EnsureDirectoryExists("/1/".ReplaceDirectorySeparator());
        MockFileSystem.File.Create("/1/not_audio.dll".ReplaceDirectorySeparator());
        Model.AppData.Folders.Add("/1".ReplaceDirectorySeparator());

        Model.SearchText = "";

        await Task.Delay(220);
        Assert.Empty(Model.Files);
    }

    [Fact]
    public void Load_Default_CallSettingsLoad()
    {
        Model.AppData.Folders.Add("D:\\");

        Model.Load();

        _mockAppSettings.Verify(x => x.Load(), Times.Once);
    }

    [Fact]
    public void SaveSettingsCommand_Default_CallSettingsSave()
    {
        Model.AppData.Folders.Add("D:\\");

        Model.SaveSettingsCommand.ExecuteIfCan();

        _mockAppSettings.Verify(x => x.Save(), Times.Once);
    }

    [Fact]
    public void RemoveMediaCommand_OneOfTwo_RemoveFile()
    {
        var fileA = new FileItem("a", 1);
        Model.Playlist.Files.Add(fileA);
        var fileB = new FileItem("b", 1);
        Model.Playlist.Files.Add(fileB);

        Model.RemoveMediaCommand.ExecuteIfCan(fileB);

        Assert.Single(Model.Playlist.Files);
        Assert.Equal(fileA, Model.Playlist.Files.Single());
    }

    [Fact]
    public void RemoveMediaCommand_LastFile_PausedOff()
    {
        var file = new FileItem("a", 1);
        Model.Playlist.Files.Add(file);

        Model.RemoveMediaCommand.ExecuteIfCan(file);

        Assert.Empty(Model.Playlist.Files);
        Assert.False(Model.IsPaused);
    }

    [Fact]
    public void RemoveMediaCommand_Null_DoNothing()
    {
        var file = new FileItem("a", 1);
        Model.Playlist.Files.Add(file);

        Model.RemoveMediaCommand.Execute();

        Assert.Single(Model.Playlist.Files);
    }

    [Fact]
    public void RemoveMediaCommand_MissingItem_DoNothing()
    {
        var file = new FileItem("a", 1);

        Model.RemoveMediaCommand.Execute(file);

        Assert.Empty(Model.Playlist.Files);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddFolderCommand_NullOrEmpty_DoNotAddFolder(string folder)
    {
        SetDialogManagerOpenFolder(folder);

        Model.AddFolderCommand.Execute();

        Assert.Empty(Model.AppData.Folders);
    }

    [Theory]
    [InlineData("/NewFolder")]
    [InlineData("/NewFolder/")]
    public void AddFolderCommand_NewFolder_AddFolder(string folder)
    {
        SetDialogManagerOpenFolder(folder);

        Model.AddFolderCommand.ExecuteIfCan();

        Assert.Single(Model.AppData.Folders);
    }

    [Fact]
    public void AddFolderCommand_NewFolderWithEndSeparator_AddFolderWithoutEndSeparator()
    {
        SetDialogManagerOpenFolder("/NewFolder/");

        Model.AddFolderCommand.ExecuteIfCan();

        Assert.Single(Model.AppData.Folders);
        Assert.False(Model.AppData.Folders.Single().EndsWith(Path.DirectorySeparatorChar));
    }
    
    [Theory]
    [InlineData("/NewFolder")]
    [InlineData("/NewFolder/")]
    public void AddFolderCommand_NewFolder_LoadFolder(string folder)
    {
        var fileName = "file1.mp3";
        AddFile("/NewFolder/" + fileName);
        SetDialogManagerOpenFolder(folder);

        Model.AddFolderCommand.ExecuteIfCan();

        Assert.Single(Model.Files);
        Assert.EndsWith(fileName, Model.Files.Single());
    }
    
    [Theory]
    [InlineData("/NewFolder")]
    [InlineData("/NewFolder/$%*")]
    public void AddFolderCommand_InvalidFolder_AddFolderWithNoError(string folder)
    {
        SetDialogManagerOpenFolder(folder);

        Model.AddFolderCommand.ExecuteIfCan();

        Assert.Single(Model.AppData.Folders);
        Assert.Empty(Model.Files);
    }
    
    [Theory]
    [InlineData("/NewFolder", "/NewFolder")]
    [InlineData("/NewFolder", "/NewFolder/")]
    public void AddFolderCommand_Duplicate_DoNotAddDuplicate(string folder1, string folder2)
    {
        SetDialogManagerOpenFolder(folder1);
        Model.AddFolderCommand.Execute();
        SetDialogManagerOpenFolder(folder2);
        Model.AddFolderCommand.Execute();

        Assert.Single(Model.AppData.Folders);
        Assert.Equal(folder1.ReplaceDirectorySeparator(), Model.AppData.Folders.Single());
        Assert.Empty(Model.Files);
    }
    
    [Fact]
    public void AddFolderCommand_AddSubdirectory_DoNotLoadDuplicateFile()
    {
        AddFile("/Dir/Sub/File1.mp3");
        
        SetDialogManagerOpenFolder("/Dir");
        Model.AddFolderCommand.Execute();
        SetDialogManagerOpenFolder("/Dir/Sub");
        Model.AddFolderCommand.Execute();

        Assert.Single(Model.Files);
    }

    [Fact]
    public void AddFolderCommand_AddRoot_AddAndLoadRoot()
    {
        var root = "/".ReplaceDirectorySeparator();
        AddFile("/File1.mp3");
        
        SetDialogManagerOpenFolder(root);
        Model.AddFolderCommand.Execute();

        Assert.Single(Model.Files);
    }

    [Fact]
    public void LoadPresetCommand_NoPreset_CanExecuteFalse()
    {
        var result = Model.LoadPresetCommand.CanExecute();

        Assert.False(result);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public void LoadPresetCommand_NullOrFalse_DoNotLoadPreset(bool? dialogResult)
    {
        var presetName = "init";
        Model.Playlist.Name = presetName;
        Model.AppData.Presets.Add(new PresetItem(presetName));
        SelectPresetViewModel vm = null;
        MockDialogManager.Setup(x => x.ShowDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<IModalDialogViewModel>(), It.IsAny<Type>()))
            .Returns<INotifyPropertyChanged, INotifyPropertyChanged, Type>((_, viewModel, _) =>
            {
                vm = (SelectPresetViewModel)viewModel;
                vm.SelectedItem = Model.AppData.Presets.First();
                return Task.FromResult(dialogResult);
            });

        Model.LoadPresetCommand.Execute();

        Assert.Equal(presetName, Model.Playlist.Name);
        Assert.False(vm.ModeSave);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Loaded")]
    public void LoadPresetCommand_PresetSelected_LoadPresetName(string presetName)
    {
        Model.AppData.Presets.Add(new PresetItem(presetName));
        SelectPresetViewModel vm = null;
        MockDialogManager.Setup(x => x.ShowDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<IModalDialogViewModel>(), It.IsAny<Type>()))
            .Returns<INotifyPropertyChanged, INotifyPropertyChanged, Type>((_, viewModel, _) =>
            {
                vm = (SelectPresetViewModel)viewModel;
                vm.SelectedItem = Model.AppData.Presets.First();
                vm.DialogResult = true;
                return Task.FromResult<bool?>(true);
            });

        Model.LoadPresetCommand.ExecuteIfCan();

        Assert.Equal(presetName, Model.Playlist.Name);
        Assert.False(vm.ModeSave);
    }
}

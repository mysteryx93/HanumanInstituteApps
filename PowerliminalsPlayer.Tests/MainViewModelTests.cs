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
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.DialogTypeLocators;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HanumanInstitute.PowerliminalsPlayer.Business;
using HanumanInstitute.PowerliminalsPlayer.Models;
using HanumanInstitute.PowerliminalsPlayer.ViewModels;
using Moq;
using Xunit;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.PowerliminalsPlayer.Tests;

public class MainViewModelTests
{
    protected AppSettingsData Settings => MockAppSettings.Object.Value;

    protected Mock<FakeSettingsProvider<AppSettingsData>> MockAppSettings => _mockAppSettings ??= 
        new Mock<FakeSettingsProvider<AppSettingsData>>() { CallBase = true };
    private Mock<FakeSettingsProvider<AppSettingsData>> _mockAppSettings;

    protected Mock<IDialogManager> MockDialogManager => _mockDialogManager ??= new Mock<IDialogManager>();
    private Mock<IDialogManager> _mockDialogManager;

    protected IDialogService DialogService => _dialogService ??= new DialogService(MockDialogManager.Object);
    private IDialogService _dialogService;

    protected IDictionary<string, MockFileData> Files { get; set; } = new Dictionary<string, MockFileData>();
    protected string CurrentDirectory { get; set; } = string.Empty;
    
    protected void AddFile(string filePath) => Files.Add(
        new KeyValuePair<string, MockFileData>(filePath.ReplaceDirectorySeparator(), new MockFileData(string.Empty)));

    protected void AddFolder(string folder) =>
        Settings.Folders.Add(folder.ReplaceDirectorySeparator());

    protected void AddFilesAndLoad(int count)
    {
        var files = new string[count];
        for (var i = 0; i < count; i++)
        {
            files[i] = $"File{i + 1}.mp3";
        }
        AddFilesAndLoad(files);
    }
    protected void AddFilesAndLoad(IEnumerable<string> files)
    {
        foreach (var item in files)
        {
            AddFile("D:/" + item);            
        }
        AddFolder("D:/");
        Model.ReloadFiles();
    }

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

    protected IPathFixer PathFixer => _pathFixer ??= new AppPathFixer(MockFileSystem, DialogService, MockAppSettings.Object); 
    private IPathFixer _pathFixer;

    protected MainViewModel Model => _model ??= new MainViewModel(AppPath, MockAppSettings.Object, MockFileSystem, DialogService, PathFixer);
    private MainViewModel _model;
    
    protected void SetDialogManagerOpenFolder(string result) =>
        MockDialogManager.Setup(x => x.ShowFrameworkDialogAsync<OpenFolderDialogSettings, string>(
                It.IsAny<INotifyPropertyChanged>(), It.IsAny<OpenFolderDialogSettings>(), It.IsAny<AppDialogSettingsBase>()))
            .Returns(Task.FromResult(result?.ReplaceDirectorySeparator()));

    protected void SetDialogManagerLoadPreset(PresetItem result) =>
        MockDialogManager.Setup(x => x.ShowDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<IModalDialogViewModel>(), It.IsAny<Type>()))
            .Returns<INotifyPropertyChanged, IModalDialogViewModel, Type>((_, viewModel, _) =>
            {
                var vm = (SelectPresetViewModel)viewModel;
                vm.SelectedItem = result;
                vm.DialogResult = result != null;
                return Task.CompletedTask;
            });
    
    protected void SetDialogManagerSavePreset(string result) =>
        MockDialogManager.Setup(x => x.ShowDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<IModalDialogViewModel>(), It.IsAny<Type>()))
            .Returns<INotifyPropertyChanged, IModalDialogViewModel, Type>((_, viewModel, _) =>
            {
                var vm = (SelectPresetViewModel)viewModel;
                vm.PresetName = result;
                vm.DialogResult = result.HasValue();
                return Task.CompletedTask;
            });

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

        Model.LoadSettings();

        _mockAppSettings.Verify(x => x.Load(), Times.Once);
    }

    [Fact]
    public void SaveSettingsCommand_Default_CallSettingsSave()
    {
        Model.AppData.Folders.Add("D:\\");

        Model.SaveSettings();

        _mockAppSettings.Verify(x => x.Save(), Times.Once);
    }

    [Fact]
    public void RemoveMediaCommand_OneOfTwo_RemoveFile()
    {
        var fileA = new PlayingItem("a");
        Model.Playlist.Files.Add(fileA);
        var fileB = new PlayingItem("b");
        Model.Playlist.Files.Add(fileB);

        Model.RemoveMediaCommand.ExecuteIfCan(fileB);

        Assert.Single(Model.Playlist.Files);
        Assert.Equal(fileA, Model.Playlist.Files.Single());
    }

    [Fact]
    public void RemoveMediaCommand_LastFile_PausedOff()
    {
        var file = new PlayingItem("a");
        Model.Playlist.Files.Add(file);

        Model.RemoveMediaCommand.ExecuteIfCan(file);

        Assert.Empty(Model.Playlist.Files);
        Assert.False(Model.IsPaused);
    }

    [Fact]
    public void RemoveMediaCommand_Null_DoNothing()
    {
        var file = new PlayingItem("a");
        Model.Playlist.Files.Add(file);

        Model.RemoveMediaCommand.Execute();

        Assert.Single(Model.Playlist.Files);
    }

    [Fact]
    public void RemoveMediaCommand_MissingItem_DoNothing()
    {
        var file = new PlayingItem("a");

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
    public void AddFolderCommand_NewFolder_LoadFiles(string folder)
    {
        var fileName = "file1.mp3";
        AddFile("/NewFolder/" + fileName);
        SetDialogManagerOpenFolder(folder);

        Model.AddFolderCommand.ExecuteIfCan();

        Assert.Single(Model.Files);
        Assert.EndsWith(fileName, Model.Files.Single().FullPath);
    }
    
    [Theory]
    [InlineData("/NewFolder")]
    [InlineData("/NewFolder/")]
    public void AddFolderCommand_NewFolder_DisplayLoadedFileNames(string folder)
    {
        var fileName = "Dir/file1.mp3";
        AddFile("/NewFolder/" + fileName);
        SetDialogManagerOpenFolder(folder);

        Model.AddFolderCommand.ExecuteIfCan();

        Assert.Single(Model.Files);
        Assert.Equal(fileName, Model.Files.Single().Display);
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
    
    [Theory]
    [InlineData("/Dir", "/Sub/Dir")]
    [InlineData("/Sub/Dir", "/Dir")]
    public void AddFolderCommand_AddSubdirectory_DoNotLoadDuplicateFile(string dir1, string dir2)
    {
        AddFile("/Dir/Sub/File1.mp3");
        
        SetDialogManagerOpenFolder(dir1);
        Model.AddFolderCommand.Execute();
        SetDialogManagerOpenFolder(dir2);
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
    public void AddFolderCommand_Add_SelectNewItem()
    {
        Model.AppData.Folders.Add("/a");
        
        SetDialogManagerOpenFolder("/b");
        Model.AddFolderCommand.Execute();

        Assert.Equal(1, Model.SelectedFolderIndex);
    }

    [Fact]
    public void RemoveFolderCommand_EmptyList_DoNothing()
    {
        Model.RemoveFolderCommand.Execute();
        
        Assert.Empty(Model.AppData.Folders);
    }
    
    [Fact]
    public void RemoveFolderCommand_NoSelection_DoNothing()
    {
        Model.AppData.Folders.Add("/");
        Model.SelectedFolderIndex = -1;
        
        Model.RemoveFolderCommand.Execute();
        
        Assert.Single(Model.AppData.Folders);
    }

    [Fact]
    public void RemoveFolderCommand_UniqueSelection_RemoveItem()
    {
        Model.AppData.Folders.Add("/");
        Model.SelectedFolderIndex = 0;
        
        Model.RemoveFolderCommand.Execute();
        
        Assert.Empty(Model.AppData.Folders);
    }
    
    [Fact]
    public void RemoveFolderCommand_UniqueSelection_ResetSelection()
    {
        Model.AppData.Folders.Add("/");
        Model.SelectedFolderIndex = 0;
        
        Model.RemoveFolderCommand.Execute();
        
        Assert.Equal(-1, Model.SelectedFolderIndex);
    }
    
    [Theory]
    [InlineData(-1, -1)]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    public void RemoveFolderCommand_Selection_SelectOther(int selStart, int selNext)
    {
        Model.AppData.Folders.Add("/a");
        Model.AppData.Folders.Add("/b");
        Model.AppData.Folders.Add("/c");
        Model.SelectedFolderIndex = selStart;
        
        Model.RemoveFolderCommand.Execute();

        Assert.Equal(selNext, Model.SelectedFolderIndex);
    }

    [Fact]
    public void PlayCommand_NoSelection_DoNothing()
    {
        AddFilesAndLoad(1);
        Model.Files.CurrentPosition = -1;
        
        Model.PlayCommand.Execute();
        
        Assert.Empty(Model.Playlist.Files);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void PlayCommand_Selection_PlaySelection(int selectedIndex)
    {
        AddFilesAndLoad(2);
        Model.Files.CurrentPosition = selectedIndex;
        
        Model.PlayCommand.Execute();
        
        Assert.Single(Model.Playlist.Files);
        Assert.Equal(Model.Files.Source[selectedIndex].FullPath, Model.Playlist.Files.First().FullPath);
    }
    
    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(3, -1)]
    [InlineData(4, 2)]
    [InlineData(5, -2)]
    [InlineData(6, 3)]
    [InlineData(7, -3)]
    [InlineData(8, 4)]
    [InlineData(9, -4)]
    [InlineData(10, 0)]
    [InlineData(11, 1)]
    [InlineData(12, -1)]
    public void PlayCommand_Multiple_AutoAdjustSpeed(int times, int expectedSpeed)
    {
        AddFilesAndLoad(1);
        Model.Files.CurrentPosition = 0;
        
        for (var i = 0; i < times; i++)
        {
            Model.PlayCommand.Execute();
        }
        
        Assert.Equal(expectedSpeed, Model.Playlist.Files.Last().Speed);
    }
    
    [Theory]
    [InlineData("0", 1)]
    [InlineData("0,1", -1)]
    [InlineData("1,-1", 0)]
    [InlineData("0,1,-1,-2", 2)]
    [InlineData("0,1,-1,2,-2,3,4", -3)]
    [InlineData("2,1,0", -1)]
    [InlineData("2,1,0,-1", -2)]
    [InlineData("4,3,2,1,0,-1,-2,-3,-4", 0)]
    [InlineData("4,3,2,1,1,0,0,-1,-2,-3,-4", -1)]
    [InlineData("4,4,3,3,2,2,1,1,0,0,0,-1,-1,-2,-2,-3,-3,-4,-4", 1)]
    public void PlayCommand_AlreadyPlaying_SetUnusedSpeed(string fileSpeeds, int expectedSpeed)
    {
        AddFilesAndLoad(1);
        Model.Files.CurrentPosition = 0;
        var speedList = fileSpeeds.Split(',').Select(int.Parse);
        foreach (var speed in speedList)
        {
            Model.PlayCommand.Execute();
            Model.Playlist.Files.Last().Speed = speed;
        }
        
        Model.PlayCommand.Execute();
        
        Assert.Equal(expectedSpeed, Model.Playlist.Files.Last().Speed);
    }
    
    [Fact]
    public void PlayCommand_HasOtherFile_IgnoreForSpeedCalculation()
    {
        AddFilesAndLoad(2);
        Model.Files.CurrentPosition = 0;
        Model.PlayCommand.Execute();

        Model.Files.CurrentPosition = 1;
        Model.PlayCommand.Execute();
        
        Assert.Equal(0, Model.Playlist.Files.Last().Speed);
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
                vm.DialogResult = dialogResult;
                return Task.CompletedTask;
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
        var volume = 80;
        var preset = new PresetItem(presetName) { MasterVolume = 80};
        Model.AppData.Presets.Add(preset);
        SetDialogManagerLoadPreset(preset);

        Model.LoadPresetCommand.ExecuteIfCan();

        Assert.Equal(presetName, Model.Playlist.Name);
        Assert.Equal(volume, Model.Playlist.MasterVolume);
    }
    
    [Fact]
    public void LoadPresetCommand_PresetSelected_LoadPresetFiles()
    {
        var preset = new PresetItem("Preset");
        var volume = 80;
        preset.Files.Add(new PlayingItem("/File1", volume));
        preset.Files.Add(new PlayingItem("/File2", volume));
        Model.AppData.Presets.Add(preset);
        SetDialogManagerLoadPreset(preset);

        Model.LoadPresetCommand.ExecuteIfCan();

        Assert.NotEmpty(Model.Playlist.Files);
        Assert.True(Model.Playlist.Files.Count == 2);
        Assert.All(Model.Playlist.Files, x => Assert.True(x.IsPlaying));
        Assert.All(Model.Playlist.Files, x => Assert.Equal(volume, x.Volume));
    }
    
    [Fact]
    public async Task LoadPresetCommand_EditMasterVolume_EditPlaylistFileVolume()
    {
        var volume = 80;
        var preset = new PresetItem("a") { MasterVolume = volume };
        preset.Files.Add(new PlayingItem("/File1", volume));
        Model.AppData.Presets.Add(preset);
        SetDialogManagerLoadPreset(preset);

        var newVolume = 40;
        Model.LoadPresetCommand.ExecuteIfCan();
        Model.Playlist.MasterVolume = newVolume;

        await Task.Delay(120);
        Assert.Equal(newVolume, Model.Playlist.Files.First().Volume);
    }
    
    [Fact]
    public void LoadPresetCommand_EditMasterVolume_DoNotEditPreset()
    {
        var volume = 80;
        var preset = new PresetItem("a") { MasterVolume = volume };
        Model.AppData.Presets.Add(preset);
        SetDialogManagerLoadPreset(preset);

        Model.LoadPresetCommand.ExecuteIfCan();
        Model.Playlist.MasterVolume = 20;

        Assert.Equal(volume, Model.AppData.Presets.First().MasterVolume);
    }
    
    [Fact]
    public void LoadPresetCommand_EditFileVolume_DoNotEditPreset()
    {
        var volume = 80;
        var preset = new PresetItem("a");
        preset.Files.Add(new PlayingItem("/file1", volume));
        Model.AppData.Presets.Add(preset);
        SetDialogManagerLoadPreset(preset);

        Model.LoadPresetCommand.ExecuteIfCan();
        Model.Playlist.Files.First().Volume = 20;

        Assert.Equal(volume, Model.AppData.Presets.First().Files.First().Volume);
    }

    [Fact]
    public void SavedPresetCommand_NoFilePlaying_CanExecuteFalse()
    {
        var result = Model.SavePresetCommand.CanExecute();

        Assert.False(result);
    }
    
    [Fact]
    public void SavedPresetCommand_StopPlayingLastFile_CanExecuteFalse()
    {
        var playing = new PlayingItem("/File1");
        Model.Playlist.Files.Add(playing);
        
        Model.RemoveMediaCommand.Execute(playing);
        var result = Model.SavePresetCommand.CanExecute();

        Assert.False(result);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public void SavedPresetCommand_NullOrFalse_DoNotSavePreset(bool? dialogResult)
    {
        SelectPresetViewModel vm = null;
        MockDialogManager.Setup(x => x.ShowDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<IModalDialogViewModel>(), It.IsAny<Type>()))
            .Returns<INotifyPropertyChanged, INotifyPropertyChanged, Type>((_, viewModel, _) =>
            {
                vm = (SelectPresetViewModel)viewModel;
                vm.PresetName = "a";
                vm.DialogResult = dialogResult;
                return Task.CompletedTask;
            });

        Model.SavePresetCommand.Execute();

        Assert.Empty(Model.AppData.Presets);
        Assert.True(vm.ModeSave);
    }
    
    [Fact]
    public void SavePresetCommand_PresetSelected_SavePresetName()
    {
        var presetName = "Preset";
        var volume = 80;
        Model.Playlist.MasterVolume = volume;
        Model.Playlist.Files.Add(new PlayingItem("/File1"));
        SetDialogManagerSavePreset(presetName);

        Model.SavePresetCommand.ExecuteIfCan();

        Assert.Single(Model.AppData.Presets);
        Assert.Equal(presetName, Model.AppData.Presets.First().Name);
        Assert.Equal(volume, Model.AppData.Presets.First().MasterVolume);
    }
    
    [Fact]
    public void SavePresetCommand_PresetSelected_SavePresetFiles()
    {
        var presetName = "Preset";
        var volume = 80;
        Model.Playlist.Files.Add(new PlayingItem("/File1", volume));
        Model.Playlist.Files.Add(new PlayingItem("/File2", volume));
        SetDialogManagerSavePreset(presetName);

        Model.SavePresetCommand.ExecuteIfCan();

        Assert.Single(Model.AppData.Presets);
        Assert.True(Model.AppData.Presets.First().Files.Count == 2);
        Assert.All(Model.AppData.Presets.First().Files, x => Assert.Equal(volume, x.Volume));
    }
    
    [Fact]
    public void SavePresetCommand_ExistingPreset_OverwritePresetFiles()
    {
        var presetName = "Preset";
        Model.Playlist.Files.Add(new PlayingItem("/File1"));
        SetDialogManagerSavePreset(presetName);
        Model.SavePresetCommand.ExecuteIfCan();

        var newFile = "/File2";
        Model.Playlist.Files.Clear();
        Model.Playlist.Files.Add(new PlayingItem(newFile));
        SetDialogManagerSavePreset(presetName);
        Model.SavePresetCommand.ExecuteIfCan();

        Assert.Single(Model.AppData.Presets);
        Assert.Equal(newFile, Model.AppData.Presets.Single().Files.Single().FullPath);
    }
    
    [Fact]
    public async Task SavePresetCommand_EditMasterVolume_EditPlaylistFileVolume()
    {
        var presetName = "Preset";
        var volume = 80;
        Model.Playlist.Files.Add(new PlayingItem("/File1", volume));
        Model.Playlist.MasterVolume = volume;
        SetDialogManagerSavePreset(presetName);

        var newVolume = 40;
        Model.SavePresetCommand.ExecuteIfCan();
        Model.Playlist.MasterVolume = newVolume;

        await Task.Delay(120);
        Assert.Equal(newVolume, Model.Playlist.Files.First().Volume);
    }
    
    [Fact]
    public void SavePresetCommand_EditMasterVolume_DoNotEditPreset()
    {
        var presetName = "Preset";
        var volume = 80;
        Model.Playlist.Files.Add(new PlayingItem("/File1"));
        Model.Playlist.MasterVolume = volume;
        SetDialogManagerSavePreset(presetName);

        Model.SavePresetCommand.ExecuteIfCan();
        Model.Playlist.MasterVolume = 20;

        Assert.Equal(volume, Model.AppData.Presets.First().MasterVolume);
    }
    
    [Fact]
    public void SavePresetCommand_EditFileVolume_DoNotEditPreset()
    {
        var presetName = "Preset";
        var volume = 80;
        Model.Playlist.Files.Add(new PlayingItem("/File1", volume));
        Model.Playlist.Files.Add(new PlayingItem("/File2", volume));
        SetDialogManagerSavePreset(presetName);

        Model.SavePresetCommand.ExecuteIfCan();
        Model.Playlist.Files[0].Volume = 20;
        Model.Playlist.Files[1].Volume = 20;

        Assert.All(Model.AppData.Presets.First().Files, x => Assert.Equal(volume, x.Volume));
    }

    [Fact]
    public void PauseCommand_SomePlaying_PauseAll()
    {
        Model.Playlist.Files.Add(new PlayingItem("/file1.mp3"));
        Model.Playlist.Files.Add(new PlayingItem("/file2.mp3") { IsPlaying = true });
        
        Model.PauseCommand.ExecuteIfCan();

        Assert.True(Model.IsPaused);
        Assert.All(Model.Playlist.Files, x => Assert.False(x.IsPlaying));
    }
    
    [Fact]
    public void PauseCommand_SomePaused_PlayAll()
    {
        Model.Playlist.Files.Add(new PlayingItem("/file1.mp3") { IsPlaying = true });
        Model.Playlist.Files.Add(new PlayingItem("/file2.mp3"));
        Model.IsPaused = true;
        
        Model.PauseCommand.ExecuteIfCan();

        Assert.False(Model.IsPaused);
        Assert.All(Model.Playlist.Files, x => Assert.True(x.IsPlaying));
    }
}

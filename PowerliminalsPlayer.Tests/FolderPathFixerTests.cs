﻿using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using HanumanInstitute.Common.Avalonia.App.Tests;
using HanumanInstitute.Common.Services;
using HanumanInstitute.PowerliminalsPlayer.Business;
using HanumanInstitute.PowerliminalsPlayer.Models;
using Moq;
using MvvmDialogs;
using MvvmDialogs.Avalonia;
using MvvmDialogs.DialogTypeLocators;
using MvvmDialogs.FrameworkDialogs;
using Xunit;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.PowerliminalsPlayer.Tests;

public class FolderPathFixerTests
{
    protected AppSettingsData Settings => MockSettingsProvider.Value;

    protected ISettingsProvider<AppSettingsData> MockSettingsProvider => _mockSettingsProvider ??= new FakeSettingsProvider<AppSettingsData>();
    private ISettingsProvider<AppSettingsData> _mockSettingsProvider;

    protected Mock<IDialogManager> MockDialogManager => _mockDialogManager ??= new Mock<IDialogManager>();
    private Mock<IDialogManager> _mockDialogManager;

    protected IDialogService DialogService => _dialogService ??= new DialogService(null, MockDialogManager.Object);
    private IDialogService _dialogService;

    protected IDictionary<string, MockFileData> Files { get; set; } = new Dictionary<string, MockFileData>();

    protected IFileSystemService MockFileSystem => _mockFileSystem ??= new FakeFileSystemService(Files);
    private IFileSystemService _mockFileSystem;

    protected INotifyPropertyChanged Owner { get; set; } = Mock.Of<INotifyPropertyChanged>();

    protected IFolderPathFixer Model => _model ??= new FolderPathFixer(MockFileSystem, DialogService, MockSettingsProvider);
    private IFolderPathFixer _model;

    protected void AddFile(string filePath) => Files.Add(
        new KeyValuePair<string, MockFileData>(filePath.ReplaceDirectorySeparator(), MockFileData.NullObject));

    protected void AddFolder(string folder) =>
        Settings.Folders.Add(folder.ReplaceDirectorySeparator());

    protected void AddPreset(string presetName = "Preset") =>
        Settings.Presets.Add(new PresetItem(presetName));

    protected void AddPresetFile(string filePath) =>
        Settings.Presets[0].Files.Add(new FileItem(filePath.ReplaceDirectorySeparator()));

    protected void SetMessageBoxResult(bool? result)
    {
        MockDialogManager.Setup(x =>
                x.ShowFrameworkDialogAsync<MessageBoxSettings, bool?>(Owner, It.IsAny<MessageBoxSettings>(), It.IsAny<AppDialogSettingsBase>()))
            .ReturnsAsync(result);
    }

    protected void SetOpenFolderResult(string result, bool? nextPromptResult = null)
    {
        MockDialogManager.Setup(x =>
                x.ShowFrameworkDialogAsync<OpenFolderDialogSettings, string>(Owner, It.IsAny<OpenFolderDialogSettings>(),
                    It.IsAny<AppDialogSettingsBase>()))
            .ReturnsAsync(result)
            .Callback(() => SetMessageBoxResult(nextPromptResult));
    }

    protected void VerifyMessageBox(Times times)
    {
        MockDialogManager.Verify(x => x.ShowFrameworkDialogAsync<MessageBoxSettings, bool?>(
            Owner, It.IsAny<MessageBoxSettings>(), It.IsAny<AppDialogSettingsBase>()), times);
    }

    protected void VerifyOpenFolder(Times times)
    {
        MockDialogManager.Verify(x => x.ShowFrameworkDialogAsync<OpenFolderDialogSettings, string>(
            Owner, It.IsAny<OpenFolderDialogSettings>(), It.IsAny<AppDialogSettingsBase>()), times);
    }

    [Fact]
    public async Task PromptFixPaths_NoFolder_DoNotPrompt()
    {
        await Model.PromptFixPathsAsync(Owner);

        VerifyMessageBox(Times.Never());
        MockDialogManager.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("/Dir")]
    [InlineData("/Dir/Sub")]
    [InlineData("/Dir/")]
    [InlineData("/Dir/Sub/")]
    public async Task PromptFixPaths_FoldersValid_DoNotPrompt(string folder)
    {
        AddFile("/Dir/Sub/file.mp3");
        AddFolder(folder);

        await Model.PromptFixPathsAsync(Owner);

        VerifyMessageBox(Times.Never());
        MockDialogManager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PromptFixPaths_InvalidFolder_Prompt()
    {
        AddFolder("/Invalid");

        await Model.PromptFixPathsAsync(Owner);

        VerifyMessageBox(Times.Once());
        MockDialogManager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PromptFixPaths_AnswerYes_PromptFolder()
    {
        AddFolder("/Invalid");
        SetMessageBoxResult(true);

        await Model.PromptFixPathsAsync(Owner);

        VerifyOpenFolder(Times.Once());
    }

    [Fact]
    public async Task PromptFixPaths_BrowseInvalid_PromptAgain()
    {
        AddFolder("/Invalid");
        SetMessageBoxResult(true);
        SetOpenFolderResult("/Nope".ReplaceDirectorySeparator());

        await Model.PromptFixPathsAsync(Owner);

        VerifyOpenFolder(Times.Once());
        VerifyMessageBox(Times.Exactly(2));
        MockDialogManager.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("/New")]
    [InlineData("/New/")]
    [InlineData("/New/Sub")]
    [InlineData("/New/Sub/")]
    public async Task PromptFixPaths_BrowseValid_ReplaceFolder(string selectPath)
    {
        AddFile("/New/Sub/File1.mp3");
        AddFolder("/Old/Sub");
        SetMessageBoxResult(true);
        SetOpenFolderResult(selectPath.ReplaceDirectorySeparator());

        await Model.PromptFixPathsAsync(Owner);

        Assert.Equal("/New/Sub".ReplaceDirectorySeparator(), Settings.Folders[0]);
        VerifyMessageBox(Times.Once());
    }

    [Theory]
    [InlineData("/New")]
    [InlineData("/New/")]
    [InlineData("/New/Sub")]
    [InlineData("/New/Sub/")]
    public async Task PromptFixPaths_BrowseValid_ReplaceOtherFolder(string selectPath)
    {
        AddFile("/New/Sub/File1.mp3");
        AddFile("/New/Music/File2.mp3");
        AddFolder("/Old/Sub");
        AddFolder("/Old/Music");
        SetMessageBoxResult(true);
        SetOpenFolderResult(selectPath.ReplaceDirectorySeparator());

        await Model.PromptFixPathsAsync(Owner);

        Assert.Equal("/New/Sub".ReplaceDirectorySeparator(), Settings.Folders[0]);
        Assert.Equal("/New/Music".ReplaceDirectorySeparator(), Settings.Folders[1]);
        VerifyMessageBox(Times.Once());
    }

    [Fact]
    public async Task PromptFixPaths_ReplaceDone_PromptOtherInvalidFolder()
    {
        AddFile("/New/Sub/File1.mp3");
        AddFolder("/Old/Sub");
        AddFolder("/Older/Dir");
        SetMessageBoxResult(true);
        SetOpenFolderResult("/New".ReplaceDirectorySeparator());

        await Model.PromptFixPathsAsync(Owner);

        VerifyOpenFolder(Times.Once());
        VerifyMessageBox(Times.Exactly(2));
        MockDialogManager.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("/Old/Sub/File1.mp3", "/New/Sub/File1.mp3")]
    [InlineData("/Old/Sub/NoFile.mp3", "/New/Sub/NoFile.mp3")]
    [InlineData("/Old/Other/NoFile.mp3", "/New/Other/NoFile.mp3")]
    public async Task PromptFixPaths_BrowseValid_ReplacePresetFile(string presetFilePath, string expected)
    {
        AddFile("/New/Sub/File1.mp3");
        AddFolder("/Old/Sub");
        AddPreset();
        AddPresetFile(presetFilePath);
        SetMessageBoxResult(true);
        SetOpenFolderResult("/New/Sub".ReplaceDirectorySeparator());

        await Model.PromptFixPathsAsync(Owner);

        Assert.Equal(expected.ReplaceDirectorySeparator(), Settings.Presets[0].Files[0].FullPath);
    }
    
    [Theory]
    [InlineData("/Backup/Home/Music/INNA/Party", "/Sync/Music/INNA/Party", "/Backup/Home/Music/Other.mp3", "/Sync/Music/Other.mp3")]
    [InlineData("/Backup/Music/INNA/Party", "/Music/INNA/Party", "/Backup/Music/Other.mp3", "/Music/Other.mp3")]
    [InlineData("/Backup/Music/INNA/Party", "/Mus/INNA/Party", "/Backup/Music/INNA/End/Wow.mp3", "/Mus/INNA/End/Wow.mp3")]
    [InlineData("/Backup/Music/INNA/Party", "/Mus/INNA/Party", "/Backup/Music/Other.mp3", "/Mus/Other.mp3")]
    [InlineData("/Backup/Music/INNA/Party", "/Mus/INNA/Party", "/Music/Other.mp3", "/Music/Other.mp3")]
    public async Task PromptFixPaths_BrowseValid_ReplacePresetFileComplex(string oldFolder, string newFolder, string oldFile, string newFile)
    {
        newFile = newFile.ReplaceDirectorySeparator();
        AddFile(newFile);
        AddFolder(oldFolder);
        AddPreset();
        AddPresetFile(oldFile);
        SetMessageBoxResult(true);
        SetOpenFolderResult(newFolder.ReplaceDirectorySeparator());

        await Model.PromptFixPathsAsync(Owner);

        Assert.Equal(newFile, Settings.Presets[0].Files[0].FullPath);
    }
}

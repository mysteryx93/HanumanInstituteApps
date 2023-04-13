using System.ComponentModel;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.FileSystem;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HanumanInstitute.Services;

// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.PowerliminalsPlayer.Tests;

public class AppPathFixerTests
{
    protected AppSettingsData Settings => MockSettingsProvider.Value;

    protected ISettingsProvider<AppSettingsData> MockSettingsProvider => _mockSettingsProvider ??= new FakeSettingsProvider<AppSettingsData>();
    private ISettingsProvider<AppSettingsData> _mockSettingsProvider;

    protected Mock<IDialogManager> MockDialogManager => _mockDialogManager ??= new Mock<IDialogManager>();
    private Mock<IDialogManager> _mockDialogManager;

    protected IDialogService DialogService => _dialogService ??= new DialogService(MockDialogManager.Object);
    private IDialogService _dialogService;

    protected IDictionary<string, MockFileData> Files { get; set; } = new Dictionary<string, MockFileData>();

    protected IFileSystemService MockFileSystem => _mockFileSystem ??= new FakeFileSystemService(Files);
    private IFileSystemService _mockFileSystem;

    protected INotifyPropertyChanged Owner { get; set; } = Mock.Of<INotifyPropertyChanged>();

    protected IPathFixer Model => _model ??= new PathFixer(MockFileSystem, DialogService);
    private IPathFixer _model;

    protected void AddFile(string filePath) => Files.Add(
        new KeyValuePair<string, MockFileData>(filePath.ReplaceDirectorySeparator(), new MockFileData(string.Empty)));

    protected void AddFolder(string folder) =>
        Settings.Folders.Add(folder.ReplaceDirectorySeparator());

    protected void AddPreset(string presetName = "Preset") =>
        Settings.Presets.Add(new PresetItem(presetName));

    protected void AddPresetFile(string filePath) =>
        Settings.Presets[0].Files.Add(new PlayingItem(filePath.ReplaceDirectorySeparator()));

    protected void SetMessageBoxResult(bool? result)
    {
        MockDialogManager.Setup(x =>
                x.ShowFrameworkDialogAsync(Owner, It.IsAny<MessageBoxSettings>(), It.IsAny<AppDialogSettingsBase>(), It.IsAny<Func<object,string>>()))
            .ReturnsAsync(result);
    }

    protected void SetOpenFolderResult(string result, bool? nextPromptResult = null)
    {
        MockDialogManager.Setup(x =>
                x.ShowFrameworkDialogAsync(Owner, It.IsAny<OpenFolderDialogSettings>(),
                    It.IsAny<AppDialogSettingsBase>(), It.IsAny<Func<object,string>>()))
            .ReturnsAsync(new List<IDialogStorageFolder> { GetFolderMock(result) })
            .Callback(() => SetMessageBoxResult(nextPromptResult));
    }
    
    private IDialogStorageFolder GetFolderMock(string path) => path == null ? null :
        Mock.Of<IDialogStorageFolder>(x => x.Name == path && x.Path == new Uri(path) && x.LocalPath == path);

    protected void VerifyMessageBox(Times times)
    {
        MockDialogManager.Verify(x => x.ShowFrameworkDialogAsync(
            Owner, It.IsAny<MessageBoxSettings>(), It.IsAny<AppDialogSettingsBase>(), It.IsAny<Func<object,string>>()), times);
    }

    protected void VerifyOpenFolder(Times times)
    {
        MockDialogManager.Verify(x => x.ShowFrameworkDialogAsync(
            Owner, It.IsAny<OpenFolderDialogSettings>(), It.IsAny<AppDialogSettingsBase>(), It.IsAny<Func<object,string>>()), times);
    }

    protected List<FixFolderItem> GetAllFolders()
    {
        var folders = new List<FixFolderItem> { new FixFolder<string>(Settings.Folders) };
        folders.AddRange(Settings.Presets.Select(x => new FixFolder<PlayingItem>(x.Files, true, f => f.Path, (f, v) => f.Path = v!)));
        return folders;
    }

    [Fact]
    public async Task ScanAndFixFoldersAsync_NoFolder_DoNotPrompt()
    {
        await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        VerifyMessageBox(Times.Never());
        MockDialogManager.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task ScanAndFixFoldersAsync_NoFolder_ReturnFalse()
    {
        var result = await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        Assert.False(result);
    }

    [Theory]
    [InlineData("/Dir")]
    [InlineData("/Dir/Sub")]
    [InlineData("/Dir/")]
    [InlineData("/Dir/Sub/")]
    public async Task ScanAndFixFoldersAsync_FoldersValid_DoNotPrompt(string folder)
    {
        AddFile("/Dir/Sub/file.mp3");
        AddFolder(folder);

        await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        VerifyMessageBox(Times.Never());
        MockDialogManager.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task ScanAndFixFoldersAsync_FoldersValid_ReturnFalse()
    {
        AddFile("/Dir/Sub/file.mp3");
        AddFolder("/Dir");

        var result = await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        Assert.False(result);
    }

    [Fact]
    public async Task ScanAndFixFoldersAsync_InvalidFolder_Prompt()
    {
        AddFolder("/Invalid");

        await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        VerifyMessageBox(Times.Once());
        MockDialogManager.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ScanAndFixFoldersAsync_AnswerYes_PromptFolder()
    {
        AddFolder("/Invalid");
        SetMessageBoxResult(true);

        await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        VerifyOpenFolder(Times.Once());
    }

    [Fact]
    public async Task ScanAndFixFoldersAsync_BrowseInvalid_PromptAgain()
    {
        AddFolder("/Invalid");
        SetMessageBoxResult(true);
        SetOpenFolderResult("/Nope".ReplaceDirectorySeparator());

        await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        VerifyOpenFolder(Times.Once());
        VerifyMessageBox(Times.Exactly(2));
        MockDialogManager.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task ScanAndFixFoldersAsync_BrowseInvalid_ReturnFalse()
    {
        AddFolder("/Invalid");
        SetMessageBoxResult(true);
        SetOpenFolderResult("/Nope".ReplaceDirectorySeparator());

        var result = await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        Assert.False(result);
    }

    [Theory]
    [InlineData("/New")]
    [InlineData("/New/")]
    [InlineData("/New/Sub")]
    [InlineData("/New/Sub/")]
    public async Task ScanAndFixFoldersAsync_BrowseValid_ReplaceFolder(string selectPath)
    {
        AddFile("/New/Sub/File1.mp3");
        AddFolder("/Old/Sub");
        SetMessageBoxResult(true);
        SetOpenFolderResult(selectPath.ReplaceDirectorySeparator());

        await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        Assert.Equal("/New/Sub".ReplaceDirectorySeparator(), Settings.Folders[0]);
        VerifyMessageBox(Times.Once());
    }
    
    [Fact]
    public async Task ScanAndFixFoldersAsync_BrowseValid_ReturnTrue()
    {
        AddFile("/New/Sub/File1.mp3");
        AddFolder("/Old/Sub");
        SetMessageBoxResult(true);
        SetOpenFolderResult("/New".ReplaceDirectorySeparator());

        var result = await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        Assert.True(result);
    }

    [Theory]
    [InlineData("/New")]
    [InlineData("/New/")]
    [InlineData("/New/Sub")]
    [InlineData("/New/Sub/")]
    public async Task ScanAndFixFoldersAsync_BrowseValid_ReplaceOtherFolder(string selectPath)
    {
        AddFile("/New/Sub/File1.mp3");
        AddFile("/New/Music/File2.mp3");
        AddFolder("/Old/Sub");
        AddFolder("/Old/Music");
        SetMessageBoxResult(true);
        SetOpenFolderResult(selectPath.ReplaceDirectorySeparator());

        await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        Assert.Equal("/New/Sub".ReplaceDirectorySeparator(), Settings.Folders[0]);
        Assert.Equal("/New/Music".ReplaceDirectorySeparator(), Settings.Folders[1]);
        VerifyMessageBox(Times.Once());
    }

    [Fact]
    public async Task ScanAndFixFoldersAsync_ReplaceDone_PromptOtherInvalidFolder()
    {
        AddFile("/New/Sub/File1.mp3");
        AddFolder("/Old/Sub");
        AddFolder("/Older/Dir");
        SetMessageBoxResult(true);
        SetOpenFolderResult("/New".ReplaceDirectorySeparator());

        await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        VerifyOpenFolder(Times.Once());
        VerifyMessageBox(Times.Exactly(2));
        MockDialogManager.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task ScanAndFixFoldersAsync_PromptOtherInvalidFolder_ReturnTrue()
    {
        AddFile("/New/Sub/File1.mp3");
        AddFolder("/Old/Sub");
        AddFolder("/Older/Dir");
        SetMessageBoxResult(true);
        SetOpenFolderResult("/New".ReplaceDirectorySeparator());

        var result = await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        Assert.True(result);
    }

    [Theory]
    [InlineData("/Old/Sub/File1.mp3", "/New/Sub/File1.mp3")]
    [InlineData("/Old/Sub/NoFile.mp3", "/New/Sub/NoFile.mp3")]
    [InlineData("/Old/Other/NoFile.mp3", "/New/Other/NoFile.mp3")]
    public async Task ScanAndFixFoldersAsync_BrowseValid_ReplacePresetFile(string presetFilePath, string expected)
    {
        AddFile("/New/Sub/File1.mp3");
        AddFolder("/Old/Sub");
        AddPreset();
        AddPresetFile(presetFilePath);
        SetMessageBoxResult(true);
        SetOpenFolderResult("/New/Sub".ReplaceDirectorySeparator());

        await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        Assert.Equal(expected.ReplaceDirectorySeparator(), Settings.Presets[0].Files[0].Path);
    }
    
    [Theory]
    [InlineData("/Backup/Home/Music/INNA/Party", "/Sync/Music/INNA/Party", "/Backup/Home/Music/Other.mp3", "/Sync/Music/Other.mp3")]
    [InlineData("/Backup/Music/INNA/Party", "/Music/INNA/Party", "/Backup/Music/Other.mp3", "/Music/Other.mp3")]
    [InlineData("/Backup/Music/INNA/Party", "/Mus/INNA/Party", "/Backup/Music/INNA/End/Wow.mp3", "/Mus/INNA/End/Wow.mp3")]
    [InlineData("/Backup/Music/INNA/Party", "/Mus/INNA/Party", "/Backup/Music/Other.mp3", "/Mus/Other.mp3")]
    [InlineData("/Backup/Music/INNA/Party", "/Mus/INNA/Party", "/Music/Other.mp3", "/Music/Other.mp3")]
    public async Task ScanAndFixFoldersAsync_BrowseValid_ReplacePresetFileComplex(string oldFolder, string newFolder, string oldFile, string newFile)
    {
        newFile = newFile.ReplaceDirectorySeparator();
        AddFile(newFile);
        AddFolder(oldFolder);
        AddPreset();
        AddPresetFile(oldFile);
        SetMessageBoxResult(true);
        SetOpenFolderResult(newFolder.ReplaceDirectorySeparator());

        await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        Assert.Equal(newFile, Settings.Presets[0].Files[0].Path);
    }
    
    [Fact]
    public async Task ScanAndFixFoldersAsync_ValidFile_ReturnFalse()
    {
        var fileName = "/Dir/Sub/file.mp3";
        AddFile(fileName);
        AddFolder("/Dir/Sub/");
        AddPreset();
        AddPresetFile(fileName);

        var result = await Model.ScanAndFixFoldersAsync(Owner, GetAllFolders());

        Assert.False(result);
    }}

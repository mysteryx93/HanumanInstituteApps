using System.Collections.Generic;
using System.IO;
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

    protected FakeFileSystemService MockFileSystem => _mockFileSystem ??= new FakeFileSystemService();
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

        Model.SaveSettingsCommand.Execute(null);

        _mockAppSettings.Verify(x => x.Save(), Times.Once);
    }
}

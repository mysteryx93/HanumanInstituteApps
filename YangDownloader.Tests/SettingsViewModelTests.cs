using System.Collections.Generic;
using FluentAvalonia.Styling;
using HanumanInstitute.Common.Avalonia.App;
using HanumanInstitute.FFmpeg;
using HanumanInstitute.MvvmDialogs.FileSystem;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HanumanInstitute.YangDownloader.Models;

namespace YangDownloader.Tests;

public class SettingsViewModelTests: TestsBase
{
    public SettingsViewModelTests(ITestOutputHelper output) : base(output) { }

    public SettingsViewModel Model => _model ??= 
        new SettingsViewModel(FakeSettings, Mock.Of<IFluentAvaloniaTheme>(), DialogService, MockEncoderService.Object, FakeFileSystem);
    private SettingsViewModel _model;
    
    public IDialogService DialogService => _dialogService ??= new DialogService(MockDialogManager.Object);
    private IDialogService _dialogService;

    public Mock<IDialogManager> MockDialogManager => _mockDialogManager ??= new Mock<IDialogManager>();
    private Mock<IDialogManager> _mockDialogManager;
    
    public ISettingsProvider<AppSettingsData> FakeSettings => _fakeSettings ??= new FakeSettingsProvider<AppSettingsData>();
    private ISettingsProvider<AppSettingsData> _fakeSettings;

    public IFileSystemService FakeFileSystem => _fakeFileSystem ??= new FakeFileSystemService();
    private IFileSystemService _fakeFileSystem;
    
    public Mock<IEncoderService> MockEncoderService => _mockEncoderService ??= new Mock<IEncoderService>();
    private Mock<IEncoderService> _mockEncoderService;

    protected void SetBrowseDestination(string value) =>
        MockDialogManager.Setup(x => x.ShowFrameworkDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<OpenFolderDialogSettings>(),
                It.IsAny<AppDialogSettingsBase>(), It.IsAny<Func<object, string>>()))
            .Returns(Task.FromResult<object>(new List<IDialogStorageFolder> { GetFolderMock(value) }));

    private IDialogStorageFolder GetFolderMock(string path) => path == null ? null :
        Mock.Of<IDialogStorageFolder>(x => x.Name == path && x.Path == new Uri(path) && x.LocalPath == path);

    [Fact]
    public void BrowseDestination_Cancel_DisplayOpenFolder()
    {
        var dest = "/MyDest";
        Model.Settings.DestinationFolder = dest;
        SetBrowseDestination(null);

        Model.BrowseDestination.Execute().Subscribe();

        Assert.Equal(dest, Model.Settings.DestinationFolder);
        MockDialogManager.Verify(
            x => x.ShowFrameworkDialogAsync(It.IsAny<INotifyPropertyChanged>(), It.IsAny<OpenFolderDialogSettings>(),
                It.IsAny<AppDialogSettingsBase>(), It.IsAny<Func<object, string>>()),
            Times.Once);
    }

    [Fact]
    public void BrowseDestination_Select_SetNewDestinationAndResetError()
    {
        var dest = "/MyDest";
        SetBrowseDestination(dest);

        Model.BrowseDestination.Execute().Subscribe();

        Assert.Equal(dest, Model.Settings.DestinationFolder);
    }
}

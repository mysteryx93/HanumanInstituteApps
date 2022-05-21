using System.Collections.Generic;
using System.IO;
using System.Linq;
using HanumanInstitute.Common.Services;
using HanumanInstitute.Player432hz.Business;
using HanumanInstitute.Player432hz.ViewModels;
using Moq;
using Xunit;

namespace HanumanInstitute.Player432hz.Tests.ViewModels;

public class FilesListViewModelTests
{
    public Mock<IPlaylistPlayer> MockPlayer => _mockPlayer ??= new Mock<IPlaylistPlayer>();
    private Mock<IPlaylistPlayer>? _mockPlayer;

    public Mock<IFileSystemService> MockFileSystem => _mockFileSystem ??= SetupFileSystem();
    private Mock<IFileSystemService>? _mockFileSystem;

    public IAppPathService AppPath => _appPath ??= new AppPathService(Mock.Of<IEnvironmentService>(), MockFileSystem.Object);
    private IAppPathService? _appPath;

    public IFileLocator FileLocator => _fileLocator ??= new FileLocator(AppPath, MockFileSystem.Object);
    private IFileLocator? _fileLocator;

    public IFilesListViewModel Model => _model ??= new FilesListViewModel(FileLocator, MockPlayer.Object);
    private IFilesListViewModel? _model;

    private static IEnumerable<string> PathValue => new[] { "test-path" };
    private static IEnumerable<string> FileList => new[] { "file1", "file2" };

    private static Mock<IFileSystemService> SetupFileSystem()
    {
        var result = new Mock<IFileSystemService>();
        result.Setup(
                x => x.GetFilesByExtensions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<SearchOption>()))
            .Returns(FileList);
        return result;
    }

    private void VerifyGetFiles(Times times)
    {
        MockFileSystem.Verify(
            x => x.GetFilesByExtensions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<SearchOption>()),
            times);
    }

    [Fact]
    public void SetPaths_Null_FilesEmptyNoGetFiles()
    {
        Model.SetPaths(null);

        Assert.Empty(Model.Files.Source);
        VerifyGetFiles(Times.Never());
    }

    [Fact]
    public void SetPaths_Value_FilesNotEmptyGetFilesOnce()
    {
        Model.SetPaths(PathValue);

        Assert.Equal(FileList, Model.Files.Source.Select(x => x.Path));
        VerifyGetFiles(Times.Once());
    }
    
    [Fact]
    public void SetPaths_Value_FilesHaveName()
    {
        Model.SetPaths(PathValue);

        Assert.Empty(Model.Files.Source.Select(x => string.IsNullOrEmpty(x.Name)));
    }

    [Fact]
    public void SetPaths_Value_FilesLazyLoaded()
    {
        Model.SetPaths(PathValue);

        VerifyGetFiles(Times.Never());
    }

    [Fact]
    public void SetPaths_Value_AccessFilesTwiceGetFilesOnce()
    {
        Model.SetPaths(PathValue);

        var a = Model.Files.Source;
        var b = Model.Files.Source;
        
        VerifyGetFiles(Times.Once());
        Assert.NotEmpty(a);
        Assert.NotEmpty(b);
    }

    [Fact]
    public void SetPaths_ValueThenNull_FilesEmptyNoGetFiles()
    {
        Model.SetPaths(PathValue);
        Model.SetPaths(null);

        Assert.Empty(Model.Files.Source);
        VerifyGetFiles(Times.Never());
    }

    [Fact]
    public void SetPaths_ValueReadThenNull_FilesEmptyGetFilesOnce()
    {
        Model.SetPaths(PathValue);
        var _ = Model.Files;
        Model.SetPaths(null);

        Assert.Empty(Model.Files.Source);
        VerifyGetFiles(Times.Once());
    }
}

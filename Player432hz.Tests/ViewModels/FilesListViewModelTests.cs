using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HanumanInstitute.Common.Avalonia.App.Tests;
using HanumanInstitute.Common.Services;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.Player432hz.Business;
using HanumanInstitute.Player432hz.ViewModels;
using Moq;
using Xunit;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PossibleUnintendedReferenceComparison

namespace HanumanInstitute.Player432hz.Tests.ViewModels;

public class FilesListViewModelTests
{
    public Mock<IPlaylistPlayer> MockPlayer => _mockPlayer ??= new Mock<IPlaylistPlayer>();
    private Mock<IPlaylistPlayer>? _mockPlayer;

    public Mock<FakeFileSystemService> MockFileSystem => _mockFileSystem ??= SetupFileSystem();
    private Mock<FakeFileSystemService>? _mockFileSystem;

    public IAppPathService AppPath =>
        _appPath ??= new AppPathService(Mock.Of<IEnvironmentService>(), MockFileSystem.Object, MockBassDevice);
    private IAppPathService? _appPath;

    public IBassDevice MockBassDevice => _mockBassDevice ??= Mock.Of<IBassDevice>(x =>
        x.SupportedExtensions == new List<FileExtension>(new[] { new FileExtension("MP3", new[] { "mp3" }) })
    );
    private IBassDevice? _mockBassDevice;

    public IFileLocator FileLocator => _fileLocator ??= new FileLocator(AppPath, MockFileSystem.Object);
    private IFileLocator? _fileLocator;

    public IFilesListViewModel Model => _model ??= new FilesListViewModel(FileLocator, MockPlayer.Object);
    private IFilesListViewModel? _model;

    private static IEnumerable<string> PathValue => new[] { "test-path" };
    private static IEnumerable<string> FileList => new[] { "file1", "file2" };

    private static Mock<FakeFileSystemService> SetupFileSystem()
    {
        var result = new Mock<FakeFileSystemService>() { CallBase = true };
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

        Assert.All(Model.Files.Source, x =>
        {
            Assert.NotNull(x.Name);
            Assert.NotEmpty(x.Name);
        });
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

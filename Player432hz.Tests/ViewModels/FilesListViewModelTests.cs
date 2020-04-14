using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using HanumanInstitute.CommonServices;
using HanumanInstitute.CommonWpfApp.Tests;
using HanumanInstitute.Player432hz.Business;
using HanumanInstitute.Player432hz.ViewModels;
using Xunit;
using Moq;

namespace Player432hz.Tests.ViewModels
{
    public class FilesListViewModelTests
    {
        private Mock<IPlaylistPlayer> _mockPlayer;
        private Mock<FakeFileSystemService> _mockFileSystem;
        private IEnumerable<string> PathValue => new string[] { "test-path" };
        private IEnumerable<string> FileList => new string[] { "file1", "file2" };

        private IFilesListViewModel SetupModel()
        {
            _mockPlayer = new Mock<IPlaylistPlayer>();
            _mockFileSystem = new Mock<FakeFileSystemService>();
            _mockFileSystem.Setup(
                x => x.GetFilesByExtensions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<SearchOption>()))
                .Returns(FileList);
            var fileLocator = new FileLocator(new AppPathService(Mock.Of<IEnvironmentService>(), _mockFileSystem.Object), _mockFileSystem.Object);
            return new FilesListViewModel(fileLocator, _mockPlayer.Object);
        }

        private void VerifyGetFiles(Times times)
        {
            _mockFileSystem.Verify(
                x => x.GetFilesByExtensions(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<SearchOption>()),
                times);
        }

        [Fact]
        public void SetPaths_Null_FilesEmptyNoGetFiles()
        {
            var model = SetupModel();

            model.SetPaths(null);

            Assert.Empty(model.Files.List);
            VerifyGetFiles(Times.Never());
        }

        [Fact]
        public void SetPaths_Value_FilesNotEmptyGetFilesOnce()
        {
            var model = SetupModel();

            model.SetPaths(PathValue);

            Assert.Equal(FileList, model.Files.List);
            VerifyGetFiles(Times.Once());
        }

        [Fact]
        public void SetPaths_Value_FilesLazyLoaded()
        {
            var model = SetupModel();

            model.SetPaths(PathValue);

            VerifyGetFiles(Times.Never());
        }

        [Fact]
        public void SetPaths_Value_AccessFilesTwiceGetFilesOnce()
        {
            var model = SetupModel();

            model.SetPaths(PathValue);

            var _ = model.Files.List;
            _ = model.Files.List;
            VerifyGetFiles(Times.Once());
        }

        [Fact]
        public void SetPaths_ValueThenNull_FilesEmptyNoGetFiles()
        {
            var model = SetupModel();

            model.SetPaths(PathValue);
            model.SetPaths(null);

            Assert.Empty(model.Files.List);
            VerifyGetFiles(Times.Never());
        }

        [Fact]
        public void SetPaths_ValueReadThenNull_FilesEmptyGetFilesOnce()
        {
            var model = SetupModel();

            model.SetPaths(PathValue);
            var _ = model.Files;
            model.SetPaths(null);

            Assert.Empty(model.Files.List);
            VerifyGetFiles(Times.Once());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a")]
        public void PlayCommand_NoPaths_PlayNotCalled(string current)
        {
            var model = SetupModel();

            model.PlayCommand.Execute(current);

            _mockPlayer.Verify(x => x.Play(It.IsAny<IEnumerable<string>>(), current), Times.Never());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("a")]
        public void PlayCommand_HasPaths_PlayCalled(string current)
        {
            var model = SetupModel();
            model.SetPaths(PathValue);

            model.PlayCommand.Execute(current);

            _mockPlayer.Verify(x => x.Play(It.IsAny<IEnumerable<string>>(), current), Times.Once());
        }
    }
}

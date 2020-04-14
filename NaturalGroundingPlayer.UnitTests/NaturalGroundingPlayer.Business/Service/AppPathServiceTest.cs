using System;
using System.IO;
using Moq;
using Xunit;
using EmergenceGuardian.CommonServices;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business.Test {
    public class AppPathServiceTest {
        private readonly IAppPathService appPath;

        public AppPathServiceTest() {
            // Setup mocks.
            var environment = new Mock<IEnvironmentService>(MockBehavior.Strict);
            environment.Setup(x => x.CommonApplicationDataPath).Returns(@"C:\ProgramData\");
            environment.Setup(x => x.AppDirectory).Returns(@"C:\Program Files (x86)\Natural Grounding Player\");
            environment.Setup(x => x.SystemRootDirectory).Returns(@"C:\");
            environment.Setup(x => x.ProgramFilesX86).Returns(@"C:\Program Files (x86)\");
            var fileSystem = new Mock<IFileSystemService>();
            fileSystem.Setup(x => x.Path.GetTempPath()).Returns(@"C:\Users\Etienne\AppData\Local\Temp\");
            fileSystem.Setup(x => x.Path.Combine(It.IsAny<string>(), It.IsAny<string>())).Returns((string a, string b) => Path.Combine(a, b));
            fileSystem.Setup(x => x.File.Exists(It.IsAny<string>())).Returns(true);
            appPath = new AppPathService(environment.Object, fileSystem.Object);
        }

        [Fact]
        public void VideoExtensions() {
            var Result = appPath.VideoExtensions;
            ValidateMediaExtensionList(Result, ".avi", 13);
        }

        [Fact]
        public void AudioExtensions() {
            var Result = appPath.AudioExtensions;
            ValidateMediaExtensionList(Result, ".aac", 7);
        }

        [Fact]
        public void ImageExtensions() {
            var Result = appPath.ImageExtensions;
            ValidateMediaExtensionList(Result, ".png", 5);
        }

        [Theory]
        [InlineData(MediaType.None, null, 0)]
        [InlineData(MediaType.Video, ".avi", 13)]
        [InlineData(MediaType.Audio, ".aac", 7)]
        [InlineData(MediaType.Image, ".png", 5)]
        public void GetMediaTypeExtensions(MediaType mediaType, string ext, int length) {
            var Result = appPath.GetMediaTypeExtensions(mediaType);
            ValidateMediaExtensionList(Result, ext, length);
        }

        /// <summary>
        /// Ensures a list of string contains a specified value and has the specified length.
        /// </summary>
        /// <param name="list">The list to validate.</param>
        /// <param name="contains">A value that should be contained in the list.</param>
        /// <param name="length">The expected length of the list.</param>
        protected void ValidateMediaExtensionList(string[] list, string contains, int length) {
            Assert.NotNull(list);
            if (contains != null)
                Assert.Contains(list, (e) => contains.Equals(e));
            Assert.Equal(list.Length, length);
        }

        [Fact]
        public void SettingsPath() => ValidatePath(appPath.SettingsPath, "Settings.xml", true);

        [Fact]
        public void UnhandledExceptionLogPath() => ValidatePath(appPath.UnhandledExceptionLogPath, "Log.txt", true);

        [Fact]
        public void DatabasePath() => ValidatePath(appPath.DatabasePath, "NaturalGroundingVideos.db", true);

        [Fact]
        public void InitialDatabasePath() => ValidatePath(appPath.InitialDatabasePath, "InitialDatabase.db", true);

        [Fact]
        public void AvisynthPluginsPath() => ValidatePath(appPath.AvisynthPluginsPath, "Encoder\\", true);

        [Fact]
        public void Player432hzScriptFile() => ValidatePath(appPath.Player432hzScriptFile, "432hzPlaying.avs", true);

        [Fact]
        public void Player432hzConfigFile() => ValidatePath(appPath.Player432hzConfigFile, "432hzConfig.xml", true);

        [Fact]
        public void PowerliminalsPlayerConfigFile() => ValidatePath(appPath.PowerliminalsPlayerConfigFile, "PowerliminalsConfig.xml", true);

        [Fact]
        public void FFmpegPath() => ValidatePath(appPath.FFmpegPath, "ffmpeg.exe", false);

        [Fact]
        public void LocalTempPath() => ValidatePath(appPath.LocalTempPath, "Temp\\", false);

        [Fact]
        public void SystemTempPath() => ValidatePath(appPath.SystemTempPath, "", true);

        [Fact]
        public void DownloaderTempPath() => ValidatePath(appPath.DownloaderTempPath, "YangYoutubeDownloader\\", true);

        [Fact]
        public void DefaultNaturalGroundingFolder() => ValidatePath(appPath.DefaultNaturalGroundingFolder, "Natural Grounding\\", true);

        [Fact]
        public void GetDefaultSvpPath() => ValidatePath(appPath.GetDefaultSvpPath(), "SVPManager.exe", true);

        /// <summary>
        /// Ensures a path ends with specified value, and checks whether it is absolute or relative.
        /// </summary>
        /// <param name="path">The path to validate.</param>
        /// <param name="endswith">The expected end of the path.</param>
        /// <param name="absolute">Whether the path should be absolute or relative.</param>
        protected void ValidatePath(string path, string endswith, bool absolute) {
            Assert.NotNull(path);
            Assert.Equal(absolute, Path.IsPathRooted(path));
            if (absolute)
                endswith = "\\" + endswith;
            Assert.EndsWith(endswith, path);
        }
    }
}

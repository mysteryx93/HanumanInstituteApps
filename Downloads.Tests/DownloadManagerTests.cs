using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using HanumanInstitute.CommonTests;
using HanumanInstitute.FFmpeg;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace HanumanInstitute.Downloads.Tests
{
    public class DownloadManagerTests
    {
        private const string VideoUrl = "https://www.youtube.com/watch?v=HV9PCGoGbSA";
        private const string DestFile = "DownloadTestFile.mp4";
        private readonly VideoId _videoId = new VideoId(VideoUrl);
        const string VideoTitle = "Title";
        private readonly Mock<IOptions<DownloadOptions>> _mockOptions = new Mock<IOptions<DownloadOptions>>();
        private readonly Mock<IYouTubeDownloader> _mockYouTube = new Mock<IYouTubeDownloader>();
        [NotNull]
        private FakeDownloadTaskFactory? _fakeFactory;

        private DownloadManager SetupModel()
        {
            var muxer = Mock.Of<IMediaMuxer>();
            SetConcurrentDownloads(2);
            var taskFactory = new DownloadTaskFactory(_mockYouTube.Object, new YouTubeStreamSelector(), new FakeFileSystemService(), muxer);
            return new DownloadManager(taskFactory, _mockYouTube.Object, _mockOptions.Object);
        }

        private DownloadManager SetupModelWithFakeTask()
        {
            var muxer = Mock.Of<IMediaMuxer>();
            SetConcurrentDownloads(2);
            _fakeFactory = new FakeDownloadTaskFactory();
            return new DownloadManager(_fakeFactory, _mockYouTube.Object, _mockOptions.Object);
        }

        private void SetConcurrentDownloads(int concurrent)
        {
            _mockOptions.Setup(x => x.Value).Returns(new DownloadOptions() { ConcurrentDownloads = concurrent });
        }

        [Fact]
        public async void GetVideoTitleAsync_ValidUrl_ReturnsTitle()
        {
            using var model = SetupModel();
            _mockYouTube.Setup(x => x.QueryVideoAsync(It.IsAny<VideoId>())).Returns(() =>
                Task.FromResult(new Video(_videoId, VideoTitle, "Author", new DateTimeOffset(), "description", new TimeSpan(), new ThumbnailSet(_videoId), new List<string>(), new Engagement(0, 0, 0))));

            var result = await model.GetVideoTitleAsync(new Uri(VideoUrl));

            Assert.Equal(VideoTitle, result);
        }

        [Fact]
        public async void GetVideoTitleAsync_ValidString_ReturnsTitle()
        {
            using var model = SetupModel();
            _mockYouTube.Setup(x => x.QueryVideoAsync(It.IsAny<VideoId>())).Returns(() =>
                Task.FromResult(new Video(_videoId, VideoTitle, "Author", new DateTimeOffset(), "description", new TimeSpan(), new ThumbnailSet(_videoId), new List<string>(), new Engagement(0, 0, 0))));

            var result = await model.GetVideoTitleAsync(VideoUrl);

            Assert.Equal(VideoTitle, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("https://")]
        [InlineData("https://www.youtube.com/watch")]
        public async void GetVideoTitleAsync_InvalidString_ThrowsUriFormatException(string url)
        {
            using var model = SetupModel();
            _mockYouTube.Setup(x => x.QueryVideoAsync(It.IsAny<VideoId>())).Returns(() =>
                Task.FromResult(new Video(_videoId, VideoTitle, "Author", new DateTimeOffset(), "description", new TimeSpan(), new ThumbnailSet(_videoId), new List<string>(), new Engagement(0, 0, 0))));

            async Task<string> Act() => await model.GetVideoTitleAsync(url);

            await Assert.ThrowsAsync<UriFormatException>(Act);
        }

        [Fact]
        public async void GetDownloadInfoAsync_ValidUrl_ReturnsTitle()
        {
            using var model = SetupModel();
            _mockYouTube.Setup(x => x.QueryVideoAsync(It.IsAny<VideoId>())).Returns(() => Task.FromResult(
                new Video(_videoId, VideoTitle, "Author", new DateTimeOffset(), "description", new TimeSpan(), new ThumbnailSet(_videoId), new List<string>(), new Engagement(0, 0, 0))));
            _mockYouTube.Setup(x => x.QueryStreamInfoAsync(It.IsAny<VideoId>())).Returns(() => Task.FromResult(
                new StreamManifest(new List<IStreamInfo>())));

            var result = await model.GetDownloadInfoAsync(new Uri(VideoUrl));

            Assert.Equal(VideoTitle, result?.Info.Title);
            Assert.NotNull(result?.Streams);
        }

        [Fact]
        public async void GetDownloadInfoAsync_ValidString_ReturnsTitle()
        {
            using var model = SetupModel();
            _mockYouTube.Setup(x => x.QueryVideoAsync(It.IsAny<VideoId>())).Returns(() => Task.FromResult(
                new Video(_videoId, VideoTitle, "Author", new DateTimeOffset(), "description", new TimeSpan(), new ThumbnailSet(_videoId), new List<string>(), new Engagement(0, 0, 0))));
            _mockYouTube.Setup(x => x.QueryStreamInfoAsync(It.IsAny<VideoId>())).Returns(() => Task.FromResult(
                new StreamManifest(new List<IStreamInfo>())));

            var result = await model.GetDownloadInfoAsync(VideoUrl);

            Assert.Equal(VideoTitle, result?.Info.Title);
            Assert.NotNull(result?.Streams);
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("https://")]
        [InlineData("https://www.youtube.com/watch")]
        public async void GetDownloadInfoAsync_InvalidString_ThrowsUriFormatException(string url)
        {
            using var model = SetupModel();
            _mockYouTube.Setup(x => x.QueryVideoAsync(It.IsAny<VideoId>())).Returns(() => Task.FromResult(
                new Video(_videoId, VideoTitle, "Author", new DateTimeOffset(), "description", new TimeSpan(), new ThumbnailSet(_videoId), new List<string>(), new Engagement(0, 0, 0))));
            _mockYouTube.Setup(x => x.QueryStreamInfoAsync(It.IsAny<VideoId>())).Returns(() => Task.FromResult(
                new StreamManifest(new List<IStreamInfo>())));

            async Task<VideoInfo?> Act() => await model.GetDownloadInfoAsync(url);

            await Assert.ThrowsAsync<UriFormatException>(Act);
        }

        [Fact]
        public async void DownloadAsync_ValidUrl_RaiseDownloadAddedWithSameUrlDest()
        {
            using var model = SetupModel();
            Uri? url = null;
            string? dest = null;
            model.DownloadAdded += (s, e) =>
            {
                url = e?.DownloadTask?.Url;
                dest = e?.DownloadTask?.Destination;
            };

            await model.DownloadAsync(VideoUrl, DestFile);

            Assert.Equal(url?.AbsoluteUri, VideoUrl);
            Assert.Equal(dest, DestFile);
        }

        [Fact]
        public async void DownloadAsync_TwoTasks_DownloadInParallel()
        {
            using var model = SetupModelWithFakeTask();

            var tList = new int[2].Select(x => model.DownloadAsync(VideoUrl, DestFile)).ToList();
            tList.Add(Task.Run(async () =>
            {
                // Wait for all downloads to start.
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, 2);

                Assert.Equal(2, _fakeFactory.TotalRunning);
                _fakeFactory.Complete();
                Assert.Equal(1, _fakeFactory.TotalRunning);
                _fakeFactory.Complete();
                return new DownloadTaskStatus();
            }));
            await Task.WhenAll(tList);

            Assert.Equal(2, _fakeFactory.TotalCreated);
            Assert.Equal(0, _fakeFactory.TotalRunning);
        }

        [Fact]
        public async void DownloadAsync_FiveTasks_ThreeWaitInQueue()
        {
            using var model = SetupModelWithFakeTask();
            var tList = new int[5].Select(x => model.DownloadAsync(VideoUrl, DestFile)).ToList();
            tList.Add(Task.Run(async () =>
            {
                // Wait for all downloads to start.
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, 2);
                await Task.Delay(100).ConfigureAwait(false);
                Assert.Equal(3, _fakeFactory.TotalWaiting);
                _fakeFactory.Complete(2);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Done, 2);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, 2);
                Assert.Equal(1, _fakeFactory.TotalWaiting);
                _fakeFactory.Complete(2);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Done, 4);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, 1);
                _fakeFactory.Complete(1);
                return new DownloadTaskStatus();
            }));
            await Task.WhenAll(tList);

            Assert.Equal(5, _fakeFactory.TotalCreated);
            Assert.Equal(0, _fakeFactory.TotalWaiting);
        }

        [Fact]
        public async void DownloadAsync_OptionThreeConcurrent_OneWaitInQueue()
        {
            using var model = SetupModelWithFakeTask();
            SetConcurrentDownloads(3);
            var tList = new int[4].Select(x => model.DownloadAsync(VideoUrl, DestFile)).ToList();
            tList.Add(Task.Run(async () =>
            {
                // Wait for all downloads to start.
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, 3);
                await Task.Delay(100).ConfigureAwait(false);
                Assert.Equal(1, _fakeFactory.TotalWaiting);
                _fakeFactory.Complete(2);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Done, 2);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, 2);
                Assert.Equal(0, _fakeFactory.TotalWaiting);
                _fakeFactory.Complete(2);
                return new DownloadTaskStatus();
            }));
            await Task.WhenAll(tList);

            Assert.Equal(4, _fakeFactory.TotalCreated);
            Assert.Equal(0, _fakeFactory.TotalWaiting);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1000)]
        public async void DownloadAsync_ConcurrentDownloadsOutOfRange_ThrowsException(int concurrent)
        {
            using var model = SetupModelWithFakeTask();
            SetConcurrentDownloads(concurrent);

            async Task Act() => await model.DownloadAsync(VideoUrl, DestFile);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(Act);
        }

        [Fact]
        public async void DownloadAsync_ChangeCapacityWithWaitLock_NoOverflow()
        {
            using var model = SetupModelWithFakeTask();
            var maxCapacity = DownloadManager.MaxConcurrentDownloads;
            SetConcurrentDownloads(maxCapacity);
            var tList = new int[maxCapacity].Select(x => model.DownloadAsync(VideoUrl, DestFile)).ToList();
            tList.Add(Task.Run(async () =>
            {
                // Wait for all downloads to start.
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, maxCapacity);
                await Task.Delay(100).ConfigureAwait(false);
                Assert.Equal(0, _fakeFactory.TotalWaiting);
                SetConcurrentDownloads(maxCapacity - 10); // The Wait() will only take effect after a download is completed.
                _fakeFactory.Complete(2);
                await Task.Delay(1000).ConfigureAwait(false);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Done, 2);
                _fakeFactory.Complete(4);
                SetConcurrentDownloads(maxCapacity);
                _fakeFactory.Complete(4);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Done, 10);
                _fakeFactory.Complete(maxCapacity - 10);
                return new DownloadTaskStatus();
            }));
            await Task.WhenAll(tList);

            Assert.Equal(maxCapacity, _fakeFactory.TotalCreated);
        }

        private async Task WaitUntilStatus(FakeDownloadTask.FakeStatus status, int count)
        {
            while (_fakeFactory.Total(status) < count)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }
    }
}

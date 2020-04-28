using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using HanumanInstitute.CommonTests;
using HanumanInstitute.FFmpeg;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using YoutubeExplode.Videos.Streams;

namespace HanumanInstitute.Downloads.Tests
{
    public class DownloadManagerTests
    {
        private const string VideoUrl = "https://www.youtube.com/watch?v=HV9PCGoGbSA";
        private const string DestFile = "DownloadTestFile.mp4";
        const string VideoTitle = "Title";
        private readonly DownloadOptions _options = new DownloadOptions();
        [NotNull]
        private FakeFileSystemService? _fileSystem;
        [NotNull]
        private FakeYouTubeDownloader? _mockYouTube;
        [NotNull]
        private FakeDownloadTaskFactory? _fakeFactory;
        [NotNull]
        private Mock<IMediaMuxer>? _mockMuxer;

        private DownloadManager SetupModel()
        {
            _fileSystem = new FakeFileSystemService();
            _mockYouTube = new FakeYouTubeDownloader(_fileSystem);
            _mockMuxer = new Mock<IMediaMuxer>();
            _mockMuxer.Setup(x => x.Muxe(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<ProcessOptionsEncoder?>(), It.IsAny<ProcessStartedEventHandler?>()))
                .Returns<string?, string?, string, ProcessOptionsEncoder?, ProcessStartedEventHandler?>((videoFile, audioFile, destination, options, callback) =>
            {
                _fileSystem.File.Copy(videoFile ?? audioFile, destination);
                return CompletionStatus.Success;
            });
            var mockOptions = Mock.Of<IOptions<DownloadOptions>>(x => x.Value == _options);
            var taskFactory = new DownloadTaskFactory(_mockYouTube, new YouTubeStreamSelector(), _fileSystem, _mockMuxer.Object);
            return new DownloadManager(taskFactory, _mockYouTube, mockOptions);
        }

        private DownloadManager SetupModelWithFakeTask()
        {
            _fileSystem = new FakeFileSystemService();
            _mockYouTube = new FakeYouTubeDownloader(_fileSystem);
            var mockOptions = Mock.Of<IOptions<DownloadOptions>>(x => x.Value == _options);
            _fakeFactory = new FakeDownloadTaskFactory();
            return new DownloadManager(_fakeFactory, _mockYouTube, mockOptions);
        }

        private void SetDownload()
        {
            _mockYouTube.Configure(VideoUrl, VideoTitle, new StreamManifest(new List<IStreamInfo>()
            {
                new VideoOnlyStreamInfo(1, "https://www.youtube.com/video", Container.Mp4, new FileSize(10240), new Bitrate(), "h264", "", VideoQuality.High720, new VideoResolution(960, 720), new Framerate()),
                new AudioOnlyStreamInfo(1, "https://www.youtube.com/audio", Container.Mp4, new FileSize(500), new Bitrate(), "aac")
            }));
        }

        [Fact]
        public async void GetVideoTitleAsync_ValidUrl_ReturnsTitle()
        {
            using var model = SetupModel();

            var result = await model.GetVideoTitleAsync(new Uri(VideoUrl));

            Assert.Equal(VideoTitle, result);
        }

        [Fact]
        public async void GetVideoTitleAsync_ValidString_ReturnsTitle()
        {
            using var model = SetupModel();

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

            async Task<string> Act()
            {
                return await model.GetVideoTitleAsync(url);
            }

            await Assert.ThrowsAsync<UriFormatException>(Act);
        }

        [Fact]
        public async void GetDownloadInfoAsync_ValidUrl_ReturnsTitle()
        {
            using var model = SetupModel();

            var result = await model.GetDownloadInfoAsync(new Uri(VideoUrl));

            Assert.Equal(VideoTitle, result?.Info.Title);
            Assert.NotNull(result?.Streams);
        }

        [Fact]
        public async void GetDownloadInfoAsync_ValidString_ReturnsTitle()
        {
            using var model = SetupModel();

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
            _mockYouTube.Configure(url, string.Empty);

            async Task<VideoInfo?> Act()
            {
                return await model.GetDownloadInfoAsync(url);
            }

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
                url = e?.Download?.Url;
                dest = e?.Download?.Destination;
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
                return DownloadStatus.Success;
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
                return DownloadStatus.Success;
            }));
            await Task.WhenAll(tList);

            Assert.Equal(5, _fakeFactory.TotalCreated);
            Assert.Equal(0, _fakeFactory.TotalWaiting);
        }

        [Fact]
        public async void DownloadAsync_OptionThreeConcurrent_OneWaitInQueue()
        {
            using var model = SetupModelWithFakeTask();
            _options.ConcurrentDownloads = 3;
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
                return DownloadStatus.Success;
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
            _options.ConcurrentDownloads = concurrent;

            async Task Act()
            {
                await model.DownloadAsync(VideoUrl, DestFile);
            }

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(Act);
        }

        [Fact]
        public async void DownloadAsync_ChangeCapacityWithWaitLock_NoOverflow()
        {
            using var model = SetupModelWithFakeTask();
            var maxCapacity = DownloadManager.MaxConcurrentDownloads;
            _options.ConcurrentDownloads = maxCapacity;
            var tList = new int[maxCapacity].Select(x => model.DownloadAsync(VideoUrl, DestFile)).ToList();
            tList.Add(Task.Run(async () =>
            {
                // Wait for all downloads to start.
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, maxCapacity);
                await Task.Delay(100).ConfigureAwait(false);
                Assert.Equal(0, _fakeFactory.TotalWaiting);
                _options.ConcurrentDownloads -= 10; // The Wait() will only take effect after a download is completed.
                _fakeFactory.Complete(2);
                await Task.Delay(1000).ConfigureAwait(false);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Done, 2);
                _fakeFactory.Complete(2);
                _options.ConcurrentDownloads = maxCapacity;
                _fakeFactory.Complete(2);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Done, 6);
                _fakeFactory.Complete(maxCapacity - 6);
                return DownloadStatus.Success;
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

        [Fact]
        public async void DownloadAsync_Valid_CreatesDestinationFile()
        {
            using var model = SetupModel();
            SetDownload();

            await model.DownloadAsync(VideoUrl, DestFile);

            Assert.True(_fileSystem.File.Exists(DestFile));
        }

        [Fact]
        public async void DownloadAsync_Valid_ReturnsSuccess()
        {
            using var model = SetupModel();
            SetDownload();

            var result = await model.DownloadAsync(VideoUrl, DestFile);

            Assert.Equal(DownloadStatus.Success, result);
        }

        [Fact]
        public async void DownloadAsync_BeforeMuxing_StatusFinalizing()
        {
            using var model = SetupModel();
            SetDownload();
            DownloadStatus? status = null;

            await model.DownloadAsync(VideoUrl, DestFile, (s, e) =>
            {
                e.Download.BeforeMuxing += (s, e) =>
                {
                    status = e.Download.Status;
                };
            });

            Assert.Equal(DownloadStatus.Finalizing, status);
        }

        [Fact]
        public async void DownloadAsync_CustomMuxing_SuccessWithCustomFileSize()
        {
            using var model = SetupModel();
            SetDownload();
            const int NewFileSize = 50000;

            var result = await model.DownloadAsync(VideoUrl, DestFile, (s, e) =>
            {
                e.Download.BeforeMuxing += (s, e) =>
                {
                    _fileSystem.File.WriteAllBytes(e.Download.Destination, new byte[NewFileSize]);
                };
            });

            Assert.Equal(DownloadStatus.Success, result);
            Assert.Equal(NewFileSize, _fileSystem.FileInfo.FromFileName(DestFile).Length);
        }

        [Fact]
        public async void DownloadAsync_Valid_ProgressUpdatedRaisedWithStatusDownloading()
        {
            using var model = SetupModel();
            SetDownload();
            var called = 0;
            DownloadStatus? status = null;

            await model.DownloadAsync(VideoUrl, DestFile, (s, e) =>
            {
                e.Download.ProgressUpdated += (s, e) =>
                {
                    called++;
                    if (called == 1)
                    {
                        status ??= e.Download.Status;
                    }
                };
            });

            Assert.NotEqual(0, called);
            Assert.Equal(DownloadStatus.Downloading, status);
        }

        [Fact]
        public async void DownloadAsync_CancelDuringProgress_StatusCancelled()
        {
            using var model = SetupModel();
            SetDownload();

            var result = await model.DownloadAsync(VideoUrl, DestFile, (s, e) =>
            {
                e.Download.ProgressUpdated += (s, e) =>
                {
                    e.Download.Cancel();
                };
            });

            Assert.Equal(DownloadStatus.Canceled, result);
        }

        [Fact]
        public async void DownloadAsync_CancelDuringProgress_TempFilesDeleted()
        {
            using var model = SetupModel();
            SetDownload();
            IList<DownloadTaskFile>? tempFiles = null;

            await model.DownloadAsync(VideoUrl, DestFile, (s, e) =>
            {
                e.Download.ProgressUpdated += (s, e) =>
                {
                    if (tempFiles == null)
                    {
                        e.Download.Cancel();
                        tempFiles = e.Download.Files;
                    }
                };
            });

            Assert.False(_fileSystem.File.Exists(tempFiles![0].Destination));
            Assert.False(_fileSystem.File.Exists(tempFiles![1].Destination));
        }

        [Fact]
        public async void DownloadAsync_CancelDuringMuxing_StatusCancelled()
        {
            using var model = SetupModel();
            SetDownload();

            var result = await model.DownloadAsync(VideoUrl, DestFile, (s, e) =>
            {
                e.Download.BeforeMuxing += (s, e) =>
                {
                    e.Download.Cancel();
                };
            });

            Assert.Equal(DownloadStatus.Canceled, result);
        }

        [Fact]
        public async void DownloadAsync_CancelDuringMuxing_TempFilesDeleted()
        {
            using var model = SetupModel();
            SetDownload();
            IList<DownloadTaskFile>? tempFiles = null;

            await model.DownloadAsync(VideoUrl, DestFile, (s, e) =>
            {
                e.Download.BeforeMuxing += (s, e) =>
                {
                    if (tempFiles == null)
                    {
                        e.Download.Cancel();
                        tempFiles = e.Download.Files;
                    }
                };
            });

            Assert.False(_fileSystem.File.Exists(tempFiles![0].Destination));
            Assert.False(_fileSystem.File.Exists(tempFiles![1].Destination));
        }

        [Fact]
        public async void DownloadAsync_FailDuringProgress_StatusFailed()
        {
            using var model = SetupModel();
            SetDownload();

            var result = await model.DownloadAsync(VideoUrl, DestFile, (s, e) =>
            {
                e.Download.ProgressUpdated += (s, e) =>
                {
                    e.Download.Fail();
                };
            });

            Assert.Equal(DownloadStatus.Failed, result);
        }

        [Fact]
        public async void DownloadAsync_FailDuringMuxing_StatusFailed()
        {
            using var model = SetupModel();
            SetDownload();

            var result = await model.DownloadAsync(VideoUrl, DestFile, (s, e) =>
            {
                e.Download.BeforeMuxing += (s, e) =>
                {
                    e.Download.Fail();
                };
            });

            Assert.Equal(DownloadStatus.Failed, result);
        }

        [Fact]
        public async void DownloadAsync_ErrorDuringProgress_ThrowsException()
        {
            using var model = SetupModel();
            SetDownload();

            async Task<DownloadStatus> Act()
            {
                return await model.DownloadAsync(VideoUrl, DestFile, (s, e) =>
                {
                    e.Download.ProgressUpdated += (s, e) =>
                    {
                        throw new Exception("BOOM!");
                    };
                });
            }

            // await Act();
            await Assert.ThrowsAsync<Exception>(Act);
        }

        [Fact]
        public async void DownloadAsync_ErrorDuringMuxing_ThrowsException()
        {
            using var model = SetupModel();
            SetDownload();

            async Task<DownloadStatus> Act()
            {
                return await model.DownloadAsync(VideoUrl, DestFile, (s, e) =>
                {
                    e.Download.BeforeMuxing += (s, e) =>
                    {
                        throw new Exception("BOOM!");
                    };
                });
            }

            await Assert.ThrowsAsync<Exception>(Act);
        }

        [Fact]
        public async void DownloadAsync_HttpError_StatusFailed()
        {
            using var model = SetupModel();
            SetDownload();
            _mockYouTube.ThrowError = true;

            var result = await model.DownloadAsync(VideoUrl, DestFile);

            Assert.Equal(DownloadStatus.Failed, result);
        }

        [Fact]
        public async void DownloadAsync_HttpError_TempFilesDeleted()
        {
            using var model = SetupModel();
            SetDownload();
            _mockYouTube.ThrowError = true;
            IList<DownloadTaskFile>? tempFiles = null;

            var result = await model.DownloadAsync(VideoUrl, DestFile, (s, e) =>
            {
                tempFiles = e.Download.Files;
            });

            Assert.False(_fileSystem.File.Exists(tempFiles![0].Destination));
            Assert.False(_fileSystem.File.Exists(tempFiles![1].Destination));
        }

        [Theory]
        [InlineData(true, true, 2)]
        [InlineData(true, false, 1)]
        [InlineData(false, true, 1)]
        public async void DownloadAsync_DownloadAudioOrVideo_HasExpectedStreams(bool downloadVideo, bool downloadAudio, int streamCount)
        {
            using var model = SetupModel();
            SetDownload();
            IList<DownloadTaskFile>? tempFiles = null;

            var result = await model.DownloadAsync(VideoUrl, DestFile, (s, e) =>
            {
                tempFiles = e.Download.Files;
            }, downloadVideo, downloadAudio);

            Assert.Equal(streamCount, tempFiles!.Count);
            Assert.Equal(DownloadStatus.Success, result);
        }

        [Fact]
        public async void DownloadAsync_DownloadNoStream_Exception()
        {
            using var model = SetupModel();
            SetDownload();

            async Task<DownloadStatus> Act() => await model.DownloadAsync(VideoUrl, DestFile, null, false, false);

            await Assert.ThrowsAsync<ArgumentException>(Act);
        }
    }
}

using HanumanInstitute.FFmpeg;
using Microsoft.Extensions.Options;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
// ReSharper disable MemberCanBePrivate.Global

namespace HanumanInstitute.Downloads.Tests
{
    public class DownloadManagerTests
    {
        private const string VideoUrl = "https://www.youtube.com/watch?v=HV9PCGoGbSA";
        private const string DestFile = "DownloadTestFile.mp4";
        const string VideoTitle = "Title";
        private readonly DownloadOptions _options = new DownloadOptions();

        public FakeFileSystemService FileSystem => _fileSystem ??= new FakeFileSystemService();
        private FakeFileSystemService? _fileSystem;

        public FakeYouTubeDownloader MockYouTube => _mockYouTube ??= new FakeYouTubeDownloader(FileSystem);
        private FakeYouTubeDownloader? _mockYouTube;

        public IYouTubeStreamSelector StreamSelector => _streamSelector ??= new YouTubeStreamSelector();
        private IYouTubeStreamSelector? _streamSelector;

        public FakeDownloadTaskFactory MockFactory => _mockFactory ??= new FakeDownloadTaskFactory();
        private FakeDownloadTaskFactory? _mockFactory;

        public Mock<IMediaMuxer> MockMuxer => _mockMuxer ?? SetupMuxer();
        private Mock<IMediaMuxer>? _mockMuxer;
        private Mock<IMediaMuxer> SetupMuxer()
        {
            _mockMuxer = new Mock<IMediaMuxer>();
            _mockMuxer.Setup(x => x.Muxe(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<ProcessOptionsEncoder?>(), It.IsAny<ProcessStartedEventHandler?>()))
                .Returns<string?, string?, string, ProcessOptionsEncoder?, ProcessStartedEventHandler?>((videoFile, audioFile, destination, _, _) =>
                {
                    FileSystem.File.Copy(videoFile ?? audioFile, destination);
                    return CompletionStatus.Success;
                });
            return _mockMuxer;
        }

        private DownloadManager SetupModel()
        {
            var taskFactory = new DownloadTaskFactory(MockYouTube, FileSystem, MockMuxer.Object);
            return new DownloadManager(taskFactory, MockYouTube, StreamSelector, Options.Create(_options));
        }

        private DownloadManager SetupModelWithFakeTask()
        {
            return new DownloadManager(MockFactory, MockYouTube, StreamSelector, Options.Create(_options));
        }

        private void SetDownload()
        {
            MockYouTube.Configure(VideoUrl, VideoTitle, new StreamManifest(new List<IStreamInfo>()
            {
                new VideoOnlyStreamInfo("https://www.youtube.com/video", Container.Mp4, new FileSize(10240), new Bitrate(), "h264", new VideoQuality(720, 30), new Resolution(960, 720)),
                new AudioOnlyStreamInfo("https://www.youtube.com/audio", Container.Mp4, new FileSize(500), new Bitrate(), "aac")
            }));
        }

        private void SetDownloadMuxed()
        {
            MockYouTube.Configure(VideoUrl, VideoTitle, new StreamManifest(new List<IStreamInfo>()
            {
                new MuxedStreamInfo("https://www.youtube.com/video", Container.Mp4, new FileSize(10240), new Bitrate(), "mp4a.40.2", "avc1.42001E", new VideoQuality(360, 24), new Resolution(480, 360))
            }));
        }

        private static async Task<StreamQueryInfo> GetQueryAsync(IDownloadManager model, bool downloadVideo = true, bool downloadAudio = true)
        {
            var streams = await model.QueryStreamInfoAsync(VideoUrl);
            return model.SelectStreams(streams, downloadVideo, downloadAudio);
        }

        [Fact]
        public async void QueryVideoAsync_ValidUrl_ReturnsTitle()
        {
            using var model = SetupModel();

            var result = await model.QueryVideoAsync(new Uri(VideoUrl));

            Assert.Equal(VideoTitle, result.Title);
        }

        [Fact]
        public async void QueryVideoAsync_ValidString_ReturnsTitle()
        {
            using var model = SetupModel();

            var result = await model.QueryVideoAsync(VideoUrl);

            Assert.Equal(VideoTitle, result.Title);
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("https://")]
        [InlineData("https://www.youtube.com/watch")]
        public async void QueryVideoAsync_InvalidString_ThrowsUriFormatException(string url)
        {
            using var model = SetupModel();

            async Task<Video> Act()
            {
                return await model.QueryVideoAsync(url);
            }

            await Assert.ThrowsAsync<UriFormatException>(Act);
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("https://")]
        [InlineData("https://www.youtube.com/watch")]
        public async void GetDownloadInfoAsync_InvalidString_ThrowsUriFormatException(string url)
        {
            using var model = SetupModel();
            MockYouTube.Configure(url, string.Empty);

            async Task<Video> Act()
            {
                return await model.QueryVideoAsync(url);
            }

            await Assert.ThrowsAsync<UriFormatException>(Act);
        }

        [Fact]
        public async void DownloadAsync_ValidUrl_RaiseDownloadAddedWithSameUrlDest()
        {
            using var model = SetupModel();
            SetDownload();
            //Uri? url = null;
            string? dest = null;
            model.DownloadAdded += (_, e) =>
            {
                // url = e?.Download?.Url;
                dest = e.Download.Destination;
            };

            var query = await GetQueryAsync(model);
            await model.DownloadAsync(query, DestFile);

            // Assert.Equal(url?.AbsoluteUri, VideoUrl);
            Assert.Equal(dest, DestFile);
        }

        [Fact]
        public async void DownloadAsync_TwoTasks_DownloadInParallel()
        {
            using var model = SetupModelWithFakeTask();
            SetDownload();

            var query = await GetQueryAsync(model);
            var tList = new int[2].Select(_ => model.DownloadAsync(query, DestFile)).ToList();
            tList.Add(Task.Run(async () =>
            {
                // Wait for all downloads to start.
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, 2);

                Assert.Equal(2, MockFactory.TotalRunning);
                MockFactory.Complete(1);
                Assert.Equal(1, MockFactory.TotalRunning);
                MockFactory.Complete(1);
                return DownloadStatus.Success;
            }));
            await Task.WhenAll(tList);

            Assert.Equal(2, MockFactory
                .TotalCreated);
            Assert.Equal(0, MockFactory.TotalRunning);
        }

        [Fact]
        public async void DownloadAsync_FiveTasks_ThreeWaitInQueue()
        {
            using var model = SetupModelWithFakeTask();

            var query = await GetQueryAsync(model);
            var tList = new int[5].Select(_ => model.DownloadAsync(query, DestFile)).ToList();
            tList.Add(Task.Run(async () =>
            {
                // Wait for all downloads to start.
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, 2);
                await Task.Delay(100).ConfigureAwait(false);
                Assert.Equal(3, MockFactory.TotalWaiting);
                MockFactory.Complete(2);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Done, 2);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, 2);
                Assert.Equal(1, MockFactory.TotalWaiting);
                MockFactory.Complete(2);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Done, 4);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, 1);
                MockFactory.Complete(1);
                return DownloadStatus.Success;
            }));
            await Task.WhenAll(tList);

            Assert.Equal(5, MockFactory.TotalCreated);
            Assert.Equal(0, MockFactory.TotalWaiting);
        }

        [Fact]
        public async void DownloadAsync_OptionThreeConcurrent_OneWaitInQueue()
        {
            using var model = SetupModelWithFakeTask();
            _options.ConcurrentDownloads = 3;

            var query = await GetQueryAsync(model);
            var tList = new int[4].Select(_ => model.DownloadAsync(query, DestFile)).ToList();
            tList.Add(Task.Run(async () =>
            {
                // Wait for all downloads to start.
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, 3);
                await Task.Delay(100).ConfigureAwait(false);
                Assert.Equal(1, MockFactory.TotalWaiting);
                MockFactory.Complete(2);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Done, 2);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, 2);
                Assert.Equal(0, MockFactory.TotalWaiting);
                MockFactory.Complete(2);
                return DownloadStatus.Success;
            }));
            await Task.WhenAll(tList);

            Assert.Equal(4, MockFactory.TotalCreated);
            Assert.Equal(0, MockFactory.TotalWaiting);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1000)]
        public async void DownloadAsync_ConcurrentDownloadsOutOfRange_ThrowsException(int concurrent)
        {
            using var model = SetupModelWithFakeTask();
            _options.ConcurrentDownloads = concurrent;

            var query = await GetQueryAsync(model);
            async Task Act()
            {
                await model.DownloadAsync(query, DestFile);
            }

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(Act);
        }

        [Fact]
        public async void DownloadAsync_ChangeCapacityWithWaitLock_NoOverflow()
        {
            using var model = SetupModelWithFakeTask();
            var maxCapacity = DownloadManager.MaxConcurrentDownloads;
            _options.ConcurrentDownloads = maxCapacity;

            var query = await GetQueryAsync(model);
            var tList = new int[maxCapacity].Select(_ => model.DownloadAsync(query, DestFile)).ToList();
            tList.Add(Task.Run(async () =>
            {
                // Wait for all downloads to start.
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Running, maxCapacity);
                await Task.Delay(100).ConfigureAwait(false);
                Assert.Equal(0, MockFactory.TotalWaiting);
                _options.ConcurrentDownloads -= 10; // The Wait() will only take effect after a download is completed.
                MockFactory.Complete(2);
                await Task.Delay(1000).ConfigureAwait(false);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Done, 2);
                MockFactory.Complete(2);
                _options.ConcurrentDownloads = maxCapacity;
                MockFactory.Complete(2);
                await WaitUntilStatus(FakeDownloadTask.FakeStatus.Done, 6);
                MockFactory.Complete(maxCapacity - 6);
                return DownloadStatus.Success;
            }));
            await Task.WhenAll(tList);

            Assert.Equal(maxCapacity, MockFactory.TotalCreated);
        }

        private async Task WaitUntilStatus(FakeDownloadTask.FakeStatus status, int count)
        {
            while (MockFactory.Total(status) < count)
            {
                await Task.Delay(100).ConfigureAwait(false);
            }
        }

        [Fact]
        public async void DownloadAsync_Valid_CreatesDestinationFile()
        {
            using var model = SetupModel();
            SetDownload();

            var query = await GetQueryAsync(model);
            await model.DownloadAsync(query, DestFile);

            Assert.True(FileSystem.File.Exists(DestFile));
        }

        [Fact]
        public async void DownloadAsync_Valid_ReturnsSuccess()
        {
            using var model = SetupModel();
            SetDownload();

            var query = await GetQueryAsync(model);
            var result = await model.DownloadAsync(query, DestFile);

            Assert.Equal(DownloadStatus.Success, result);
        }

        [Fact]
        public async void DownloadAsync_Muxing_StatusFinalizing()
        {
            using var model = SetupModel();
            SetDownload();
            DownloadStatus? status = null;

            var query = await GetQueryAsync(model);
            await model.DownloadAsync(query, DestFile, (_, e) =>
            {
                e.Download.Muxing += (_, f) =>
                {
                    status = f.Download.Status;
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

            var query = await GetQueryAsync(model);
            var result = await model.DownloadAsync(query, DestFile, (_, e) =>
            {
                e.Download.Muxing += (_, f) =>
                {
                    FileSystem.File.WriteAllBytes(f.Download.Destination, new byte[NewFileSize]);
                };
            });

            Assert.Equal(DownloadStatus.Success, result);
            Assert.Equal(NewFileSize, FileSystem.FileInfo.FromFileName(DestFile).Length);
        }

        [Fact]
        public async void DownloadAsync_Valid_ProgressUpdatedRaisedWithAllStatus()
        {
            using var model = SetupModel();
            SetDownload();
            var called = 0;
            var status = new List<DownloadStatus>();

            var query = await GetQueryAsync(model);
            await model.DownloadAsync(query, DestFile, (_, e) =>
            {
                e.Download.ProgressUpdated += (_, f) =>
                {
                    called++;
                    if (!status.Contains(f.Download.Status))
                    {
                        status.Add(f.Download.Status);
                    }
                };
            });

            Assert.NotEqual(0, called);
            Assert.Equal(DownloadStatus.Initializing, status[0]);
            Assert.Equal(DownloadStatus.Downloading, status[1]);
            Assert.Equal(DownloadStatus.Finalizing, status[2]);
            Assert.Equal(DownloadStatus.Success, status[3]);
        }

        [Fact]
        public async void DownloadAsync_CancelDuringProgress_StatusCancelled()
        {
            using var model = SetupModel();
            SetDownload();

            var query = await GetQueryAsync(model);
            var result = await model.DownloadAsync(query, DestFile, (_, e) =>
            {
                e.Download.ProgressUpdated += (_, f) =>
                {
                    f.Download.Cancel();
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

            var query = await GetQueryAsync(model);
            await model.DownloadAsync(query, DestFile, (_, e) =>
            {
                e.Download.ProgressUpdated += (_, f) =>
                {
                    if (tempFiles == null)
                    {
                        tempFiles = f.Download.Files;
                        f.Download.Cancel();
                    }
                };
            });

            Assert.False(FileSystem.File.Exists(tempFiles![0].Destination));
            Assert.False(FileSystem.File.Exists(tempFiles![1].Destination));
        }

        [Fact]
        public async void DownloadAsync_CancelDuringMuxing_StatusCancelled()
        {
            using var model = SetupModel();
            SetDownload();

            var query = await GetQueryAsync(model);
            var result = await model.DownloadAsync(query, DestFile, (_, e) =>
            {
                e.Download.Muxing += (_, f) =>
                {
                    f.Download.Cancel();
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

            var query = await GetQueryAsync(model);
            await model.DownloadAsync(query, DestFile, (_, e) =>
            {
                e.Download.Muxing += (_, f) =>
                {
                    if (tempFiles == null)
                    {
                        f.Download.Cancel();
                        tempFiles = f.Download.Files;
                    }
                };
            });

            Assert.False(FileSystem.File.Exists(tempFiles![0].Destination));
            Assert.False(FileSystem.File.Exists(tempFiles![1].Destination));
        }

        [Fact]
        public async void DownloadAsync_FailDuringProgress_StatusFailed()
        {
            using var model = SetupModel();
            SetDownload();

            var query = await GetQueryAsync(model);
            var result = await model.DownloadAsync(query, DestFile, (_, e) =>
            {
                e.Download.ProgressUpdated += (_, f) =>
                {
                    f.Download.Fail();
                };
            });

            Assert.Equal(DownloadStatus.Failed, result);
        }

        [Fact]
        public async void DownloadAsync_FailDuringMuxing_StatusFailed()
        {
            using var model = SetupModel();
            SetDownload();

            var query = await GetQueryAsync(model);
            var result = await model.DownloadAsync(query, DestFile, (_, e) =>
            {
                e.Download.Muxing += (_, f) =>
                {
                    f.Download.Fail();
                };
            });

            Assert.Equal(DownloadStatus.Failed, result);
        }

        [Fact]
        public async void DownloadAsync_ErrorDuringProgress_ThrowsException()
        {
            using var model = SetupModel();
            SetDownload();

            var query = await GetQueryAsync(model);
            async Task<DownloadStatus> Act()
            {
                return await model.DownloadAsync(query, DestFile, (_, e) =>
                {
                    e.Download.ProgressUpdated += (_, _) => throw new Exception("BOOM!");
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

            var query = await GetQueryAsync(model);
            async Task<DownloadStatus> Act()
            {
                return await model.DownloadAsync(query, DestFile, (_, e) =>
                {
                    e.Download.Muxing += (_, _) => throw new Exception("BOOM!");
                });
            }

            await Assert.ThrowsAsync<Exception>(Act);
        }

        [Fact]
        public async void DownloadAsync_HttpError_StatusFailed()
        {
            using var model = SetupModel();
            SetDownload();
            MockYouTube.ThrowError = true;

            var query = await GetQueryAsync(model);
            var result = await model.DownloadAsync(query, DestFile);

            Assert.Equal(DownloadStatus.Failed, result);
        }

        [Fact]
        public async void DownloadAsync_HttpError_TempFilesDeleted()
        {
            using var model = SetupModel();
            SetDownload();
            MockYouTube.ThrowError = true;
            IList<DownloadTaskFile>? tempFiles = null;

            var query = await GetQueryAsync(model);
            await model.DownloadAsync(query, DestFile, (_, e) =>
            {
                tempFiles = e.Download.Files;
            });

            Assert.False(FileSystem.File.Exists(tempFiles![0].Destination));
            Assert.False(FileSystem.File.Exists(tempFiles![1].Destination));
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

            var query = await GetQueryAsync(model, downloadVideo, downloadAudio);
            var result = await model.DownloadAsync(query, DestFile, (_, e) =>
            {
                tempFiles = e.Download.Files;
            });

            Assert.Equal(streamCount, tempFiles!.Count);
            Assert.Equal(DownloadStatus.Success, result);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async void DownloadAsync_DownloadMuxed_HasOneStream(bool downloadVideo, bool downloadAudio)
        {
            using var model = SetupModel();
            SetDownloadMuxed();
            IList<DownloadTaskFile>? tempFiles = null;

            var query = await GetQueryAsync(model, downloadVideo, downloadAudio);
            var result = await model.DownloadAsync(query, DestFile, (_, e) =>
            {
                tempFiles = e.Download.Files;
            });

            Assert.Equal(1, tempFiles!.Count);
            Assert.Equal(DownloadStatus.Success, result);
        }

        [Fact]
        public async void DownloadAsync_DownloadNoStream_Exception()
        {
            using var model = SetupModel();
            SetDownload();

            var query = await GetQueryAsync(model, false, false);
            async Task<DownloadStatus> Act()
            {
                return await model.DownloadAsync(query, DestFile);
            }

            await Assert.ThrowsAsync<ArgumentException>(Act);
        }
    }
}

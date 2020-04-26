using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Moq;
using YoutubeExplode;
using HanumanInstitute.CommonServices;
using HanumanInstitute.FFmpeg;
using System.Threading.Tasks;

namespace HanumanInstitute.Downloads.Tests
{
    public class DownloadTaskIntegrationTests
    {
        public static IDownloadTaskFactory SetupModel()
        {
            return new DownloadTaskFactory(new YouTubeDownloader(new YoutubeClient()), new YouTubeStreamSelector(), new FileSystemService(), new MediaMuxer(new ProcessWorkerFactory()));
        }

        [Fact]
        public async Task DownloadAsync_Valid_CreatesDestinationFile()
        {
            var model = SetupModel();
            //model.Create();
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;

namespace HanumanInstitute.Downloads.Tests
{
    public class FakeDownloadTask : IDownloadTask
    {
        public Uri Url => new Uri("");
        public string Destination => string.Empty;
        public bool DownloadVideo => true;
        public bool DownloadAudio => true;
        public DownloadTaskStatus TaskStatus => new DownloadTaskStatus();

        public FakeStatus Status { get; set; } = FakeStatus.Waiting;
        public void Done()
        {
            Status = FakeStatus.Done;
        }

        public async Task DownloadAsync()
        {
            Status = FakeStatus.Running;

            while (Status != FakeStatus.Done)
            {
                await Task.Delay(200).ConfigureAwait(false);
            }
        }

        public enum FakeStatus
        {
            Waiting,
            Running,
            Done
        }
    }
}

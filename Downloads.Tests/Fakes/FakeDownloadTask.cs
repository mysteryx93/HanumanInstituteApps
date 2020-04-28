using System;
using System.Collections.Generic;
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

        public FakeStatus Status { get; set; } = FakeStatus.Waiting;

        public IList<DownloadTaskFile> Files => new List<DownloadTaskFile>();

        public string Title { get; set; } = string.Empty;

        public double ProgressValue => 0;

        public string ProgressText { get; set; } = string.Empty;

        DownloadStatus IDownloadTask.Status => DownloadStatus.Success;

        public event DownloadTaskEventHandler? BeforeMuxing;
        public event DownloadTaskEventHandler? ProgressUpdated;

        public void Cancel()
        { }

        public void Fail()
        { }

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

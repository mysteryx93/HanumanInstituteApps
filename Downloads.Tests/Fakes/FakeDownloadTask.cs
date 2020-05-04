using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HanumanInstitute.Downloads.Tests
{
    public class FakeDownloadTask : IDownloadTask
    {
        public StreamQueryInfo Query => new StreamQueryInfo();

        public string Destination => string.Empty;

        public FakeStatus Status { get; set; } = FakeStatus.Waiting;

        public IList<DownloadTaskFile> Files => new List<DownloadTaskFile>();

        public string Title { get; set; } = string.Empty;

        public double ProgressValue => 0;

        public string ProgressText { get; set; } = string.Empty;

        DownloadStatus IDownloadTask.Status => DownloadStatus.Success;

#pragma warning disable 67
        public event MuxeTaskEventHandler? BeforeMuxing;
        public event DownloadTaskEventHandler? ProgressUpdated;
#pragma warning restore 67

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

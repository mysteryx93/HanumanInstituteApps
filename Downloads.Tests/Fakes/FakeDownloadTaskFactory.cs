using System;
using System.Collections.Concurrent;
using System.Linq;

namespace HanumanInstitute.Downloads.Tests
{
    public class FakeDownloadTaskFactory : IDownloadTaskFactory
    {
        public ConcurrentBag<FakeDownloadTask> Instances { get; private set; } = new ConcurrentBag<FakeDownloadTask>();

        public IDownloadTask Create(Uri url, string destination, bool downloadVideo, bool downloadAudio, DownloadOptions options)
        {
            var result = new FakeDownloadTask();
            Instances.Add(result);
            return result;
        }

        public int TotalCreated => Instances.Count;

        public int TotalWaiting => Total(FakeDownloadTask.FakeStatus.Waiting);

        public int TotalRunning => Total(FakeDownloadTask.FakeStatus.Running);

        public int TotalDone => Total(FakeDownloadTask.FakeStatus.Done);

        public int Total(FakeDownloadTask.FakeStatus status) => Instances.Where(x => x.Status == status).Count();

        private object _lock = new object();

        // Completes n amount of tasks.
        public void Complete(int n = 1)
        {
            for (var i = 0; i < n; i++)
            {
                lock (_lock)
                {
                    Instances.First(x => x.Status == FakeDownloadTask.FakeStatus.Running).Done();
                }
            }
        }
    }
}

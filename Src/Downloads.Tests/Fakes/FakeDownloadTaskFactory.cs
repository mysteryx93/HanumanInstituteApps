using System.Collections.Concurrent;

namespace HanumanInstitute.Downloads.Tests;

public class FakeDownloadTaskFactory : IDownloadTaskFactory
{
    public ConcurrentBag<FakeDownloadTask> Instances { get; private set; } = new();

    public IDownloadTask Create(StreamQueryInfo streamQuery, string destination)
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

    private readonly object _lock = new();

    // Completes n amount of tasks.
    public void Complete(int n)
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
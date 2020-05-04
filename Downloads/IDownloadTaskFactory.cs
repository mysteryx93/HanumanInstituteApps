using System;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Creates new instances of IDownloadTasko.
    /// </summary>
    public interface IDownloadTaskFactory
    {
        /// <summary>
        /// Creates a new IDownloadTaskInfo initialized with specified values.
        /// </summary>
        /// <param name="streamQuery">The analyzed download query.</param>
        /// <param name="destination">The destination path to store the file locally.</param>
        /// <returns>The new IDownloadTask instance.</returns>
        IDownloadTask Create(StreamQueryInfo streamQuery, string destination);
    }
}

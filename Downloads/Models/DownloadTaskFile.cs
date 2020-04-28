using System;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Contains information about a file download operation.
    /// </summary>
    public class DownloadTaskFile
    {
        //public DownloadTaskFile() { }

        public DownloadTaskFile(StreamType type, Uri url, string destination, object stream, long length)
        {
            Type = type;
            Url = url;
            Destination = destination;
            Stream = stream;
            Length = length;
        }

        /// <summary>
        /// Gets the type of data stream being downloaded.
        /// </summary>
        public StreamType Type { get; }
        /// <summary>
        /// Gets the URL to download from.
        /// </summary>
        public Uri Url { get; }
        /// <summary>
        /// Gets the destination path to store the file locally.
        /// </summary>
        public string Destination { get; }
        /// <summary>
        /// Gets the download stream.
        /// </summary>
        public object Stream { get; }
        /// <summary>
        /// Gets the stream total length in bytes.
        /// </summary>
        public long Length { get; }
        /// <summary>
        /// Gets the approximate amount of bytes downloaded so far.
        /// </summary>
        internal long Downloaded { get; set; }
        /// <summary>
        /// Gets whether the file download is completed.
        /// </summary>
        public bool Done { get; set; }
    }
}

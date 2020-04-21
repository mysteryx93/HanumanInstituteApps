using System;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Contains information about a file download operation.
    /// </summary>
    public class DownloadFileInfo
    {
        public DownloadFileInfo() { }

        public DownloadFileInfo(StreamType type, Uri url, string destination, object stream, long length)
        {
            this.Type = type;
            this.Url = url;
            this.Destination = destination;
            this.Stream = stream;
            this.Length = length;
        }

        /// <summary>
        /// Gets or sets the type of data stream being downloaded.
        /// </summary>
        public StreamType Type { get; set; }
        /// <summary>
        /// Gets or sets the URL to download from.
        /// </summary>
        public Uri Url { get; set; }
        /// <summary>
        /// Gets or sets the destination path to store the file locally.
        /// </summary>
        public string Destination { get; set; }
        /// <summary>
        /// Gets or sets the download stream.
        /// </summary>
        public object Stream { get; set; }
        /// <summary>
        /// Gets or sets the stream total length in bytes.
        /// </summary>
        public long Length { get; set; }
        /// <summary>
        /// Gets or sets the amount of bytes downloaded so far.
        /// </summary>
        public long Downloaded { get; set; }
        /// <summary>
        /// Gets or sets whether the file download is completed.
        /// </summary>
        public bool Done { get; set; }
    }
}

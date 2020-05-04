using System;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Contains information about a file download operation.
    /// </summary>
    public class DownloadTaskFile
    {
        //public DownloadTaskFile() { }

        public DownloadTaskFile(bool hasVideo, bool hasAudio, Uri url, string destination, object stream, long length)
        {
            HasVideo = hasVideo;
            HasAudio = hasAudio;
            Url = url;
            Destination = destination;
            Stream = stream;
            Length = length;
        }

        /// <summary>
        /// Gets whether the file contains a video stream.
        /// </summary>
        public bool HasVideo { get; }
        /// <summary>
        /// Gets whether the file contains an audio stream.
        /// </summary>
        public bool HasAudio { get; }
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
    }
}

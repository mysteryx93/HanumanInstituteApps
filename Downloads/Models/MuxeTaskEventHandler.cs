using System;

namespace HanumanInstitute.Downloads
{
    /// <summary>
    /// Represents the method that will handle a muxe task event, to merge audio and video files into a new container.
    /// </summary>
    /// <param name="e">Information about the download task.</param>
    public delegate void MuxeTaskEventHandler(object sender, MuxeTaskEventArgs e);

    /// <summary>
    /// Contains data for download task event.
    /// </summary>
    public class MuxeTaskEventArgs : DownloadTaskEventArgs
    {
        public MuxeTaskEventArgs(IDownloadTask downloadTask, string? videoFile, string? audioFile) : base(downloadTask)
        {
            VideoFile = videoFile;
            AudioFile = audioFile;
        }

        public string? VideoFile { get; set; }

        public string? AudioFile { get; set; }

        public string Destination => Download.Destination;
    }
}

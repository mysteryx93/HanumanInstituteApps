using System;
using HanumanInstitute.FFmpeg;

namespace HanumanInstitute.MediaMuxer.Models
{
    /// <summary>
    /// Represents a file stream to display in the UI.
    /// </summary>
    public class UIFileStream : MediaStream
    {
        public bool IsChecked { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the UIFileStream class.
        /// </summary>
        public UIFileStream()
        { }

        /// <summary>
        /// Initializes a new instance of the UIFileStream class and copy from specified FFmpegStreamInfo class.
        /// </summary>
        /// <param name="item">The FFmpegStreamInfo to copy info from.</param>
        /// <param name="path">The file this file stream belongs to.</param>
        public UIFileStream(MediaStreamInfo item, string path)
        {
            Index = item.Index;
            Format = item.Format;
            Type = item.StreamType;
            Path = path;
        }

        public string Display => string.Format("{0}[{1}]:{2} - {3}", Type.ToString(), Index, Format, Path);

        public string DisplayShort => string.Format("{0}[{1}]:{2}", Type.ToString(), Index, Format);
    }
}

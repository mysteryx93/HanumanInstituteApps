using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmergenceGuardian.FFmpeg;

namespace AudioVideoMuxer {
    /// <summary>
    /// Represents a file stream to display in the UI.
    /// </summary>
    public class UIFileStream : FFmpegStream {
        public bool IsChecked { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the UIFileStream class.
        /// </summary>
        public UIFileStream() {
        }

        /// <summary>
        /// Initializes a new instance of the UIFileStream class and copy from specified FFmpegStreamInfo class.
        /// </summary>
        /// <param name="item">The FFmpegStreamInfo to copy info from.</param>
        /// <param name="path">The file this file stream belongs to.</param>
        public UIFileStream(FFmpegStreamInfo item, string path) {
            this.Index = item.Index;
            this.Format = item.Format;
            this.Type = item.StreamType;
            this.Path = path;
        }

        public string Display {
            get {
                return string.Format("{0}[{1}]:{2} - {3}", Type.ToString(), Index, Format, Path);
            }
        }

        public string DisplayShort {
            get {
                return string.Format("{0}[{1}]:{2}", Type.ToString(), Index, Format);
            }
        }
    }
}

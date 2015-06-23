using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business {
    /// <summary>
    /// Represents the current status of a download.
    /// </summary>
    public enum DownloadStatus {
        Waiting,
        Initializing,
        Downloading,
        Done,
        Canceled,
        Failed
    }
}

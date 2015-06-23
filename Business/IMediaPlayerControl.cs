using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Business {
    public interface IMediaPlayerControl {
        event EventHandler MediaOpened;
        event EventHandler MediaResume;
        event EventHandler MediaPause;
        event EventHandler Closed;

        void Show();
        void Hide();
        void Close();
        Task OpenFileAsync(string fileName);
        bool AllowClose { get; set; }
        bool Loop { get; set; }
        double Position { get; set; }
        double Duration { get; }
        Dispatcher Dispatcher { get; }
    }
}

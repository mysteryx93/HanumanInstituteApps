using System;
using System.Threading.Tasks;
namespace Business {
    public interface IMediaPlayerBusiness {
        bool AllowClose { get; set; }
        void Close();
        DataAccess.Media CurrentVideo { get; set; }
        bool IsPlaying { get; }
        bool IsAvailable { get; }
        bool IgnorePos { get; set; }
        event EventHandler NowPlaying;
        event EventHandler Pause;
        event EventHandler PlayNext;
        Task PlayVideoAsync(DataAccess.Media video);
        double Position { get; }
        Task SetPositionAsync(double pos);
        event EventHandler PositionChanged;
        event EventHandler Resume;
        void Show();
        void Hide();
        void SetPath();
    }
}

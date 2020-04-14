using System;
using System.Threading.Tasks;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {
    public interface IMediaPlayer {
        bool IsAutoPitchEnabled { get; set; }
        bool AllowClose { get; set; }
        bool IsPlaying { get; }
        bool IsAvailable { get; }
        bool IgnorePos { get; set; }
        double? StartPos { get; }
        double? EndPos { get; }
        double Position { get; set; }
        DataAccess.Media CurrentVideo { get; set; }
        event EventHandler NowPlaying;
        event EventHandler PositionChanged;
        event EventHandler PlayNext;
        event EventHandler Pause;
        event EventHandler Resume;
        Task PlayVideoAsync(DataAccess.Media video, bool enableAutoPitch);
        Task PlayVideoAsync(string filePath);
        Task SetPositionAsync(double pos);
        void Show();
        void Hide();
        void Close();
        void SetPath();
    }
}

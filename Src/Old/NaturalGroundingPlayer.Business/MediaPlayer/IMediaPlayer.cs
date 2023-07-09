using System;
using System.Threading.Tasks;
using HanumanInstitute.NaturalGroundingPlayer.Models;

namespace HanumanInstitute.NaturalGroundingPlayer.MediaPlayer
{
    public interface IMediaPlayer
    {
        bool IsAutoPitchEnabled { get; set; }
        bool AllowClose { get; set; }
        bool IsPlaying { get; }
        bool IsAvailable { get; }
        bool IgnorePos { get; set; }
        double? StartPos { get; }
        double? EndPos { get; }
        double Position { get; set; }
        Media CurrentVideo { get; set; }
        event EventHandler NowPlaying;
        event EventHandler PositionChanged;
        event EventHandler PlayNext;
        event EventHandler Pause;
        event EventHandler Resume;
        Task PlayVideoAsync(Media video, bool enableAutoPitch);
        Task PlayVideoAsync(string filePath);
        Task SetPositionAsync(double pos);
        void Show();
        void Hide();
        void Close();
        void SetPath();
    }
}

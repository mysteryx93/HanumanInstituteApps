using System;

namespace HanumanInstitute.Player432hz.Business
{
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class PlayingInfo
    {
        public string FileName { get; set; } = string.Empty;
        public double Position { get; set; }
        public double Duration { get; set; }
    }
}

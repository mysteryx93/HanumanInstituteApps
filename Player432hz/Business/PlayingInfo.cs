
namespace HanumanInstitute.Player432hz.Business {
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class PlayingInfo {
        public string FileName { get; set; }
        public double Position { get; set; }
        public double Duration { get; set; }
    }
}

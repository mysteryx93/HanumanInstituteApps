using ReactiveUI;

namespace HanumanInstitute.Player432Hz.Business;

public class PlayingInfo : ReactiveObject
{
    public string FileName
    {
        get => _fileName;
        set => this.RaiseAndSetIfChanged(ref _fileName, value, nameof(FileName));
    }
    private string _fileName = string.Empty;

    public double Position
    {
        get => _position;
        set => this.RaiseAndSetIfChanged(ref _position, value, nameof(Position));
    }
    private double _position;

    public double Duration
    {
        get => _duration;
        set => this.RaiseAndSetIfChanged(ref _duration, value, nameof(Duration));
    }
    private double _duration;
}

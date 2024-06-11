using System.ComponentModel;
using Avalonia.Markup.Xaml;
using HanumanInstitute.BassAudio;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.PowerliminalsPlayer.Views;

namespace HanumanInstitute.PowerliminalsPlayer;

public class App : CommonApplication<MainView>
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    protected override INotifyPropertyChanged InitViewModel() => ViewModelLocator.Main;

    protected override void BackgroundInit()
    {
        BassDevice.Instance.InitPlugins();
        BassDevice.Instance.VerifyPlugins();
        BassDevice.Instance.InitMidi(ViewModelLocator.AppPathService.MidiSoundsFile);
    }
}

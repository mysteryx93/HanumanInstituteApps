using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Markup.Xaml;
using HanumanInstitute.BassAudio;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.Player432Hz.Views;
using ManagedBass;
using Splat;

namespace HanumanInstitute.Player432Hz;

public class App : CommonApplication<MainView>
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    protected override INotifyPropertyChanged InitViewModel() => ViewModelLocator.Main;

    protected override void BackgroundInit()
    {
        BassDevice.Instance.InitPlugins();
        BassDevice.Instance.VerifyPlugins();
        // Add support for MIDI files, if 'midisounds.sf2' is present in app folder.
        BassDevice.Instance.InitMidi(ViewModelLocator.AppPathService.MidiSoundsFile);
    }
}

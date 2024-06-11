using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Markup.Xaml;
using HanumanInstitute.Converter432Hz.Views;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using ManagedBass;
using Splat;

namespace HanumanInstitute.Converter432Hz;

public class App : CommonApplication<MainView>
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    protected override INotifyPropertyChanged InitViewModel() => ViewModelLocator.Main;

    protected override void BackgroundInit()
    {
        BassDevice.Instance.InitNoSound();
        BassDevice.Instance.VerifyPlugins();
        // Add support for MIDI files, if 'midisounds.sf2' is present in app folder.
        BassDevice.Instance.InitMidi(ViewModelLocator.AppPathService.MidiSoundsFile);
    }
}


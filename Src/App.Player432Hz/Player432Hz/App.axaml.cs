using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;
using HanumanInstitute.Player432Hz.Views;
using ManagedBass;

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
        // It will not be bundled by default. Windows may have its default sound fonts.
        var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var path = Path.Combine(dir, "midisounds.sf2");
        // ASCII on Windows, UTF8 on others.
        var ptr = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Marshal.StringToHGlobalAnsi(path)
            : NativeUtf8FromString(path);
        ManagedBass.Bass.Configure(Configuration.MidiDefaultFont, ptr);
    }
    
    private static IntPtr NativeUtf8FromString(string managedString) {
        var len = Encoding.UTF8.GetByteCount(managedString);
        var buffer = new byte[len + 1];
        Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
        var nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
        Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
        return nativeUtf8;
    }
}

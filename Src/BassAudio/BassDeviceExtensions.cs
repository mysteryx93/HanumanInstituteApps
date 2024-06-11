using System.Runtime.InteropServices;
using System.Text;
using ManagedBass;

namespace HanumanInstitute.BassAudio;

public static class BassDeviceExtensions
{
    /// <summary>
    /// Adds support for MIDI files, if specific soundfont file exists. 
    /// </summary>
    /// <param name="soundFontPath">The path to a soundfound (sf2) file.</param>
    public static void InitMidi(this BassDevice device, string soundFontPath)
    {
        // ASCII on Windows, UTF8 on others.
        var ptr = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? Marshal.StringToHGlobalAnsi(soundFontPath)
            : NativeUtf8FromString(soundFontPath);
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

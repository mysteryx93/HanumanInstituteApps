using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace HanumanInstitute.Services;

/// <inheritdoc />
public class ProcessService : IProcessService
{
    /// <inheritdoc />
    public ProcessWrapper Start(string fileName, string arguments) => new(Process.Start(fileName, arguments));

    /// <inheritdoc />
    public ProcessWrapper Start(string fileName) => new(Process.Start(fileName));

    /// <inheritdoc />
    public ProcessWrapper StartNotePad(string arguments) => Start("notepad.exe", arguments);

    /// <inheritdoc />
    public ProcessWrapper[] GetProcessesByName(string appName) => Process.GetProcessesByName(appName).Select(p => new ProcessWrapper(p)).ToArray();
    
    /// <inheritdoc />
    public void OpenBrowserUrl(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true, Verb = "open" });
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }
}

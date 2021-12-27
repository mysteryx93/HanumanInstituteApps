using System.Diagnostics;
using System.Linq;

// ReSharper disable CheckNamespace
namespace HanumanInstitute.Common.Services;

/// <inheritdoc />
public class ProcessService : IProcessService
{
    /// <inheritdoc />
    public ProcessWrapper Start(string fileName, string arguments) => new ProcessWrapper(Process.Start(fileName, arguments));

    /// <inheritdoc />
    public ProcessWrapper Start(string fileName) => new ProcessWrapper(Process.Start(fileName));

    /// <inheritdoc />
    public ProcessWrapper StartNotePad(string arguments) => Start("notepad.exe", arguments);

    /// <inheritdoc />
    public ProcessWrapper[] GetProcessesByName(string appName) => Process.GetProcessesByName(appName).Select(p => new ProcessWrapper(p)).ToArray();
}

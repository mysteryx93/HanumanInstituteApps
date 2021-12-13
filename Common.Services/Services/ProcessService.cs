using System;
using System.Diagnostics;
using System.Linq;

namespace HanumanInstitute.Common.Services;

/// <summary>
/// Allows the creation and management of system processes.
/// </summary>
public class ProcessService : IProcessService
{
    public ProcessService() { }

    /// <summary>
    /// Starts specified application with specified arguments.
    /// </summary>
    /// <param name="fileName">The application to start.</param>
    /// <param name="arguments">The arguments to pass to the application.</param>
    /// <returns>The newly created process.</returns>
    public ProcessWrapper Start(string fileName, string arguments) => new ProcessWrapper(Process.Start(fileName, arguments));

    /// <summary>
    /// Starts specified application.
    /// </summary>
    /// <param name="fileName">The application to start.</param>
    /// <returns>The newly created process.</returns>
    public ProcessWrapper Start(string fileName) => new ProcessWrapper(Process.Start(fileName));

    /// <summary>
    /// Starts the NotePad application.
    /// </summary>
    /// <param name="arguments">The arguments to pass to NotePad.</param>
    /// <returns>The newly created process.</returns>
    public ProcessWrapper StartNotePad(string arguments) => Start("notepad.exe", arguments);

    /// <summary>
    /// Returns all processes that have specified name.
    /// </summary>
    /// <param name="appName">The name of the processes to look for.</param>
    /// <returns>An array of processes.</returns>
    public ProcessWrapper[] GetProcessesByName(string appName) => Process.GetProcessesByName(appName).Select(p => new ProcessWrapper(p)).ToArray();
}
namespace HanumanInstitute.Services;

/// <summary>
/// Allows the creation and management of system processes.
/// </summary>
public interface IProcessService
{
    /// <summary>
    /// Starts specified application with specified arguments.
    /// </summary>
    /// <param name="fileName">The application to start.</param>
    /// <returns>The newly created process.</returns>
    ProcessWrapper Start(string fileName);

    /// <summary>
    /// Starts specified application.
    /// </summary>
    /// <param name="fileName">The application to start.</param>
    /// <param name="arguments">The arguments to pass to the application.</param>
    /// <returns>The newly created process.</returns>
    ProcessWrapper Start(string fileName, string arguments);

    /// <summary>
    /// Starts the NotePad application.
    /// </summary>
    /// <param name="arguments">The arguments to pass to NotePad.</param>
    /// <returns>The newly created process.</returns>
    ProcessWrapper StartNotePad(string arguments);

    /// <summary>
    /// Returns all processes that have specified name.
    /// </summary>
    /// <param name="appName">The name of the processes to look for.</param>
    /// <returns>An array of processes.</returns>
    ProcessWrapper[] GetProcessesByName(string appName);

    /// <summary>
    /// Opens specified URL in the default browser.
    /// </summary>
    /// <param name="url">The URL to navigate to.</param>
    void OpenBrowserUrl(string url);
}

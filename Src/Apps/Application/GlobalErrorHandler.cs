using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using ReactiveUI;

namespace HanumanInstitute.Apps;

/// <summary>
/// A global error handler that displays a message box when unhandled errors occur.
/// </summary>
public class GlobalErrorHandler : IObserver<Exception>
{
    private IDialogService? _dialogService;

    public static GlobalErrorHandler Instance => _instance ??= new GlobalErrorHandler();
    private static GlobalErrorHandler? _instance;

    /// <summary>
    /// Sets the global error handler to display all errors.
    /// We set dependencies later because we want to initialize the ViewModelLocator in parallel to the View,
    /// and DefaultExceptionHandler must be set before creating the view.
    /// </summary>
    public static void BeginInit() => RxApp.DefaultExceptionHandler = Instance;

    /// <summary>
    /// Sets the dependencies for handling and displaying errors.
    /// </summary>
    /// <param name="dialogService">The service used to display dialogs.</param>
    /// <param name="ownerVm">The ViewModel of the View in which to display error messages.</param>
    public static void EndInit(IDialogService dialogService, INotifyPropertyChanged? ownerVm)
    {
        Instance._dialogService = dialogService;
        Instance.OwnerVm = ownerVm;
    }

    /// <summary>
    /// Gets or sets the ViewModel of the View in which to display error messages.
    /// </summary>
    public INotifyPropertyChanged? OwnerVm { get; set; }

    /// <inheritdoc />
    public void OnNext(Exception error) => ShowError(error);

    /// <inheritdoc />
    public void OnError(Exception error) { }

    /// <inheritdoc />
    public void OnCompleted() { }

    private async void ShowError(Exception error)
    {
        if (_dialogService != null && OwnerVm != null)
        {
            await _dialogService.ShowMessageBoxAsync(OwnerVm!, error.ToString(), "Application Error", MessageBoxButton.Ok,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Sets the log path before using <see cref="ShowErrorLog"/>.
    /// </summary>
    public static string? LogPath { get; set; }

    /// <summary>
    /// Dump error to log file and open default text editor.
    /// </summary>
    /// <param name="ex">The exception to output in a log file.</param>
    public static void ShowErrorLog(Exception ex)
    {
        if (LogPath.HasValue())
        {
            var logDir = Path.GetDirectoryName(LogPath)!;
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            File.WriteAllText(LogPath, ex.ToString());
            new Process { StartInfo = new ProcessStartInfo(LogPath) { UseShellExecute = true } }.Start();
            // Text editor gets killed after 1 second in the IDE, but stays open if app is run directly. 
            Thread.Sleep(TimeSpan.FromSeconds(1));
        }
    }
}

using System.Windows.Input;
using HanumanInstitute.Common.Services;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;

namespace HanumanInstitute.Player432hz.ViewModels;

public class AboutViewModel : ICloseable
{
    private readonly IEnvironmentService _environment;
    private readonly IProcessService _processService;
    /// <inheritdoc />
    public event EventHandler? RequestClose;

    public AboutViewModel(IEnvironmentService environment, IProcessService processService)
    {
        _environment = environment;
        _processService = processService;
    }

    /// <summary>
    /// Returns the name of the application.
    /// </summary>
    public string AppName => "432hz Player";

    /// <summary>
    /// Returns the description of the application.
    /// </summary>
    public string AppDescription => "Plays music in 432hz";

    /// <summary>
    /// Returns the version of the application.
    /// </summary>
    public Version AppVersion => _environment.AppVersion;

    /// <summary>
    /// Opens specified URL in the default browser.
    /// </summary>
    public ICommand OpenBrowser => _openBrowser ??= ReactiveCommand.Create<string>(OpenBrowserImpl);
    private ICommand? _openBrowser;
    private void OpenBrowserImpl(string url) => _processService.OpenBrowserUrl(url);
    
    /// <summary>
    /// Closes the window.
    /// </summary>
    public ICommand Close => _close ??= ReactiveCommand.Create(CloseImpl);
    private ICommand? _close;
    private void CloseImpl()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}

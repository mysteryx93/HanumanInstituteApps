using System.ComponentModel;
using HanumanInstitute.Common.Services;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;

namespace HanumanInstitute.Common.Avalonia.App;

/// <inheritdoc />
public class AppUpdateService<TSettings> : IAppUpdateService 
    where TSettings : SettingsDataBase, new()
{
    private readonly IDialogService _dialogService;
    private readonly ISettingsProvider<TSettings> _settings;
    private readonly IEnvironmentService _environment;
    private readonly IUpdateService _updateService;
    private readonly IProcessService _processService;
    private readonly IAppInfo _appInfo;

    public AppUpdateService(ISettingsProvider<TSettings> settings, IDialogService dialogService, IEnvironmentService environment,
        IUpdateService updateService, IProcessService processService, IAppInfo appInfo)
    {
        _settings = settings;
        _dialogService = dialogService;
        _environment = environment;
        _updateService = updateService;
        _processService = processService;
        _appInfo = appInfo;
    }

    /// <inheritdoc />
    public async Task CheckForUpdatesAsync(INotifyPropertyChanged owner)
    {
        var interval = _settings.Value.CheckForUpdates;
        var lastCheck = _environment.Now - _settings.Value.LastCheckForUpdate;
        if (interval != UpdateInterval.Never &&
            (lastCheck == null ||
             (interval == UpdateInterval.Daily && lastCheck > TimeSpan.FromDays(1)) ||
             (interval == UpdateInterval.Biweekly && lastCheck > TimeSpan.FromDays(3.5)) ||
             (interval == UpdateInterval.Weekly && lastCheck > TimeSpan.FromDays(7)) ||
             (interval == UpdateInterval.Bimonthly && lastCheck > TimeSpan.FromDays(15)) ||
             (interval == UpdateInterval.Monthly && lastCheck > TimeSpan.FromDays(30))))
        {
            _updateService.FileFormat = _appInfo.GitHubFileFormat;
            var version = await _updateService.GetLatestVersionAsync().ConfigureAwait(true);
            if (version != null)
            {
                _settings.Value.LastCheckForUpdate = _environment.Now;
                _settings.Save();
            }
            if (version > _environment.AppVersion)
            {
                await Task.Delay(1).ConfigureAwait(true);
                var result = await _dialogService.ShowMessageBoxAsync(owner,
                    $"Version {version} is available!\n\nWould you like to download it now?".ReplaceLineEndings(),
                    "Update Available!", MessageBoxButton.YesNo, MessageBoxImage.Information).ConfigureAwait(false);
                if (result == true)
                {
                    _processService.OpenBrowserUrl(_updateService.GetDownloadInfoLink());
                }
            }
        }
    }
}

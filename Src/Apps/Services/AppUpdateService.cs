using System.ComponentModel;
using HanumanInstitute.Apps.AdRotator;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;

namespace HanumanInstitute.Apps;

/// <inheritdoc />
public class AppUpdateService<TSettings> : IAppUpdateService 
    where TSettings : SettingsBase, new()
{
    private readonly IDialogService _dialogService;
    private readonly ISettingsProvider<TSettings> _settings;
    private readonly IEnvironmentService _environment;
    private readonly IHanumanInstituteHttpClient _httpClient;
    private readonly IProcessService _processService;
    private readonly IAppInfo _appInfo;
    private readonly IAdRotatorViewModel _adRotator;

    public AppUpdateService(ISettingsProvider<TSettings> settings, IDialogService dialogService, IEnvironmentService environment,
        IHanumanInstituteHttpClient httpClient, IProcessService processService, IAppInfo appInfo, IAdRotatorViewModel adRotator)
    {
        _settings = settings;
        _dialogService = dialogService;
        _environment = environment;
        _httpClient = httpClient;
        _processService = processService;
        _appInfo = appInfo;
        _adRotator = adRotator;
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
            var version = await _httpClient.QueryVersionAsync();
            if (version != null)
            {
                _settings.Value.LastCheckForUpdate = _environment.Now;
                _settings.Save();
                
                if (version.LatestVersion > _environment.AppVersion)
                {
                    await Task.Delay(1).ConfigureAwait(true);
                    var result = await _dialogService.ShowMessageBoxAsync(owner,
                        $"Version {version.LatestVersion} is available!\n\nWould you like to download it now?".ReplaceLineEndings(),
                        "Update Available!", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (result == true)
                    {
                        _processService.OpenBrowserUrl(_appInfo.DownloadInfoUrl);
                    }
                }
                if (version.AdsLastUpdated > _adRotator.AdInfo.LastUpdated)
                {
                    await _adRotator.LoadFromServerAsync();
                }
            }
        }
    }
}

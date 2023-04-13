using HanumanInstitute.Apps;
using HanumanInstitute.Services;

namespace HanumanInstitute.YangDownloader.ViewModels;

public sealed class AboutViewModel : AboutViewModelBase<AppSettingsData>
{
    public AboutViewModel(IAppInfo appInfo, IEnvironmentService environment,
        ISettingsProvider<AppSettingsData> settings, IUpdateService updateService, ILicenseValidator licenseValidator, IDialogService dialogService) :
        base(appInfo, environment, settings, updateService, licenseValidator, dialogService)
    {
    }
}

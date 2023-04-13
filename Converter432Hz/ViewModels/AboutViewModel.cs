using HanumanInstitute.Apps;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.Services;

namespace HanumanInstitute.Converter432Hz.ViewModels;

public class AboutViewModel : AboutViewModelBase<AppSettingsData>
{
    public AboutViewModel(IAppInfo appInfo, IEnvironmentService environment,
        ISettingsProvider<AppSettingsData> settings, IUpdateService updateService, ILicenseValidator licenseValidator,
        IDialogService dialogService) :
        base(appInfo, environment, settings, updateService, licenseValidator, dialogService)
    {
    }
}

﻿using HanumanInstitute.Common.Avalonia.App;

namespace HanumanInstitute.YangDownloader.ViewModels;

public sealed class AboutViewModel : AboutViewModelBase<AppSettingsData>
{
    public AboutViewModel(IAppInfo appInfo, IEnvironmentService environment,
        ISettingsProvider<AppSettingsData> settings, IUpdateService updateService) :
        base(appInfo, environment, settings, updateService)
    {
    }
}

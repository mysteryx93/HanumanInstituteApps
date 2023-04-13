using Avalonia;
using Avalonia.Input.Platform;
using HanumanInstitute.Apps.AdRotator;
using HanumanInstitute.Common.Services;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.Services;
using Splat;

namespace HanumanInstitute.Apps;

public static class SplatContainerExtensions
{
    /// <summary>
    /// Registers CommonWpfApp classes into the IoC container.
    /// </summary>
    /// <param name="services">The IoC services container.</param>
    public static IMutableDependencyResolver AddCommonAvaloniaApp<TSettings>(this IMutableDependencyResolver services)
        where TSettings : SettingsBase, new()
    {
        services.CheckNotNull(nameof(services));

        // SplatRegistrations.RegisterLazySingleton<GlobalErrorHandler>();
        services.Register<IAppUpdateService>(() => new AppUpdateService<TSettings>(
            GetService<ISettingsProvider<TSettings>>(), 
            GetService<IDialogService>(),
            GetService<IEnvironmentService>(),
            GetService<IUpdateService>(),
            GetService<IProcessService>(),
            GetService<IAppInfo>()));
        
        services.Register<IClipboard>(() => Application.Current!.Clipboard);

        // SplatRegistrations.Register<IAppUpdateService, AppUpdateService<TSettings>>();
        SplatRegistrations.RegisterLazySingleton<ILicenseValidator, LicenseValidator>();
        SplatRegistrations.RegisterLazySingleton<IAppPathServiceBase, AppPathServiceBase>();
        SplatRegistrations.RegisterLazySingleton<AdRotatorViewModel>();
        SplatRegistrations.SetupIOC();

        return services;
    }

    private static T GetService<T>() => Locator.Current.GetService<T>()!;
}

using System;
using HanumanInstitute.Common.Services;
using Splat;

namespace HanumanInstitute.Common.Avalonia.App;

public static class WindsorContainerExtensions
{
    /// <summary>
    /// Registers CommonWpfApp classes into the IoC container.
    /// </summary>
    /// <param name="services">The IoC services container.</param>
    public static IMutableDependencyResolver AddCommonWpfApp(this IMutableDependencyResolver services)
    {
        services.CheckNotNull(nameof(services));

        //services.Register(Component.For<SettingsProvider>().ImplementedBy<SettingsProvider>().LifeStyle.Singleton);
        // services.Register(
        //     Component.For<SplashViewModel>()
        //     .LifeStyle.Transient);

        return services;
    }
}

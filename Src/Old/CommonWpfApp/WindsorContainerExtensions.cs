using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using HanumanInstitute.CommonServices;
using HanumanInstitute.CommonWpfApp.ViewModels;

namespace HanumanInstitute.CommonWpfApp
{
    public static class WindsorContainerExtensions
    {
        /// <summary>
        /// Registers CommonWpfApp classes into the IoC container.
        /// </summary>
        /// <param name="services">The IoC services container.</param>
        public static IWindsorContainer AddCommonWpfApp(this IWindsorContainer services)
        {
            services.CheckNotNull(nameof(services));

            //services.Register(Component.For<SettingsProvider>().ImplementedBy<SettingsProvider>().LifeStyle.Singleton);
            services.Register(
                Component.For<SplashViewModel>()
                .LifeStyle.Transient);

            return services;
        }
    }
}

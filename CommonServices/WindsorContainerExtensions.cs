using System;
using System.IO.Abstractions;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace HanumanInstitute.CommonServices
{
    public static class WindsorContainerExtensions
    {
        /// <summary>
        /// Registers CommonServices classes into the IoC container.
        /// </summary>
        /// <param name="services">The IoC services container.</param>
        public static IWindsorContainer AddCommonServices(this IWindsorContainer services)
        {
            services.CheckNotNull(nameof(services));

            //services.Register(Component.For<SettingsProvider>().ImplementedBy<SettingsProvider>().LifeStyle.Singleton);
            services.Register(
                Component.For<ISerializationService>()
                .ImplementedBy<SerializationService>()
                .LifeStyle.Transient);

            services.Register(
                Component.For<IFileSystemService>()
                .ImplementedBy<FileSystemService>()
                .LifeStyle.Transient);

            services.Register(
                Component.For<IFileSystem>()
                .ImplementedBy<FileSystem>()
                .LifeStyle.Transient);

            services.Register(
                Component.For<IProcessService>()
                .ImplementedBy<ProcessService>()
                .LifeStyle.Transient);

            services.Register(
                Component.For<IEnvironmentService>()
                .ImplementedBy<EnvironmentService>()
                .LifeStyle.Transient);

            services.Register(
                Component.For<IWindowsApiService>()
                .ImplementedBy<WindowsApiService>()
                .LifeStyle.Transient);

            return services;
        }
    }
}

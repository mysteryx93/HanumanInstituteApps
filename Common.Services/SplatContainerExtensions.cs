using System.IO.Abstractions;
using Splat;

namespace HanumanInstitute.Common.Services;

public static class SplatContainerExtensions
{
    /// <summary>
    /// Registers Common.Services classes into the IoC container.
    /// </summary>
    /// <param name="services">The IoC services container.</param>
    public static IMutableDependencyResolver AddCommonServices(this IMutableDependencyResolver services)
    {
        services.CheckNotNull(nameof(services));

        //services.Register(Component.For<SettingsProvider>().ImplementedBy<SettingsProvider>().LifeStyle.Singleton);
        SplatRegistrations.Register<ISerializationService, SerializationService>();
        SplatRegistrations.Register<IFileSystemService, FileSystemService>();
        SplatRegistrations.Register<IFileSystem, FileSystem>();
        SplatRegistrations.Register<IProcessService, ProcessService>();
        SplatRegistrations.Register<IEnvironmentService, EnvironmentService>();
        SplatRegistrations.Register<IWindowsApiService, WindowsApiService>();
        SplatRegistrations.SetupIOC();

        return services;
    }
}

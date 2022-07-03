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

        SplatRegistrations.RegisterLazySingleton<ISerializationService, SerializationService>();
        SplatRegistrations.RegisterLazySingleton<IFileSystemService, FileSystemService>();
        SplatRegistrations.RegisterLazySingleton<IFileSystem, FileSystem>();
        SplatRegistrations.RegisterLazySingleton<IProcessService, ProcessService>();
        SplatRegistrations.RegisterLazySingleton<IEnvironmentService, EnvironmentService>();
        SplatRegistrations.RegisterLazySingleton<IWindowsApiService, WindowsApiService>();
        SplatRegistrations.RegisterLazySingleton<IUpdateService, UpdateService>();
        SplatRegistrations.RegisterLazySingleton<ISyndicationFeedService, SyndicationFeedService>();
        SplatRegistrations.SetupIOC();

        return services;
    }
}

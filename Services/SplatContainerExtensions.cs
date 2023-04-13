using System.IO.Abstractions;
using System.Net.Http;
using Splat;

namespace HanumanInstitute.Services;

public static class SplatContainerExtensions
{
    /// <summary>
    /// Registers Services classes into the IoC container.
    /// </summary>
    /// <param name="services">The IoC services container.</param>
    public static IMutableDependencyResolver AddCommonServices(this IMutableDependencyResolver services)
    {
        services.CheckNotNull(nameof(services));

        services.Register(() => new HttpClient());
        SplatRegistrations.RegisterLazySingleton<ISerializationService, SerializationService>();
        SplatRegistrations.RegisterLazySingleton<IFileSystemService, FileSystemService>();
        SplatRegistrations.RegisterLazySingleton<IFileSystem, FileSystem>();
        SplatRegistrations.RegisterLazySingleton<IProcessService, ProcessService>();
        SplatRegistrations.RegisterLazySingleton<IEnvironmentService, EnvironmentService>();
        SplatRegistrations.RegisterLazySingleton<IWindowsApiService, WindowsApiService>();
        SplatRegistrations.RegisterLazySingleton<IUpdateService, UpdateService>();
        SplatRegistrations.RegisterLazySingleton<ISyndicationFeedService, SyndicationFeedService>();
        SplatRegistrations.RegisterLazySingleton<IPathFixer, PathFixer>();
        SplatRegistrations.RegisterLazySingleton<IRandomGenerator, RandomGenerator>();

        SplatRegistrations.SetupIOC();

        return services;
    }
}

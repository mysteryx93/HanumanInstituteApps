using Splat;
using YoutubeExplode;

namespace HanumanInstitute.Downloads;

public static class SplatContainerExtensions
{
    /// <summary>
    /// Registers Services classes into the IoC container.
    /// </summary>
    /// <param name="services">The IoC services container.</param>
    /// <param name="options">Download manager configuration.</param>
    public static IMutableDependencyResolver AddDownloads(this IMutableDependencyResolver services, IOptions<DownloadOptions>? options = null)
    {
        services.CheckNotNull(nameof(services));

        services.RegisterLazySingleton(() => options ?? Options.Create(new DownloadOptions()));
        services.Register(() => new YoutubeClient());
        
        SplatRegistrations.RegisterLazySingleton<IDownloadManager, DownloadManager>();
        SplatRegistrations.Register<IDownloadTaskFactory, DownloadTaskFactory>();
        SplatRegistrations.Register<IYouTubeDownloader, YouTubeDownloader>();
        SplatRegistrations.Register<IYouTubeStreamSelector, YouTubeStreamSelector>();
        SplatRegistrations.SetupIOC();

        return services;
    }
}

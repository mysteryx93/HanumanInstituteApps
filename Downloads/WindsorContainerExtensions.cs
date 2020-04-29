using System;
using Microsoft.Extensions.Options;
using Castle.Windsor;
using Castle.MicroKernel.Registration;

namespace HanumanInstitute.Downloads
{
    public static class WindsorContainerExtensions
    {
        /// <summary>
        /// Registers Downloads classes into the IoC container.
        /// </summary>
        /// <param name="services">The IoC services container.</param>
        public static IWindsorContainer AddDownloads(this IWindsorContainer services, IOptions<DownloadOptions>? options = null)
        {
            services.CheckNotNull(nameof(services));

            services.Register(
                Component.For<IOptions<DownloadOptions>>()
                .Instance(options ?? Options.Create(new DownloadOptions())));

            services.Register(
                Component.For<IDownloadManager>()
                .ImplementedBy<DownloadManager>()
                .LifeStyle.Singleton);

            services.Register(
                Component.For<IDownloadTaskFactory>()
                .ImplementedBy<DownloadTaskFactory>()
                .LifeStyle.Transient);

            services.Register(
                Component.For<IYouTubeDownloader>()
                .ImplementedBy<YouTubeDownloader>()
                .LifeStyle.Transient);

            services.Register(
                Component.For<IYouTubeStreamSelector>()
                .ImplementedBy<YouTubeStreamSelector>()
                .LifeStyle.Transient);

            services.Register(
                Component.For<YoutubeExplode.YoutubeClient>()
                .LifeStyle.Transient);

            return services;
        }
    }
}

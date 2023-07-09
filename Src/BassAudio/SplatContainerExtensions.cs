using Splat;

namespace HanumanInstitute.BassAudio;

/// <summary>
/// Registers services into the IOC container. 
/// </summary>
public static class SplatContainerExtensions
{
    /// <summary>
    /// Registers Services classes into the IoC container.
    /// </summary>
    /// <param name="services">The IoC services container.</param>
    public static IMutableDependencyResolver AddBassAudio(this IMutableDependencyResolver services)
    {
        services.CheckNotNull(nameof(services));

        SplatRegistrations.Register<IPitchDetector, PitchDetector>();
        SplatRegistrations.Register<IPitchDetectorWithCache, PitchDetectorWithCache>();
        SplatRegistrations.Register<IAudioEncoder, AudioEncoder>();
        // SplatRegistrations.Register<IBassDevice, BassDevice>();
        SplatRegistrations.SetupIOC();

        return services;
    }
}

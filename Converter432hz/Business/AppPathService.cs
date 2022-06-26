using System.Linq;

namespace HanumanInstitute.Converter432hz.Business;

/// <summary>
/// Manages the file system paths used by the application.
/// </summary>
public class AppPathService : IAppPathService
{
    private readonly IEnvironmentService _environment;
    private readonly IFileSystemService _fileSystem;
    private readonly IBassDevice _bassDevice;

    public AppPathService(IEnvironmentService environmentService, IFileSystemService fileSystemService, IBassDevice bassDevice)
    {
        _environment = environmentService;
        _fileSystem = fileSystemService;
        _bassDevice = bassDevice;
    }

    /// <summary>
    /// Returns all valid audio extensions
    /// </summary>
    public IReadOnlyList<string> AudioExtensions => _audioExtensions ??= 
        _bassDevice.SupportedExtensions.SelectMany(x => x.Extensions).Distinct().OrderBy(x => x).ToList();
    private IReadOnlyList<string>? _audioExtensions;

    /// <summary>
    /// Returns the path where unhandled exceptions are logged.
    /// </summary>
    public string UnhandledExceptionLogPath => _fileSystem.Path.Combine(_environment.ApplicationDataPath, @"Natural Grounding Player\Log.txt");
    /// <summary>
    /// Returns the path where the 432hz Player settings file is stored.
    /// </summary>
    public string Player432hzConfigFile => _fileSystem.Path.Combine(_environment.ApplicationDataPath, @"Natural Grounding Player\432hzConverterConfig.xml");
}

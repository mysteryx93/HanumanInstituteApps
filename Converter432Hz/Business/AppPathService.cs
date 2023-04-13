using System.Linq;
using HanumanInstitute.Apps;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;

namespace HanumanInstitute.Converter432Hz.Business;

/// <inheritdoc cref="AppPathServiceBase" />
public class AppPathService : AppPathServiceBase, IAppPathService
{
    private readonly IEnvironmentService _environment;
    private readonly IFileSystemService _fileSystem;
    private readonly IBassDevice _bassDevice;

    public AppPathService(IEnvironmentService environmentService, IFileSystemService fileSystemService, IBassDevice bassDevice) :
        base(environmentService, fileSystemService)
    {
        _environment = environmentService;
        _fileSystem = fileSystemService;
        _bassDevice = bassDevice;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> AudioExtensions => _audioExtensions ??= 
        _bassDevice.SupportedExtensions.SelectMany(x => x.Extensions).Distinct().OrderBy(x => x).ToList();
    private IReadOnlyList<string>? _audioExtensions;

    /// <inheritdoc />
    public string ConfigFile => _configFile ??= GetStoragePath("432HzConverterConfig.json");
    private string? _configFile;
}

using System.Linq;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;

namespace HanumanInstitute.Converter432hz.Business;

/// <inheritdoc />
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

    /// <inheritdoc />
    public IReadOnlyList<string> AudioExtensions => _audioExtensions ??= 
        _bassDevice.SupportedExtensions.SelectMany(x => x.Extensions).Distinct().OrderBy(x => x).ToList();
    private IReadOnlyList<string>? _audioExtensions;

    /// <inheritdoc />
    public string UnhandledExceptionLogPath => _unhandledExceptionLogPath ??=
        Combine(_environment.ApplicationDataPath, @"Natural Grounding Player\Log.txt");
    private string? _unhandledExceptionLogPath;
    
    /// <inheritdoc />
    public string ConfigFile => _configFile ??= 
        Combine(_environment.ApplicationDataPath, @"Hanuman Institute\432hzConverterConfig.xml");
    private string? _configFile;
    
    /// <inheritdoc />
    public string OldConfigFile => _oldConfigFile ??= 
        _fileSystem.Path.Combine(_environment.ApplicationDataPath, @"Natural Grounding Player\432hzConverterConfig.xml");
    private string? _oldConfigFile; // Created file with '\' char instead of creating in folder for Linux.
    
    /// <summary>
    /// Combines two paths while replacing folder separator chars with platform-specific char.
    /// </summary>
    private string Combine(string part1, string part2)
    {
        part1 = part1.Replace('\\', _fileSystem.Path.DirectorySeparatorChar);
        part2 = part2.Replace('\\', _fileSystem.Path.DirectorySeparatorChar);
        return _fileSystem.Path.Combine(part1, part2);
    }
}

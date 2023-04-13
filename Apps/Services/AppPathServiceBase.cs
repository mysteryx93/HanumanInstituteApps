using HanumanInstitute.Common.Services;

namespace HanumanInstitute.Apps;

/// <inheritdoc />
public class AppPathServiceBase : IAppPathServiceBase
{
    private readonly IEnvironmentService _environment;
    private readonly IFileSystemService _fileSystem;

    public AppPathServiceBase(IEnvironmentService environmentService, IFileSystemService fileSystemService)
    {
        _environment = environmentService;
        _fileSystem = fileSystemService;
    }
    
    /// <inheritdoc />
    public string UnhandledExceptionLogPath => _unhandledExceptionLogPath ??= GetStoragePath("Log.txt");
    private string? _unhandledExceptionLogPath;

    /// <inheritdoc />
    public string AdInfoPath => _adInfoPath ??= GetStoragePath("Ads.json");
    private string? _adInfoPath;

    /// <inheritdoc />
    public string GetStoragePath(string fileName) => Combine(StorageFolder, fileName);
    
    /// <inheritdoc />
    public string StorageFolder => Combine(_environment.ApplicationDataPath, @"Hanuman Institute");
    
    /// <summary>
    /// Combines two paths while replacing folder separator chars with platform-specific char.
    /// </summary>
    protected string Combine(string part1, string part2)
    {
        part1 = part1.Replace('\\', _fileSystem.Path.DirectorySeparatorChar);
        part2 = part2.Replace('\\', _fileSystem.Path.DirectorySeparatorChar);
        return _fileSystem.Path.Combine(part1, part2);
    }
}

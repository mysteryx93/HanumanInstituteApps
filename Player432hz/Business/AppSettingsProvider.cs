namespace HanumanInstitute.Player432hz.Business;

/// <summary>
/// Contains custom application settings for 432hz Player.
/// </summary>
public class AppSettingsProvider : SettingsProvider<AppSettingsData>
{
    private readonly IAppPathService _appPath;
    private readonly IFileSystemService _fileSystem;

    public AppSettingsProvider(ISerializationService serializationService, IAppPathService appPath, IFileSystemService fileSystem) :
        base(serializationService)
    {
        _appPath = appPath;
        _fileSystem = fileSystem;

        Load();
    }

    /// <summary>
    /// Loads settings file if present, or creates a new object with default values.
    /// </summary>
    public sealed override AppSettingsData Load()
    {
        // If upgrading from older version, move settings from old location to new location.
        if (!_fileSystem.File.Exists(_appPath.ConfigFile) && _fileSystem.File.Exists(_appPath.OldConfigFile))
        {
            _fileSystem.EnsureDirectoryExists(_appPath.ConfigFile);
            _fileSystem.File.Move(_appPath.OldConfigFile, _appPath.ConfigFile);
        }
        
        return Load(_appPath.ConfigFile);
    }

    /// <summary>
    /// Saves settings into an XML file.
    /// </summary>
    public override void Save() => Save(_appPath.ConfigFile);

    protected override AppSettingsData GetDefault() => new AppSettingsData();
}

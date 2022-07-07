using System.IO.Abstractions;
using System.Xml.Serialization;
using HanumanInstitute.Common.Services;
using HanumanInstitute.PowerliminalsPlayer.Models;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

/// <summary>
/// Manages the PowerliminalsPlayer application settings.
/// </summary>
[XmlRoot("PowerliminalsPlayer")]
public sealed class AppSettingsProvider : SettingsProvider<AppSettingsData>
{
    private readonly IAppPathService _appPath;
    private readonly IFileSystemService _fileSystem;

    public AppSettingsProvider(ISerializationService serializationService, IAppPathService appPath, IFileSystemService fileSystem) : base(serializationService)
    {
        _appPath = appPath;
        _fileSystem = fileSystem;

        Load();
    }

    public override AppSettingsData Load()
    {
        // If upgrading from older version, move settings from old location to new location.
        if (!_fileSystem.File.Exists(_appPath.ConfigFile) && _fileSystem.File.Exists(_appPath.OldConfigFile))
        {
            _fileSystem.EnsureDirectoryExists(_appPath.ConfigFile);
            _fileSystem.File.Move(_appPath.OldConfigFile, _appPath.ConfigFile);
        }
        
        return Load(_appPath.ConfigFile);
    }

    public override void Save() => base.Save(_appPath.ConfigFile);

    protected override AppSettingsData GetDefault() => new AppSettingsData();
}

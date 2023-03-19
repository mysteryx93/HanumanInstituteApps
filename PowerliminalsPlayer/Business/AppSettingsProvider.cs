using System.Text.Json.Serialization.Metadata;
using System.Xml.Serialization;
using Avalonia.Controls;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

/// <summary>
/// Manages the PowerliminalsPlayer application settings.
/// </summary>
[XmlRoot("PowerliminalsPlayer")]
public sealed class AppSettingsProvider : SettingsProviderBase<AppSettingsData>
{
    private readonly IAppPathService _appPath;
    private readonly IFileSystemService _fileSystem;

    public AppSettingsProvider(ISerializationService serializationService, IAppPathService appPath, IFileSystemService fileSystem, IJsonTypeInfoResolver? serializerContext) :
        base(serializationService, serializerContext)
    {
        _appPath = appPath;
        _fileSystem = fileSystem;

        Load();
    }
    
    /// <inheritdoc />
    public override string FilePath => _appPath.ConfigFile;

    public override AppSettingsData Load()
    {
        if (Design.IsDesignMode) { return GetDefault(); }
        
        // If upgrading from older version, move settings from old location to new location.
        if (!_fileSystem.File.Exists(_appPath.ConfigFile) && _fileSystem.File.Exists(_appPath.OldConfigFile))
        {
            _fileSystem.EnsureDirectoryExists(_appPath.ConfigFile);
            _fileSystem.File.Move(_appPath.OldConfigFile, _appPath.ConfigFile);
        }
        
        return Load(_appPath.ConfigFile);
    }

    public override void Save() => base.Save(_appPath.ConfigFile);

    protected override AppSettingsData GetDefault() => new() 
    {
        Width = 680,
        Height = 380
    };
}

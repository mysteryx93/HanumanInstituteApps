using System.Text.Json.Serialization.Metadata;
using Avalonia.Controls;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

/// <summary>
/// Manages the PowerliminalsPlayer application settings.
/// </summary>
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
        
        return Load(_appPath.ConfigFile);
    }

    public override void Save() => base.Save(_appPath.ConfigFile);

    protected override AppSettingsData GetDefault() => new() 
    {
        Width = 720,
        Height = 400
    };
}

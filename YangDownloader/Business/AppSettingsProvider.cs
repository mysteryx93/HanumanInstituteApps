using Avalonia.Controls;

namespace HanumanInstitute.YangDownloader.Business;

/// <summary>
/// Contains custom application settings for 432hz Player.
/// </summary>
public sealed class AppSettingsProvider : SettingsProviderBase<AppSettingsData>
{
    private readonly IAppPathService _appPath;

    public AppSettingsProvider(ISerializationService serializationService, IAppPathService appPath) :
        base(serializationService)
    {
        _appPath = appPath;

        Load();
    }
    
    /// <inheritdoc />
    public override string FilePath => _appPath.ConfigFile;

    /// <summary>
    /// Loads settings file if present, or creates a new object with default values.
    /// </summary>
    public override AppSettingsData Load()
    {
        if (Design.IsDesignMode) { return GetDefault(); }
        
        return Load(_appPath.ConfigFile);
    }

    /// <summary>
    /// Saves settings into an XML file.
    /// </summary>
    public override void Save() => Save(_appPath.ConfigFile);

    protected override AppSettingsData GetDefault() => new()
    {
        Width = 540,
        Height = 400
    };
}

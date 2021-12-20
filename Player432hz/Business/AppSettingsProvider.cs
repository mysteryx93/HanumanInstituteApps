using HanumanInstitute.Common.Services;
using HanumanInstitute.Player432hz.Models;

namespace HanumanInstitute.Player432hz.Business;

/// <summary>
/// Contains custom application settings for 432hz Player.
/// </summary>
public class AppSettingsProvider : SettingsProvider<AppSettingsData>
{
    private readonly IAppPathService _appPath;

    public AppSettingsProvider(ISerializationService serializationService, IAppPathService appPath) :
        base(serializationService)
    {
        _appPath = appPath;

        Load();
    }

    /// <summary>
    /// Loads settings file if present, or creates a new object with default values.
    /// </summary>
    public sealed override AppSettingsData Load() => Load(_appPath.Player432hzConfigFile);

    /// <summary>
    /// Saves settings into an XML file.
    /// </summary>
    public override void Save() => Save(_appPath.Player432hzConfigFile);

    protected override AppSettingsData GetDefault() => new AppSettingsData();
}

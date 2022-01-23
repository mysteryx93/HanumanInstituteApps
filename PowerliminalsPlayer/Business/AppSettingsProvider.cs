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

    public AppSettingsProvider(ISerializationService serializationService, IAppPathService appPath) : base(serializationService)
    {
        _appPath = appPath;
    }

    public override AppSettingsData Load() => base.Load(_appPath.SettingsPath);

    public override void Save() => base.Save(_appPath.SettingsPath);

    protected override AppSettingsData GetDefault() => new AppSettingsData();
}

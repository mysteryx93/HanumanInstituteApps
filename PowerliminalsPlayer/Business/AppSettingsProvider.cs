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
    public const int DefaultWidth = 730;
    public const int DefaultHeight = 410;

    public AppSettingsProvider(ISerializationService serializationService, IAppPathService appPath) : base(serializationService)
    {
        _appPath = appPath;
    }

    public override AppSettingsData Load()
    {
        base.Load(_appPath.SettingsPath);
        if (Value.Width <= 0 && Value.Height <= 0)
        {
            Value.Width = DefaultWidth;
            Value.Height = DefaultHeight;
        }
        return Value;
    }

    public override void Save() => base.Save(_appPath.SettingsPath);

    protected override AppSettingsData GetDefault() => new AppSettingsData();
}

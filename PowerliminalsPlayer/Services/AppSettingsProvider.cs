using System;
using System.Xml.Serialization;
using HanumanInstitute.CommonServices;
using HanumanInstitute.PowerliminalsPlayer.Models;

namespace HanumanInstitute.PowerliminalsPlayer.Services
{
    /// <summary>
    /// Manages the PowerliminalsPlayer application settings.
    /// </summary>
    [XmlRoot("PowerliminalsPlayer")]
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class AppSettingsProvider : SettingsProvider<AppSettingsData>
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
}

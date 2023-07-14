using System.Xml.Serialization;
using HanumanInstitute.NaturalGroundingPlayer.Models;
using HanumanInstitute.CommonServices;
using HanumanInstitute.NaturalGroundingPlayer.Services;

namespace HanumanInstitute.NaturalGroundingPlayer.Configuration
{
    /// <summary>
    /// Manages the NaturalGroundingPlayer application settings.
    /// </summary>
    [XmlRoot("PowerliminalsPlayer")]
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class AppSettingsProvider : SettingsProvider<AppSettingsData>, IAppSettingsProvider
    {
        private readonly IAppPathService _appPath;

        public AppSettingsProvider(ISerializationService serializationService, IAppPathService appPath) : base(serializationService)
        {
            _appPath = appPath;
        }

        public override AppSettingsData Load() => base.Load(_appPath.SettingsPath);

        public override void Save() => base.Save(_appPath.SettingsPath);

        protected override AppSettingsData GetDefault()
        {
            return new AppSettingsData
            {
                NaturalGroundingFolder = _appPath.DefaultNaturalGroundingFolder,
                SvpPath = _appPath.GetDefaultSvpPath() // Auto-detect SVP path.
            };
        }
    }
}

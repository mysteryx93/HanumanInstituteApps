using System;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.Player432hz.Business
{
    /// <summary>
    /// Contains custom application settings for 432hz Player.
    /// </summary>
    public class SettingsProvider : GenericSettingsProvider<SettingsData>, ISettingsProvider
    {
        private readonly IAppPathService _appPath;

        public SettingsProvider(ISerializationService serializationService, IAppPathService appPath) :
            base(serializationService)
        {
            _appPath = appPath;

            Load();
        }

        /// <summary>
        /// Loads settings file if present, or creates a new object with default values.
        /// </summary>
        public SettingsData Load() => Load(_appPath.Player432hzConfigFile);

        /// <summary>
        /// Saves settings into an XML file.
        /// </summary>
        public void Save() => Save(_appPath.Player432hzConfigFile);

        protected override SettingsData GetDefault() => new SettingsData();
    }
}

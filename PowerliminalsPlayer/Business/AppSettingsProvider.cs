using System;
using System.Xml.Serialization;
using HanumanInstitute.CommonServices;


namespace HanumanInstitute.PowerliminalsPlayer.Business
{
    /// <summary>
    /// Manages the PowerliminalsPlayer application settings.
    /// </summary>
    [XmlRoot("PowerliminalsPlayer")]
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public class AppSettingsProvider : GenericSettingsProvider<AppSettingsFile>
    {
        protected IAppPathService appPath;

        public AppSettingsProvider(ISerializationService serializationService, IAppPathService appPath) : base(serializationService)
        {
            this.appPath = appPath;
        }

        public void Load() => base.Load(appPath.SettingsPath);

        public void Save() => base.Save(appPath.SettingsPath);

        protected override AppSettingsFile GetDefault() => new AppSettingsFile();

        /// <summary>
        /// Returns Data.Zoom
        /// </summary>
        //public double Zoom => Current?.Zoom ?? 1;
    }
}

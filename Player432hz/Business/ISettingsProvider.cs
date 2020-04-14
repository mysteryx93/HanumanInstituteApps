using System;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.Player432hz.Business
{
    /// <summary>
    /// Contains custom application settings for 432hz Player.
    /// </summary>
    public interface ISettingsProvider : IGenericSettingsProvider<SettingsData>
    {
        /// <summary>
        /// Loads settings file if present, or creates a new object with default values.
        /// </summary>
        SettingsData Load();

        /// <summary>
        /// Saves settings into an XML file.
        /// </summary>
        void Save();
    }
}
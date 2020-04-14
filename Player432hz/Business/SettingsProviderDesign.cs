using System;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.Player432hz.Business
{
    /// <summary>
    /// Contains custom application settings for 432hz Player in design mode.
    /// </summary>
    public class SettingsProviderDesign : ISettingsProvider
    {
        public SettingsData Current { get; set; } = new SettingsData();

#pragma warning disable 67
        public event EventHandler BeforeLoad;
        public event EventHandler Loaded;
        public event EventHandler Saving;
        public event EventHandler Saved;
#pragma warning restore 67

        public SettingsData Load() => new SettingsData();

        public SettingsData Load(string path) => new SettingsData();

        public void Save() { }

        public void Save(string path) { }
    }
}

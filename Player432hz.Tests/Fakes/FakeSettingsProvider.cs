using System;
using HanumanInstitute.Player432hz.Business;

namespace Player432hz.Tests
{
    public class FakeSettingsProvider : ISettingsProvider
    {
        public SettingsData Current { get; set; } = new SettingsData();

        public event EventHandler Loaded;
        public event EventHandler Saving;
        public event EventHandler Saved;

        public SettingsData Load()
        {
            Loaded?.Invoke(this, new EventArgs());
            return Current;
        }

        public SettingsData Load(string path)
        {
            return Load();
        }

        public void Save()
        {
            Saving?.Invoke(this, new EventArgs());
            Saved?.Invoke(this, new EventArgs());
        }

        public void Save(string path)
        {
            Save();
        }
    }
}

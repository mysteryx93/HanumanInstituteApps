using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanumanInstitute.Test
{
    public class SettingsProvider : GenericSettingsProvider, ISettingsProvider
    {
        public void Test() { }
    }

    public interface ISettingsProvider
    {
        void Test();
    }

    public class GenericSettingsProvider : IGenericSettingsProvider
    {
        public event EventHandler Saved;

        public void Save()
        {
            Saved?.Invoke(this, new EventArgs());
        }
    }

    public interface IGenericSettingsProvider
    {
        event EventHandler Saved;
        void Save();
    }
}

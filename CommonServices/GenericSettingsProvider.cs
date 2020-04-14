using System;
using HanumanInstitute.CommonServices.Properties;
using HanumanInstitute.CommonServices.Validation;

namespace HanumanInstitute.CommonServices
{
    /// <summary>
    /// Handles generic settings features such as loading, saving and validating data.
    /// </summary>
    /// <typeparam name="T">The type of data in which to store settings.</typeparam>
    public class GenericSettingsProvider<T> : IGenericSettingsProvider<T> 
        where T : class, new()
    {
        private readonly ISerializationService serialization;

        public GenericSettingsProvider(ISerializationService serializationService)
        {
            this.serialization = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
        }

        /// <summary>
        /// Gets or sets the current settings.
        /// </summary>
        public T Current { get; set; }

        /// <summary>
        /// Occurs after settings are loaded.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Occurs before settings are saved.
        /// </summary>
        public event EventHandler Saving;

        /// <summary>
        /// Occurs after settings are saved.
        /// </summary>
        public event EventHandler Saved;

        /// <summary>
        /// Loads settings file if present, or creates a new object with default values.
        /// </summary>
        public T Load(string path)
        {
            T result = null;
            try
            {
                result = serialization.DeserializeFromFile<T>(path);
            }
            catch (InvalidOperationException)
            {
            }
            Current = (result != null && result.Validate() == null) ? result : GetDefault();
            Loaded?.Invoke(this, new EventArgs());
            return Current;
        }

        /// <summary>
        /// Saves settings into an XML file.
        /// </summary>
        /// <param name="path">The file path to save the serialized settings object to.</param>
        public void Save(string path)
        {
            Saving?.Invoke(this, new EventArgs());
            if (Current == null) { throw new NullReferenceException(Resources.GenericSettingsProviderCurrentNull); }
            if (Current.Validate() != null) { throw new Exception(Resources.GenericSettingsProviderValidationErrors); }

            serialization.SerializeToFile<T>(Current, path);
            Saved?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// When overriden in a devired class, returns the default settings values.
        /// </summary>
        protected virtual T GetDefault() => new T();
    }
}

using System;

namespace HanumanInstitute.CommonServices
{
    /// <summary>
    /// Handles generic settings features such as loading, saving and validating data.
    /// </summary>
    /// <typeparam name="T">The type of data in which to store settings.</typeparam>
    public interface IGenericSettingsProvider<T> 
        where T : class, new()
    {
        /// <summary>
        /// Gets or sets the current settings.
        /// </summary>
        T Current { get; set; }

        /// <summary>
        /// Occurs after settings are loaded.
        /// </summary>
        event EventHandler Loaded;

        /// <summary>
        /// Occurs before settings are saved.
        /// </summary>
        event EventHandler Saving;

        /// <summary>
        /// Occurs after settings are saved.
        /// </summary>
        event EventHandler Saved;

        /// <summary>
        /// Loads settings file if present, or creates a new object with default values.
        /// </summary>
        T Load(string path);

        /// <summary>
        /// Saves settings into specified path.
        /// </summary>
        /// <param name="path">The file path to save the serialized settings object to.</param>
        void Save(string path);
    }
}
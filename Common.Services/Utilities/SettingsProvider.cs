using System;
using System.ComponentModel.DataAnnotations;
using HanumanInstitute.Common.Services.Properties;
using HanumanInstitute.Common.Services.Validation;

namespace HanumanInstitute.Common.Services;

/// <summary>
/// Handles generic settings features such as loading, saving and validating data.
/// </summary>
/// <typeparam name="T">The type of data in which to store settings.</typeparam>
public class SettingsProvider<T> : ISettingsProvider<T>
    where T : class, new()
{
    private readonly ISerializationService _serialization;

    public SettingsProvider(ISerializationService serializationService)
    {
        _serialization = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
    }

    /// <summary>
    /// Gets or sets the current settings.
    /// </summary>
    public T Value { get; set; } = new T();

    /// <summary>
    /// Occurs after settings are loaded.
    /// </summary>
    public event EventHandler? Loaded;

    /// <summary>
    /// Occurs before settings are saved.
    /// </summary>
    //public event EventHandler? Saving;

    /// <summary>
    /// Occurs after settings are saved.
    /// </summary>
    public event EventHandler? Saved;

    public virtual T Load()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Loads settings file if present, or creates a new object with default values.
    /// </summary>
    public T Load(string path)
    {
        T? result = null;
        try
        {
            result = _serialization.DeserializeFromFile<T>(path);
        }
        catch (InvalidOperationException) { }
        Value = (result != null && result.Validate() == null) ? result : GetDefault();
        Loaded?.Invoke(this, new EventArgs());
        return Value;
    }

    public virtual void Save()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Saves settings into an XML file.
    /// </summary>
    /// <param name="path">The file path to save the serialized settings object to.</param>
    /// <exception cref="NullReferenceException">Current property is null.</exception>
    /// <exception cref="ValidationException">Settings contain validation errors.</exception>
    public void Save(string path)
    {
        // Saving?.Invoke(this, new EventArgs());
        if (Value == null) { throw new NullReferenceException(Resources.GenericSettingsProviderCurrentNull); }
        if (Value.Validate() != null) { throw new ValidationException(Resources.GenericSettingsProviderValidationErrors); }

        _serialization.SerializeToFile<T>(Value, path);
        Saved?.Invoke(this, new EventArgs());
    }

    /// <summary>
    /// When overriden in a devired class, returns the default settings values.
    /// </summary>
    protected virtual T GetDefault() => new T();
}
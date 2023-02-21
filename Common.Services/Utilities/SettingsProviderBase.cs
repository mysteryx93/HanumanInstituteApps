using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using HanumanInstitute.Common.Services.Properties;
using HanumanInstitute.Common.Services.Validation;
using HanumanInstitute.MvvmDialogs;

// ReSharper disable CheckNamespace
namespace HanumanInstitute.Common.Services;

/// <summary>
/// Handles generic settings features such as loading, saving and validating data.
/// </summary>
/// <typeparam name="T">The type of data in which to store settings.</typeparam>
public abstract class SettingsProviderBase<T> : ISettingsProvider<T>
    where T : class, new()
{
    private readonly ISerializationService _serialization;

    protected SettingsProviderBase(ISerializationService serializationService)
    {
        _serialization = serializationService ?? throw new ArgumentNullException(nameof(serializationService));
    }

    /// <summary>
    /// Gets or sets the current settings.
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            _value = value;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
    private T _value = new T();
    
    /// <summary>
    /// Gets the path where settings are stored.
    /// </summary>
    public abstract string FilePath { get; }

    /// <summary>
    /// Occurs when the Value property has changed.
    /// </summary>
    public event EventHandler? Changed;

    /// <summary>
    /// Occurs after settings are saved.
    /// </summary>
    public event EventHandler? Saving;

    /// <summary>
    /// Loads settings data. Must be implemented in derived class. 
    /// </summary>
    /// <returns>The loaded settings.</returns>
    public abstract T Load();

    /// <summary>
    /// Loads settings file if present, or creates a new object with default values.
    /// </summary>
    public T Load(string path)
    {
        List<string> errors = new();
        T? result = null;
        try
        {
            result = _serialization.DeserializeFromFile<T>(path);
        }
        catch (InvalidOperationException e)
        {
            errors.Add(e.Message);
        }
        catch (DirectoryNotFoundException) { }
        catch (FileNotFoundException) { }

        var valErrors = result?.Validate();
        if (valErrors != null)
        {
            foreach (var item in valErrors)
            {
                errors.Add(item.ErrorMessage ?? string.Empty);
            }
        }
        if (errors.Any())
        {
            var msg = "Failed to load configuration file " + FilePath + Environment.NewLine + string.Join(Environment.NewLine, errors);
            throw new InvalidOperationException(msg);
        }
        
        Value = result ?? GetDefault();
        Changed?.Invoke(this, EventArgs.Empty);
        return Value;
    }

    /// <summary>
    /// Saves settings data. Must be implemented in derived class. 
    /// </summary>
    public abstract void Save();

    /// <summary>
    /// Saves settings into an XML file.
    /// </summary>
    /// <param name="path">The file path to save the serialized settings object to.</param>
    /// <exception cref="NullReferenceException">Current property is null.</exception>
    /// <exception cref="ValidationException">Settings contain validation errors.</exception>
    public void Save(string path)
    {
        Saving?.Invoke(this, EventArgs.Empty);

        // Saving?.Invoke(this, new EventArgs());
        if (Value == null) { throw new NullReferenceException(Resources.GenericSettingsProviderCurrentNull); }
        if (Value.Validate() != null) { throw new ValidationException(Resources.GenericSettingsProviderValidationErrors); }

        _serialization.SerializeToFile(Value, path);
    }

    /// <summary>
    /// When overriden in a derived class, returns the default settings values.
    /// </summary>
    protected virtual T GetDefault() => new T();
}

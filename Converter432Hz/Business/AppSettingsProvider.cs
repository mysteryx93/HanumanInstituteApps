using System.Text.Json.Serialization.Metadata;
using Avalonia.Controls;

namespace HanumanInstitute.Converter432Hz.Business;

/// <summary>
/// Contains custom application settings for 432Hz Converter.
/// </summary>
public sealed class AppSettingsProvider : SettingsProviderBase<AppSettingsData>
{
    private readonly IAppPathService _appPath;
    private readonly IFileSystemService _fileSystem;
    private readonly IEnvironmentService _environment;

    public AppSettingsProvider(ISerializationService serializationService, IAppPathService appPath, IFileSystemService fileSystem,
        IEnvironmentService environment, IJsonTypeInfoResolver? serializerContext) :
        base(serializationService, serializerContext)
    {
        _appPath = appPath;
        _fileSystem = fileSystem;
        _environment = environment;

        Load();
    }
    
    /// <inheritdoc />
    public override string FilePath => _appPath.ConfigFile;

    /// <summary>
    /// Loads settings file if present, or creates a new object with default values.
    /// </summary>
    public override AppSettingsData Load()
    {
        if (Design.IsDesignMode) { return GetDefault(); }

        return Load(_appPath.ConfigFile);
    }
    /// <summary>
    /// Saves settings into an XML file.
    /// </summary>
    public override void Save() => Save(_appPath.ConfigFile);

    protected override AppSettingsData GetDefault() =>
        new()
        {
            Width = 600,
            Height = 400,
            Encode = { PitchTo = 432 },
            MaxThreads = Math.Min(64, _environment.ProcessorCount)
        };
}

using System;
using System.Collections.Generic;
using HanumanInstitute.Common.Services;

namespace HanumanInstitute.PowerliminalsPlayer.Business;

/// <summary>
/// Manages the file system paths used by the application.
/// </summary>
public class AppPathService : IAppPathService
{
    private readonly IEnvironmentService _environment;
    private readonly IFileSystemService _fileSystem;

    public AppPathService(IEnvironmentService environmentService, IFileSystemService fileSystemService)
    {
        _environment = environmentService ?? throw new ArgumentNullException(nameof(environmentService));
        _fileSystem = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <summary>
    /// Returns all valid audio extensions
    /// </summary>
    public IList<string> AudioExtensions => _audioExtensions ??= new[] { ".mp3", ".mp2", ".aac", ".wav", ".wma", ".m4a", ".flac" };
    private IList<string>? _audioExtensions;

    /// <summary>
    /// Returns the path where the Powerliminals Player settings file is stored.
    /// </summary>
    public string SettingsPath => _fileSystem.Path.Combine(_environment.ApplicationDataPath, @"Natural Grounding Player\PowerliminalsConfig.xml");
    /// <summary>
    /// Returns the path where unhandled exceptions are logged.
    /// </summary>
    public string UnhandledExceptionLogPath => _fileSystem.Path.Combine(_environment.ApplicationDataPath, @"Natural Grounding Player\Log.txt");
}
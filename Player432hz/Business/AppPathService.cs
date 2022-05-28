﻿using System;
using System.Collections.Generic;
using System.Linq;
using HanumanInstitute.Common.Services;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;

namespace HanumanInstitute.Player432hz.Business;

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
    public IReadOnlyList<string> AudioExtensions => _audioExtensions ??= 
        _bassDevice.SupportedExtensions.SelectMany(x => x.Extensions).Distinct().OrderBy(x => x).ToList();
    private IReadOnlyList<string>? _audioExtensions;

    /// <summary>
    /// Returns the path where unhandled exceptions are logged.
    /// </summary>
    public string UnhandledExceptionLogPath => _fileSystem.Path.Combine(_environment.ApplicationDataPath, @"Natural Grounding Player\Log.txt");
    /// <summary>
    /// Returns the path where the 432hz Player settings file is stored.
    /// </summary>
    public string Player432hzConfigFile => _fileSystem.Path.Combine(_environment.ApplicationDataPath, @"Natural Grounding Player\432hzConfig.xml");
}

﻿using System.Linq;
using HanumanInstitute.MediaPlayer.Avalonia.Bass;

namespace HanumanInstitute.Player432Hz.Business;

/// <inheritdoc />
public class AppPathService : IAppPathService
{
    private readonly IEnvironmentService _environment;
    private readonly IFileSystemService _fileSystem;
    private readonly IBassDevice _bassDevice;

    public AppPathService(IEnvironmentService environmentService, IFileSystemService fileSystemService, IBassDevice bassDevice)
    {
        _environment = environmentService;
        _fileSystem = fileSystemService;
        _bassDevice = bassDevice;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> AudioExtensions => _audioExtensions ??= 
        _bassDevice.SupportedExtensions.SelectMany(x => x.Extensions).Distinct().OrderBy(x => x).ToList();
    private IReadOnlyList<string>? _audioExtensions;

    /// <inheritdoc />
    public string UnhandledExceptionLogPath => _unhandledExceptionLogPath ??=
        Combine(_environment.ApplicationDataPath, @"Hanuman Institute\Log.txt");
    private string? _unhandledExceptionLogPath;

    /// <inheritdoc />
    public string ConfigFile => _configFile ??= 
        Combine(_environment.ApplicationDataPath, @"Hanuman Institute\432HzPlayerConfig.json");
    private string? _configFile;
    
    /// <inheritdoc />
    public string MidiSoundsFile => _midiSoundsFile ??= 
        Combine(_environment.ApplicationDataPath, @"Hanuman Institute\midisounds.sf2");
    private string? _midiSoundsFile;
    
    /// <summary>
    /// Combines two paths while replacing folder separator chars with platform-specific char.
    /// </summary>
    private string Combine(string part1, string part2)
    {
        part1 = part1.Replace('\\', _fileSystem.Path.DirectorySeparatorChar);
        part2 = part2.Replace('\\', _fileSystem.Path.DirectorySeparatorChar);
        return _fileSystem.Path.Combine(part1, part2);
    }
}

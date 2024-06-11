// ReSharper disable InconsistentNaming

namespace HanumanInstitute.Converter432Hz.Business;

/// <summary>
/// Manages the file system paths used by the application.
/// </summary>
public interface IAppPathService : IAppPathServiceBase
{
    /// <summary>
    /// Returns all valid audio extensions
    /// </summary>
    IReadOnlyList<string> AudioExtensions { get; }
    /// <summary>
    /// Returns the Uri where the application settings file is stored.
    /// </summary>
    string ConfigFile { get; }
    /// <summary>
    /// Returns the path where midi sound font will be read from, if present.
    /// </summary>
    string MidiSoundsFile { get; }
}

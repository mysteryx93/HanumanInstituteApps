namespace HanumanInstitute.BassAudio;

/// <summary>
/// Represents an encoding format.
/// </summary>
public enum EncodeFormat
{
    /// <summary>
    /// MP3 audio format, most standard.
    /// </summary>
    Mp3,
    /// <summary>
    /// FLAC lossless audio format, best quality.
    /// </summary>
    Flac,
    /// <summary>
    /// WAV lossless audio format.
    /// </summary>
    Wav,
    /// <summary>
    /// OPUS audio format, best compression.
    /// </summary>
    Opus,
    /// <summary>
    /// OGG/Vorbis audio format, better quality than MP3.
    /// </summary>
    Ogg
}

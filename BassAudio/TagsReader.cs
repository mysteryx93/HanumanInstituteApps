using ManagedBass.Tags;
// ReSharper disable StringLiteralTypo

namespace HanumanInstitute.BassAudio;

/// <summary>
/// Reads media tags using BASS.
/// </summary>
public class TagsReader
{
    private readonly int _chan;
    
    /// <summary>
    /// Initializes a new instance of the TagsReader from specified BASS channel handle.
    /// </summary>
    /// <param name="bassChan">The BASS channel to read from.</param>
    public TagsReader(int bassChan)
    {
        _chan = bassChan;
    }

    private string? Tag(string name) => BassTags.Read(_chan, name);

    /// <summary>
    /// Returns the song title.
    /// </summary>
    public string? Title => Tag("%TITL");

    /// <summary>
    /// Returns the song artist.
    /// </summary>
    public string? Artist => Tag("%ARTI");
    
    /// <summary>
    /// Returns the song album name.
    /// </summary>
    public string? Album => Tag("%ALBM");

    /// <summary>
    /// Returns the song genre.
    /// </summary>
    public string? Genre => Tag("%GNRE");

    /// <summary>
    /// Returns the song/album year.
    /// </summary>
    public string? Year => Tag("%YEAR");

    /// <summary>
    /// Returns the song comment.
    /// </summary>
    public string? Comment => Tag("%CMNT");

    /// <summary>
    /// Returns the song track number (may include total track count "track/total".
    /// </summary>
    public string? Track => Tag("%TRCK");

    /// <summary>
    /// Returns the song composer.
    /// </summary>
    public string? Composer => Tag("%COMP");

    /// <summary>
    /// Returns the song copyright.
    /// </summary>
    public string? Copyright => Tag("%COPY");

    /// <summary>
    /// Returns the song subtitle.
    /// </summary>
    public string? Subtitle => Tag("%SUBT");

    /// <summary>
    /// Returns the song album artist.
    /// </summary>
    public string? AlbumArtist => Tag("%AART");
    /// <summary>
    /// Returns the song disc number (may include total disc count "disc/total").
    /// </summary>
    public string? Disc => Tag("%DISC");

    /// <summary>
    /// Returns the song publisher.
    /// </summary>
    public string? Publisher => Tag("%PUBL");
}

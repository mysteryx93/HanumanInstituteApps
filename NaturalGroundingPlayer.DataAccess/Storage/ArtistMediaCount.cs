
namespace EmergenceGuardian.NaturalGroundingPlayer.DataAccess {
    /// <summary>
    /// Contains how many songs per artist are in the database.
    /// </summary>
    public class ArtistMediaCount {
        /// <summary>
        /// The type of media file, the count is separate for each media type.
        /// </summary>
        public MediaType MediaType { get; set; }
        /// <summary>
        /// The name of an artist.
        /// </summary>
        public string Artist { get; set; }
        /// <summary>
        /// How many media files are in the database from that artist.
        /// </summary>
        public int Count;

        public ArtistMediaCount() {
        }

        public ArtistMediaCount(MediaType mediaType, string artist, int count) {
            this.MediaType = mediaType;
            this.Artist = artist;
            this.Count = count;
        }
    }
}

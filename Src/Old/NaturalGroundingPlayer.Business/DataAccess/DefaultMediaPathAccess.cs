using System;
using System.Collections.Generic;
using System.Linq;

namespace HanumanInstitute.NaturalGroundingPlayer.DataAccess {

    #region Interface

    /// <summary>
    /// Provides data access for DefaultMediaPath.
    /// </summary>
    public interface IDefaultMediaPathAccess {
        /// <summary>
        /// Returns how many songs per artist are in the database.
        /// </summary>
        List<ArtistMediaCount> GetMediaCountPerArtist();
        /// <summary>
        /// Returns a list of all categories.
        /// </summary>
        List<MediaCategory> GetCategories();
    }

    #endregion

    /// <summary>
    /// Provides data access for DefaultMediaPath.
    /// </summary>
    public class DefaultMediaPathAccess : IDefaultMediaPathAccess {

        #region Declarations / Constructors

        private INgpContextFactory contextFactory;

        public DefaultMediaPathAccess() : this(new NgpContextFactory()) { }

        public DefaultMediaPathAccess(INgpContextFactory contextFactory) {
            this.contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        #endregion

        /// <summary>
        /// Returns how many songs per artist are in the database.
        /// </summary>
        public List<ArtistMediaCount> GetMediaCountPerArtist() {
            using (Entities context = contextFactory.Create()) {
                var Result = (from m in context.Media
                              where m.Artist != ""
                              group m.MediaId by new { m.MediaTypeId, m.Artist } into a
                              select new ArtistMediaCount() { 
                                  MediaType = (MediaType)a.Key.MediaTypeId, 
                                  Artist = a.Key.Artist, 
                                  Count = a.Count()
                              }).ToList();
                return Result;
            }
        }

        /// <summary>
        /// Returns a list of all categories.
        /// </summary>
        public List<MediaCategory> GetCategories() {
            using (Entities context = contextFactory.Create()) {
                var Result = (from c in context.MediaCategories
                              select c).ToList();
                return Result;
            }
        }
    }
}

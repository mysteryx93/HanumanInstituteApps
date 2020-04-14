using System;
using System.Linq;

namespace EmergenceGuardian.NaturalGroundingPlayer.DataAccess {

    #region Interface

    /// <summary>
    /// Provides data access for DistributionGraph.
    /// </summary>
    public interface IDistributionGraphAccess {
        /// <summary>
        /// Returns how many media entries match specified conditions.
        /// </summary>
        /// <param name="mediaType">The type of media to look for.</param>
        /// <param name="ratingCategory">The media cateogory to look for.</param>
        /// <param name="min">The lowest rating value to look for.</param>
        /// <param name="max">The highest rating value to look for.</param>
        /// <param name="ratio">The height/depth ratio used to calculate rating.</param>
        /// <returns>The count of matching media.</returns>
        int GetMediaCount(MediaType mediaType, RatingCategory ratingCategory, float min, float max, double ratio);
    }

    #endregion

    /// <summary>
    /// Provides data access for DistributionGraph.
    /// </summary>
    public class DistributionGraphAccess : IDistributionGraphAccess {

        #region Declarations / Constructors

        private INgpContextFactory contextFactory;

        public DistributionGraphAccess() : this(new NgpContextFactory()) { }

        public DistributionGraphAccess(INgpContextFactory contextFactory) {
            this.contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        #endregion

        /// <summary>
        /// Returns how many media entries match specified conditions.
        /// </summary>
        /// <param name="mediaType">The type of media to look for.</param>
        /// <param name="ratingCategory">The media cateogory to look for.</param>
        /// <param name="min">The lowest rating value to look for.</param>
        /// <param name="max">The highest rating value to look for.</param>
        /// <param name="ratio">The height/depth ratio used to calculate rating.</param>
        /// <returns>The count of matching media.</returns>
        public int GetMediaCount(MediaType mediaType, RatingCategory ratingCategory, float min, float max, double ratio) {
            using (Entities context = contextFactory.Create(true)) {
                if (ratingCategory.Name != "Intensity")
                    return GetMediaQuery(context, mediaType, ratingCategory, min, max, ratio).Count();
                else
                    return GetIntensityQuery(context, mediaType, ratingCategory, min, max, ratio).Count();
            }
        }

        private IQueryable<Media> GetMediaQuery(Entities context, MediaType mediaType, RatingCategory ratingCategory, double min, double max, double ratio) {
            var Query = (from m in context.Media
                         let val = (from r in m.MediaRatings
                                    where r.RatingCategory.Name == ratingCategory.Name
                                    select r.DbGetValue(r.Height, r.Depth, ratio)).FirstOrDefault()
                         where (mediaType == MediaType.None || m.MediaTypeId == (int)mediaType) &&
                            val != null && val > min && val <= max
                         orderby m.Artist, m.Title
                         select m);
            return Query;
        }

        private IQueryable<Media> GetIntensityQuery(Entities context, MediaType mediaType, RatingCategory ratingCategory, double min, double max, double ratio) {
            var Query = (from m in context.Media
                             let val = (from r in m.MediaRatings
                                        let val = r.DbGetValue(r.Height, r.Depth, ratio)
                                        orderby val descending
                                        select val).Take(5).Average()
                         where (mediaType == MediaType.None || m.MediaTypeId == (int)mediaType) &&
                            val > min && val <= max
                         orderby m.Artist, m.Title
                         select m);
            return Query;
        }
    }
}

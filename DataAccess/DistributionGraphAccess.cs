using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public static class DistributionGraphAccess {
        public static IQueryable<Media> GetMediaQuery(Entities context, MediaType mediaType, RatingCategory ratingCategory, double min, double max, double ratio) {
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

        public static IQueryable<Media> GetIntensityQuery(Entities context, MediaType mediaType, RatingCategory ratingCategory, double min, double max, double ratio) {
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

        public static int GetMediaCount(MediaType mediaType, RatingCategory ratingCategory, float[] minMax, double ratio) {
            using (Entities context = new Entities()) {
                if (ratingCategory.Name != "Intensity")
                    return GetMediaQuery(context, mediaType, ratingCategory, minMax[0], minMax[1], ratio).Count();
                else
                    return GetIntensityQuery(context, mediaType, ratingCategory, minMax[0], minMax[1], ratio).Count();
            }
        }
    }
}

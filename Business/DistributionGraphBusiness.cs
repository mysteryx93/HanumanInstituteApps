using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace Business {
    /// <summary>
    /// Provides data for the Distribution Graph window.
    /// </summary>
    public class DistributionGraphBusiness {
        /// <summary>
        /// Contain the Min and Max value of each graph bar.
        /// </summary>
        public float[][] bars = new float[][] { 
                               new float[] { 4.0f, 4.5f },
                               new float[] { 4.5f, 5.0f },
                               new float[] { 5.0f, 5.5f },
                               new float[] { 5.5f, 6.0f },
                               new float[] { 6.0f, 6.5f },
                               new float[] { 6.5f, 7.0f },
                               new float[] { 7.0f, 7.5f },
                               new float[] { 7.5f, 8.0f },
                               new float[] { 8.0f, 8.5f },
                               new float[] { 8.5f, 9.0f },
                               new float[] { 9.0f, 9.5f },
                               new float[] { 9.5f, 10.0f },
                               new float[] { 10.0f, 10.5f },
                               new float[] { 10.5f, 11.0f }};

        public DistributionGraphBusiness() {
        }

        /// <summary>
        /// Loads the list of rating categories for the Distibution Graph interface.
        /// </summary>
        /// <returns>A list of RatingCategory objects.</returns>
        public async Task<List<RatingCategory>> GetRatingCategoriesAsync() {
            List<RatingCategory> Result = await Task.Run(() => SearchVideoAccess.GetCustomRatingCategories());
            Result.Insert(0, new RatingCategory() { Name = "--------------------" });
            Result.Insert(0, new RatingCategory() { Name = "Love" });
            Result.Insert(0, new RatingCategory() { Name = "Egoless" });
            Result.Insert(0, new RatingCategory() { Name = "Spiritual Feminine" });
            Result.Insert(0, new RatingCategory() { Name = "Spiritual Masculine" });
            Result.Insert(0, new RatingCategory() { Name = "Emotional Feminine" });
            Result.Insert(0, new RatingCategory() { Name = "Emotional Masculine" });
            Result.Insert(0, new RatingCategory() { Name = "Physical Feminine" });
            Result.Insert(0, new RatingCategory() { Name = "Physical Masculine" });
            Result.Insert(0, new RatingCategory() { Name = "Intensity" });
            Result.Insert(0, new RatingCategory());
            return Result;
        }

        /// <summary>
        /// Returns a list of video count for each graph bar.
        /// </summary>
        /// <param name="mediaType">The type of media to return.</param>
        /// <param name="graphType">The rating type to generate the graph for.</param>
        /// <param name="ratio">The ratio to use when multiplying Height and Depth.</param>
        /// <returns>A list of 14 integers.</returns>
        public async Task<List<int>> LoadGraphAsync(MediaType mediaType, RatingCategory graphType, double ratio) {
            return await Task.Run(() => LoadGraph(mediaType, graphType, ratio));
        }

        /// <summary>
        /// Returns a list of video count for each graph bar.
        /// </summary>
        /// <param name="mediaType">The type of media to return.</param>
        /// <param name="graphType">The rating type to generate the graph for.</param>
        /// <param name="ratio">The ratio to use when multiplying Height and Depth.</param>
        /// <returns>A list of 14 integers.</returns>
        public List<int> LoadGraph(MediaType mediaType, RatingCategory graphType, double ratio) {
            List<int> Result = new List<int>();
            Result.Add(DistributionGraphAccess.GetMediaCount(mediaType, graphType, bars[0], ratio));
            Result.Add(DistributionGraphAccess.GetMediaCount(mediaType, graphType, bars[1], ratio));
            Result.Add(DistributionGraphAccess.GetMediaCount(mediaType, graphType, bars[2], ratio));
            Result.Add(DistributionGraphAccess.GetMediaCount(mediaType, graphType, bars[3], ratio));
            Result.Add(DistributionGraphAccess.GetMediaCount(mediaType, graphType, bars[4], ratio));
            Result.Add(DistributionGraphAccess.GetMediaCount(mediaType, graphType, bars[5], ratio));
            Result.Add(DistributionGraphAccess.GetMediaCount(mediaType, graphType, bars[6], ratio));
            Result.Add(DistributionGraphAccess.GetMediaCount(mediaType, graphType, bars[7], ratio));
            Result.Add(DistributionGraphAccess.GetMediaCount(mediaType, graphType, bars[8], ratio));
            Result.Add(DistributionGraphAccess.GetMediaCount(mediaType, graphType, bars[9], ratio));
            Result.Add(DistributionGraphAccess.GetMediaCount(mediaType, graphType, bars[10], ratio));
            Result.Add(DistributionGraphAccess.GetMediaCount(mediaType, graphType, bars[11], ratio));
            Result.Add(DistributionGraphAccess.GetMediaCount(mediaType, graphType, bars[12], ratio));
            Result.Add(DistributionGraphAccess.GetMediaCount(mediaType, graphType, bars[13], ratio));
            return Result;
        }
    }
}

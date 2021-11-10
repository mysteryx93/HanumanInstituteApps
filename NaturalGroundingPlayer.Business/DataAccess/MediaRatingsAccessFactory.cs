using System;

namespace HanumanInstitute.NaturalGroundingPlayer.DataAccess {

    #region Interface

    /// <summary>
    /// Creates instances of EditRatingsAccess.
    /// </summary>
    public interface IMediaRatingsAccessFactory {
        /// <summary>
        /// Initializes EditRatingsAccess as a stand-alone editor.
        /// </summary>
        /// <param name="video">The video for which to edit ratings.</param>
        /// <param name="ratio">The Height:Depth ratio used to calculate ratings.</param>
        /// <returns>A new EditRatingAccess instance.</returns>
        IMediaRatingsAccess Create(Media video, double ratio);
        /// <summary>
        /// Initializes EditRatingAccess as part of an existing editing transaction.
        /// </summary>
        /// <param name="video">The video for which to edit ratings.</param>
        /// <param name="context">An existing connection to use for the edit.</param>
        /// <returns>A new EditRatingAccess instance.</returns>
        IMediaRatingsAccess Create(Media video, Entities context);
    }

    #endregion

    /// <summary>
    /// Creates instances of EditRatingsAccess.
    /// </summary>
    public class MediaRatingsAccessFactory : IMediaRatingsAccessFactory {

        #region Declarations / Constructors

        private INgpContextFactory contextFactory;

        public MediaRatingsAccessFactory() { }

        public MediaRatingsAccessFactory(INgpContextFactory contextFactory) {
            this.contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        #endregion

        /// <summary>
        /// Initializes EditRatingsAccess as a stand-alone editor.
        /// </summary>
        /// <param name="video">The video for which to edit ratings.</param>
        /// <param name="ratio">The Height:Depth ratio used to calculate ratings.</param>
        /// <returns>A new EditRatingAccess instance.</returns>
        public IMediaRatingsAccess Create(Media video, double ratio) => new MediaRatingsAccess(video, ratio, contextFactory);

        /// <summary>
        /// Initializes EditRatingAccess as part of an existing editing transaction.
        /// </summary>
        /// <param name="video">The video for which to edit ratings.</param>
        /// <param name="context">An existing connection to use for the edit.</param>
        /// <returns>A new EditRatingAccess instance.</returns>
        public IMediaRatingsAccess Create(Media video, Entities context) => new MediaRatingsAccess(video, 1, context);
    }
}

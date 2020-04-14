using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;

namespace EmergenceGuardian.NaturalGroundingPlayer.DataAccess {
    /// <summary>
    /// Extends the DbContext with extra features.
    /// </summary>
    public partial class Entities {
        /// <summary>
        /// Adds detailed validation errors to the exception when saving fails.
        /// </summary>
        public override int SaveChanges() {
            try {
                return base.SaveChanges();
            } catch (DbEntityValidationException ex) {
                // Retrieve the error messages as a list of strings.
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Combine the original exception message with the new one.
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                // Throw a new DbEntityValidationException with the improved exception message.
                throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }
        }

        [DbFunction("NaturalGroundingVideosModel.Store", "substr")]
        public string SubStr(string text, int startPos) {
            return text.Substring(startPos);
        }

        [DbFunction("NaturalGroundingVideosModel.Store", "DbCompareValues")]
        public bool CompareValues(double? value1, OperatorConditionEnum compareOp, double? value2) {
            return DbCompareValuesClass.Calculate(value1, compareOp, value2);
        }
    }

    /// <summary>
    /// Extends Media with extra features, such as allowing changes to be tracked for databinding.
    /// </summary>
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public partial class Media {
        public MediaType MediaType {
            get => (MediaType)MediaTypeId;
            set => MediaTypeId = (int)value;
        }

        public string Dimension => Height.HasValue ? string.Format("{0}p", Height) : "";

        public string DisplayTitle => Artist.Length == 0 ? Title : string.Format("{0} - {1}", Artist, Title);
    }

    /// <summary>
    /// Extends MediaRating with extra features, such as allowing changes to be tracked for databinding.
    /// </summary>
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public partial class MediaRating {
        /// <summary>
        /// Returns the computed value of Height * Depth on a scale of 11.
        /// </summary>
        /// <param name="ratio">Ratio between -1 and 1 representing the priority between Height and Depth.
        /// If Ratio is under 0, Height is prioritized. If Ratio is over 0, Depth is prioritized.</param>
        /// <returns>The rating value.</returns>
        public double? GetValue(double ratio) {
            return DbGetRatingValueClass.Calculate(Height, Depth, ratio);
        }

        /// <summary>
        /// Returns the computed value of Height * Depth on a scale of 11, calculated in the datagbase as part of LINQ queries.
        /// </summary>
        /// <param name="height">The Height rating value.</param>
        /// <param name="depth">The Depth rating value.</param>
        /// <param name="ratio">Ratio between -1 and 1 representing the priority between Height and Depth.
        /// If Ratio is under 0, Height is prioritized. If Ratio is over 0, Depth is prioritized.</param>
        /// <returns>The rating value.</returns>
        [DbFunction("NaturalGroundingVideosModel.Store", "DbGetRatingValue")]
        public double? DbGetValue(double? height, double? depth, double ratio) {
            return DbGetRatingValueClass.Calculate(height, depth, ratio);
        }
    }

    /// <summary>
    /// Extends RatingCategory with extra features, such as allowing changes to be tracked for databinding.
    /// </summary>
    [PropertyChanged.AddINotifyPropertyChangedInterface]
    public partial class RatingCategory { }
}

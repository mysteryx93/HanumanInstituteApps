using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess {
    public partial class Entities {
        public Entities() : base("name=Entities") {
            SQLiteConnection Conn = (SQLiteConnection)Database.Connection;
            Conn.Open();
            Conn.BindFunction(new DbGetRatingValueClass());
            Conn.BindFunction(new DbCompareValuesClass());

            // Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }

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

    [PropertyChanged.ImplementPropertyChanged]
    public partial class Media {
        public MediaType MediaType {
            get { return (MediaType)MediaTypeId; }
            set { MediaTypeId = (int)value; }
        }

        public string Dimension {
            get {
                if (Height.HasValue)
                    return string.Format("{0}p", Height);
                else
                    return "";
            }
        }

        public string DisplayTitle {
            get {
                if (Artist.Length == 0)
                    return Title;
                else
                    return string.Format("{0} - {1}", Artist, Title);

            }
        }
    }

    [PropertyChanged.ImplementPropertyChanged]
    public partial class MediaRating {
        /// <summary>
        /// Returns the computed value of Height * Depth on a scale of 11.
        /// </summary>
        /// <param name="ratio">Ratio between -1 and 1 representing the priority between Height and Depth.
        /// If Ratio is under 0, Height is prioritized. If Ratio is over 0, Depth is prioritized.</param>
        /// <returns></returns>
        public double? GetValue(double ratio) {
            return DbGetRatingValueClass.Calculate(Height, Depth, ratio);
        }

        [DbFunction("NaturalGroundingVideosModel.Store", "DbGetRatingValue")]
        public double? DbGetValue(double? height, double? depth, double ratio) {
            return DbGetRatingValueClass.Calculate(height, depth, ratio);
        }
    }

    [PropertyChanged.ImplementPropertyChanged]
    public partial class RatingCategory {
    }
}

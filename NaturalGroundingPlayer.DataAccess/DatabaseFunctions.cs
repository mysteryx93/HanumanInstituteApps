using System;
using System.Data.SQLite;

namespace EmergenceGuardian.NaturalGroundingPlayer.DataAccess {
    /// <summary>
    /// Calculates the rating value according to this formula: Heigth * Depth * .11 = Force, adding the Ratio to shift the weight.
    /// </summary>
    [SQLiteFunction(Name = "DbGetRatingValue", Arguments = 3, FuncType = FunctionType.Scalar)]
    public class DbGetRatingValueClass : SQLiteFunction {
        /// <summary>
        /// This function gets called by the database when converting into LINQ queries.
        /// </summary>
        public override object Invoke(object[] args) {
            double? height = args[0] != DBNull.Value ? (double?)Convert.ToDouble(args[0]) : null;
            double? depth = args[1] != DBNull.Value ? (double?)Convert.ToDouble(args[1]) : null;
            double ratio = args[2] != DBNull.Value ? Convert.ToDouble(args[2]) : 0;
            double? Result = Calculate(height, depth, ratio);
            if (Result.HasValue)
                return Result;
            else
                return DBNull.Value;
        }

        /// <summary>
        /// Calculates the function.
        /// </summary>
        /// <param name="height">The rating height.</param>
        /// <param name="depth">The rating depth.</param>
        /// <param name="ratio">The ratio between height and depth when calculating.</param>
        /// <returns>The calculated force.</returns>
        public static double? Calculate(double? height, double? depth, double ratio) {
            if (height != null || depth != null) {
                double HeightCalc = height ?? depth.Value;
                double DepthCalc = depth ?? height.Value;
                if (ratio < 0)
                    DepthCalc = DepthCalc + (HeightCalc - DepthCalc) * -ratio;
                else if (ratio > 0)
                    HeightCalc = HeightCalc + (DepthCalc - HeightCalc) * ratio;
                return (double)Math.Round(HeightCalc * DepthCalc * .11, 1);
            } else
                return null;
        }
    }

    /// <summary>
    /// Compares two values with specified operator.
    /// </summary>
    [SQLiteFunction(Name = "DbCompareValues", Arguments = 3, FuncType = FunctionType.Scalar)]
    public class DbCompareValuesClass : SQLiteFunction {
        /// <summary>
        /// This function gets called by the database when converting into LINQ queries.
        /// </summary>
        public override object Invoke(object[] args) {
            double? Value1 = args[0] != DBNull.Value ? (double?)Convert.ToDouble(args[0]) : null;
            OperatorConditionEnum CompareOp = (OperatorConditionEnum)(int)Convert.ToInt32(args[1]);
            double Value2 = args[2] != DBNull.Value ? Convert.ToDouble(args[2]) : 0;
            return Calculate(Value1, CompareOp, Value2);
        }

        /// <summary>
        /// Calculates the function.
        /// </summary>
        /// <param name="value1">The value to compare with.</param>
        /// <param name="compareOp">The comparison operator.</param>
        /// <param name="value2">The value to compare to.</param>
        /// <returns>Whether both values match the operator condition.</returns>
        public static bool Calculate(double? value1, OperatorConditionEnum compareOp, double? value2) {
            if (value1 != null && value2 != null) {
                if (compareOp == OperatorConditionEnum.Smaller)
                    return value1 < value2;
                else if (compareOp == OperatorConditionEnum.GreaterOrEqual)
                    return value1 >= value2;
                else // Equal
                    return value1 == value2;
            } else
                return false;
        }
    }
}

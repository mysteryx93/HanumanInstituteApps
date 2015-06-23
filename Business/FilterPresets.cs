using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess;

namespace Business {
    /// <summary>
    /// Returns filter presets.
    /// </summary>
    public static class FilterPresets {
        /// <summary>
        /// Preset: Fire >= 8.5, Intensity >= 9.5
        /// </summary>
        public static List<SearchRatingSetting> PresetFire(bool negate) {
            List<SearchRatingSetting> Result = new List<SearchRatingSetting>();
            if (negate) {
                Result.Add(new SearchRatingSetting() {
                    Category = "Fire",
                    Operator = OperatorConditionEnum.Smaller,
                    Value = 8.5,
                    Or = new SearchRatingSetting() {
                        Category = "Intensity",
                        Operator = OperatorConditionEnum.Smaller,
                        Value = 9.5
                    }
                });
            } else {
                Result.Add(new SearchRatingSetting() {
                    Category = "Fire",
                    Operator = OperatorConditionEnum.GreaterOrEqual,
                    Value = 8.5
                });
                Result.Add(new SearchRatingSetting() {
                    Category = "Intensity",
                    Operator = OperatorConditionEnum.GreaterOrEqual,
                    Value = 9.5
                });
            }

            return Result;
        }

        /// <summary>
        /// Preset: Water >= 6
        /// </summary>
        /// <returns></returns>
        public static List<SearchRatingSetting> PresetWater(bool negate) {
            List<SearchRatingSetting> Result = new List<SearchRatingSetting>();
            Result.Add(new SearchRatingSetting() {
                Category = "Water",
                Operator = negate ? OperatorConditionEnum.Smaller : OperatorConditionEnum.GreaterOrEqual,
                Value = 6
            });
            return Result;
        }

        /// <summary>
        /// Preset: Pause. Egoless >= 4, Fire < 7, !Egoless < 8.5
        /// </summary>
        public static List<SearchRatingSetting> PresetPause() {
            List<SearchRatingSetting> Result = new List<SearchRatingSetting>();
            Result.Add(new SearchRatingSetting() {
                Category = "Intensity",
                Operator = OperatorConditionEnum.Smaller,
                Value = 8f
            });
            Result.Add(new SearchRatingSetting() {
                Category = "Fire",
                Operator = OperatorConditionEnum.Smaller,
                Value = 7
            });
            Result.Add(new SearchRatingSetting() {
                Category = "Egoless",
                Operator = OperatorConditionEnum.GreaterOrEqual,
                Value = 2
            });
            Result.Add(new SearchRatingSetting() {
                Category = "!Egoless",
                Operator = OperatorConditionEnum.Smaller,
                Value = 9f
            });
            return Result;
        }
    }
}

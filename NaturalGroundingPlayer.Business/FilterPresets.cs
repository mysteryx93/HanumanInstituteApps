using System;
using System.Collections.Generic;
using EmergenceGuardian.NaturalGroundingPlayer.DataAccess;

namespace EmergenceGuardian.NaturalGroundingPlayer.Business {

    #region Interface

    /// <summary>
    /// Returns filter presets.
    /// </summary>
    public interface IFilterPresets {
        /// <summary>
        /// Preset: Fire >= 8.5, Intensity >= 9.5
        /// </summary>
        List<SearchRatingSetting> PresetFire(bool negate);
        /// <summary>
        /// Preset: Water >= 6
        /// </summary>
        List<SearchRatingSetting> PresetWater(bool negate);
        /// <summary>
        /// Preset: Pause. Egoless >= 4, Fire < 7, !Egoless < 8.5
        /// </summary>
        List<SearchRatingSetting> PresetPause();
    }

    #endregion

    /// <summary>
    /// Returns filter presets.
    /// </summary>
    public class FilterPresets : IFilterPresets {
        public FilterPresets() { }

        /// <summary>
        /// Preset: Fire >= 8.5, Intensity >= 9.5
        /// </summary>
        public List<SearchRatingSetting> PresetFire(bool negate) {
            if (negate) {
                return new List<SearchRatingSetting>() {
                    new SearchRatingSetting("Fire", OperatorConditionEnum.Smaller, 8.5, new SearchRatingSetting("Intensity", OperatorConditionEnum.Smaller, 9.5))
                };
            } else {
                return new List<SearchRatingSetting>() {
                    new SearchRatingSetting("Fire", OperatorConditionEnum.GreaterOrEqual, 8.5),
                    new SearchRatingSetting("Intensity", OperatorConditionEnum.GreaterOrEqual, 9.5)
                };
            }
        }

        /// <summary>
        /// Preset: Water >= 6
        /// </summary>
        public List<SearchRatingSetting> PresetWater(bool negate) {
            return new List<SearchRatingSetting>() {
                new SearchRatingSetting("Water", negate ? OperatorConditionEnum.Smaller : OperatorConditionEnum.GreaterOrEqual, 6)
            };
        }

        /// <summary>
        /// Preset: Pause. Egoless >= 4, Fire < 7, !Egoless < 8.5
        /// </summary>
        public List<SearchRatingSetting> PresetPause() {
            return new List<SearchRatingSetting> {
                new SearchRatingSetting("Intensity", OperatorConditionEnum.Smaller, 8f),
                new SearchRatingSetting("Fire", OperatorConditionEnum.Smaller, 7),
                new SearchRatingSetting("Egoless", OperatorConditionEnum.GreaterOrEqual, 2),
                new SearchRatingSetting("!Egoless", OperatorConditionEnum.Smaller, 9f)
            };
        }
    }
}

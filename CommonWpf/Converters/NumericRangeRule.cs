using System;
using System.Globalization;
using System.Windows.Controls;
using HanumanInstitute.CommonWpf.Properties;

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Allows Min/Max binding validation.
    /// </summary>
    public class NumericRangeRule : ValidationRule
    {
        public double? Min { get; set; }
        public double? Max { get; set; }
        public int? Mod { get; set; }
        public bool AllowNull { get; set; }

        public NumericRangeRule()
        {
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty((string)value))
            {
                if (AllowNull)
                {
                    return new ValidationResult(true, null);
                }
                else
                {
                    return new ValidationResult(false, Resources.NumericRangeRuleNull);
                }
            }

            if (!double.TryParse((String)value, NumberStyles.Number, CultureInfo.CurrentCulture, out double NumValue))
            {
                return new ValidationResult(false, Resources.NumericRangeRuleInvalid);
            }

            if ((Min.HasValue && NumValue < Min) || (Max.HasValue && NumValue > Max))
            {
                return new ValidationResult(false, string.Format(CultureInfo.CurrentCulture, Resources.NumericRangeRuleOutOfRange, Min, Max));
            }
            else if (Mod.HasValue && NumValue % Mod != 0)
            {
                return new ValidationResult(false, String.Format(CultureInfo.CurrentCulture, Resources.NumericRangeRuleNotMultiple, Mod));
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}

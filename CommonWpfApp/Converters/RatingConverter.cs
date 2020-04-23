using System;
using System.Globalization;
using System.Windows.Data;

namespace HanumanInstitute.CommonWpfApp
{
    /// <summary>
    /// Converts ratings such as '9.5'. Parameter is maximum allowed value.
    /// </summary>
    [ValueConversion(typeof(double?), typeof(string))]
    public class RatingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var maxValue = ParseMaxValue(parameter);

            var decValue = value as double?;
            if (decValue == null)
            {
                return string.Empty;
            }
            else if (maxValue < 0 && decValue > 9999)
            {
                // Display max 9999
                return "9999";
            }
            else
            {
                // Display the digit for up to 10 for Height and Depth, and up to 100 for Power.
                var format = decValue < (maxValue == 10 ? 10 : 100) ? "0.0" : "0";
                return decValue.Value.ToString(format, CultureInfo.CurrentCulture);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strValue = value as string;
            if (!string.IsNullOrEmpty(strValue))
            {
                var maxValue = ParseMaxValue(parameter);

                var parse = double.TryParse(strValue, NumberStyles.Number, CultureInfo.CurrentCulture, out var result);
                if (parse && result >= 0 && (!maxValue.HasValue || result <= maxValue))
                {
                    return (double)Math.Round(result, result < 100 ? 1 : 0); // Allow digit only for up to 100.
                }
            }
            return null;
        }

        private static double? ParseMaxValue(object parameter)
        {
            if (parameter != null)
            {
                if (double.TryParse(parameter.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var maxValueOut))
                {
                    return maxValueOut;
                }
            }
            return null;
        }
    }
}

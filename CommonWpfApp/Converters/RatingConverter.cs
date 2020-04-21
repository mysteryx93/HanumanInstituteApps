using System;
using System.Globalization;
using System.Windows.Data;

namespace HanumanInstitute.CommonWpfApp
{
    /// <summary>
    /// Converts ratings such as '9.5'. Parameter is maximum allowed value.
    /// </summary>
    [ValueConversion(typeof(double?), typeof(String))]
    public class RatingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double? MaxValue = null;
            if (parameter != null)
            {
                double.TryParse(parameter.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out double MaxValueOut);
                MaxValue = MaxValueOut;
            }

            double? DecValue = value as double?;
            if (DecValue == null)
            {
                return string.Empty;
            }
            else if (MaxValue < 0 && DecValue > 9999)
            {
                // Display max 9999
                return "9999";
            }
            else
            {
                // Display the digit for up to 10 for Height and Depth, and up to 100 for Power.
                var format = DecValue < (MaxValue == 10 ? 10 : 100) ? "0.0" : "0";
                return DecValue.Value.ToString(format, CultureInfo.CurrentCulture);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            if (!string.IsNullOrEmpty(strValue))
            {
                double? MaxValue = null;
                if (parameter != null)
                {
                    double.TryParse(parameter.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out double MaxValueOut);
                    MaxValue = MaxValueOut;
                }

                bool parse = double.TryParse(strValue, NumberStyles.Number, CultureInfo.CurrentCulture, out double Result);
                if (parse && Result >= 0 && (!MaxValue.HasValue || Result <= MaxValue))
                {
                    return (double)Math.Round(Result, Result < 100 ? 1 : 0); // Allow digit only for up to 100.
                }
            }
            return null;
        }
    }
}

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
            double MaxValueOut;
            if (parameter != null)
            {
                double.TryParse(parameter.ToString(), out MaxValueOut);
                MaxValue = MaxValueOut;
            }

            double? DecValue = value as double?;
            if (DecValue == null)
                return "";
            else if (MaxValue < 0 && DecValue > 9999)
                // Display max 9999
                return "9999";
            else
                // Display the digit for up to 10 for Height and Depth, and up to 100 for Power.
                return DecValue.Value.ToString(DecValue < (MaxValue == 10 ? 10 : 100) ? "0.0" : "0");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            if (string.IsNullOrEmpty(strValue))
                return null;
            else
            {
                double Result = 0;
                double? MaxValue = null;
                double MaxValueOut;
                if (parameter != null)
                {
                    double.TryParse(parameter.ToString(), out MaxValueOut);
                    MaxValue = MaxValueOut;
                }

                if (double.TryParse(strValue, out Result) && Result >= 0 && (!MaxValue.HasValue || Result <= MaxValue))
                    return (double)Math.Round(Result, Result < 100 ? 1 : 0); // Allow digit only for up to 100.
                else
                    return null;
            }
        }
    }
}

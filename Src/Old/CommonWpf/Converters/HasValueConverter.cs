using System;
using System.Globalization;
using System.Windows.Data;

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Converts a value into a boolean indicating whether value isn't null. Set parameter to 'null' to instead check if value is null.
    /// </summary>
    public class HasValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty((string)parameter))
            {
                return value != null;
            }
            else
            {
                return value == null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

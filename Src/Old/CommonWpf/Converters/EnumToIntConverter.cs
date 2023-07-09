using System;
using System.Globalization;
using System.Windows.Data;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Converts an enumeration into an integer.
    /// </summary>
    public class EnumToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            value.CheckNotNull(nameof(value));
            return (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            value.CheckNotNull(nameof(value));
            return Enum.ToObject(targetType, (int)value);
        }
    }
}

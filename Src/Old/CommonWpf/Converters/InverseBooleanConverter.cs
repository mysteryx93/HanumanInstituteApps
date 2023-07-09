using System;
using System.Globalization;
using System.Windows.Data;
using HanumanInstitute.CommonWpf.Properties;

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Inverts a boolean value. Parsing is not implemented.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException(Resources.InverseBooleanConverterTargetTypeNotBoolean);

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}

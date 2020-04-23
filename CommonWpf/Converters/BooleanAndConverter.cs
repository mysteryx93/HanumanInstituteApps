using System;
using System.Globalization;
using System.Windows.Data;
using HanumanInstitute.CommonWpf.Properties;

namespace HanumanInstitute.CommonWpf
{
    public class BooleanAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) { throw new ArgumentNullException(nameof(values)); }

            foreach (var value in values)
            {
                if ((value is bool) && (bool)value == false)
                {
                    return false;
                }
            }
            return true;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(Resources.BooleanAndConverterConvertBackNotSupported);
        }
    }
}

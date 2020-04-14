using System;
using System.Globalization;
using System.Windows.Data;

namespace HanumanInstitute.CommonWpf
{
    public class RadioButtonCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { throw new ArgumentNullException(nameof(value)); }
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { throw new ArgumentNullException(nameof(value)); }
            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}

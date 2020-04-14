using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Allows converting boolean values to any enumeration values.
    /// </summary>
    /// <typeparam name="T">The enumeration type to convert to.</typeparam>
    public class BooleanConverter<T> : IValueConverter
    {
        public BooleanConverter(T trueValue, T falseValue)
        {
            TrueValue = trueValue;
            FalseValue = falseValue;
        }

        public T TrueValue { get; set; }
        public T FalseValue { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { throw new ArgumentNullException(nameof(value)); }
            return value is bool && ((bool)value) ? TrueValue : FalseValue;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) { throw new ArgumentNullException(nameof(value)); }
            return value is T && EqualityComparer<T>.Default.Equals((T)value, TrueValue);
        }
    }
}

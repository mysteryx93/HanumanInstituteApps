using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace HanumanInstitute.Common.Avalonia;

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

    public virtual object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool and true ? TrueValue : FalseValue;

    public virtual object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is T v && EqualityComparer<T>.Default.Equals(v, TrueValue);
}

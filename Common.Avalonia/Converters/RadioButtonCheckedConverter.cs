using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using HanumanInstitute.Common.Services;

namespace HanumanInstitute.Common.Avalonia;

public class RadioButtonCheckedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        value.CheckNotNull(nameof(value));
        return value.Equals(parameter);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        value.CheckNotNull(nameof(value));
        return value.Equals(true) ? parameter : BindingOperations.DoNothing;
    }
}
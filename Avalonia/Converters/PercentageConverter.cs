using System.Globalization;
using Avalonia.Data.Converters;

namespace HanumanInstitute.Avalonia;

public class PercentageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return System.Convert.ToDouble(value, CultureInfo.InvariantCulture) * 
               System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

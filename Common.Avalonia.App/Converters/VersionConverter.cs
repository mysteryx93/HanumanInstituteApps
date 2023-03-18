using System.Globalization;
using Avalonia.Data.Converters;

namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Converts a Version to a string of format 'v1.0' or 'v1.0.1'.
/// </summary>
public class VersionConverter : IValueConverter
{
    public static VersionConverter Instance => new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) { return string.Empty; }
        
        var v = (Version)value;
        return $"v{v.Major}.{v.Minor}" + 
               (v.Build > 0 ? $".{v.Build}" : string.Empty);
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace HanumanInstitute.Common.Avalonia;

/// <summary>
/// Converts a file size to be displayed in B, KB, MB, etc. Parsing is not implemented.
/// </summary>
public class FileSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        double size = (long)value;
        var unit = 0;

        while (size >= 1024)
        {
            size /= 1024;
            ++unit;
        }

        return string.Format(CultureInfo.CurrentCulture, "{0:0.#} {1}", size, units[unit]);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Converts ratings into a color ranging from 0.0 = Hue 180 (Cyan) to 11.0 = Hue 0 (Red).
/// </summary>
public class RatingToColorConverter : IValueConverter
{
    public static RatingToColorConverter Instance { get; } = new(); 
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var result = Color.FromRgb(0, 0, 0);
        var decValue = value as double?;
        if (decValue != null)
        {
            decValue = Math.Min(decValue.Value, 10);
            var hue = (11 - decValue.Value) / 11 * .5f;
            result = HsbToRgb(hue, 1, .7f);
        }
        return targetType == typeof(Color) ? result : new SolidColorBrush(result);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static Color HsbToRgb(double hue, double saturation, double brightness)
    {
        int r = 0, g = 0, b = 0;
        if (saturation == 0)
        {
            r = g = b = (int)(brightness * 255.0f + 0.5f);
        }
        else
        {
            var h = (hue - Math.Floor(hue)) * 6.0f;
            var f = h - Math.Floor(h);
            var p = brightness * (1.0f - saturation);
            var q = brightness * (1.0f - saturation * f);
            var t = brightness * (1.0f - (saturation * (1.0f - f)));
            switch ((int)h)
            {
                case 0:
                    r = (int)(brightness * 255.0f + 0.5f);
                    g = (int)(t * 255.0f + 0.5f);
                    b = (int)(p * 255.0f + 0.5f);
                    break;
                case 1:
                    r = (int)(q * 255.0f + 0.5f);
                    g = (int)(brightness * 255.0f + 0.5f);
                    b = (int)(p * 255.0f + 0.5f);
                    break;
                case 2:
                    r = (int)(p * 255.0f + 0.5f);
                    g = (int)(brightness * 255.0f + 0.5f);
                    b = (int)(t * 255.0f + 0.5f);
                    break;
                case 3:
                    r = (int)(p * 255.0f + 0.5f);
                    g = (int)(q * 255.0f + 0.5f);
                    b = (int)(brightness * 255.0f + 0.5f);
                    break;
                case 4:
                    r = (int)(t * 255.0f + 0.5f);
                    g = (int)(p * 255.0f + 0.5f);
                    b = (int)(brightness * 255.0f + 0.5f);
                    break;
                case 5:
                    r = (int)(brightness * 255.0f + 0.5f);
                    g = (int)(p * 255.0f + 0.5f);
                    b = (int)(q * 255.0f + 0.5f);
                    break;
            }
        }
        return Color.FromArgb(System.Convert.ToByte(255), System.Convert.ToByte(r), System.Convert.ToByte(g), System.Convert.ToByte(b));
    }
}

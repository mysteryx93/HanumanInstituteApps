using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HanumanInstitute.CommonWpfApp
{
    /// <summary>
    /// Converts ratings into a color ranging from 0.0 = Hue 180 (Cyan) to 11.0 = Hue 0 (Red).
    /// </summary>
    public class RatingToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color Result = Color.FromRgb(0, 0, 0);
            double? DecValue = value as double?;
            if (DecValue != null)
            {
                DecValue = Math.Min(DecValue.Value, 10);
                double Hue = (11 - DecValue.Value) / 11 * .5f;
                Result = HSBtoRGB(Hue, 1, .7f);
            }
            return targetType?.GetType() == typeof(Color) ? (object)Result : new SolidColorBrush(Result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static Color HSBtoRGB(double hue, double saturation, double brightness)
        {
            int r = 0, g = 0, b = 0;
            if (saturation == 0)
            {
                r = g = b = (int)(brightness * 255.0f + 0.5f);
            }
            else
            {
                double h = (hue - (double)Math.Floor(hue)) * 6.0f;
                double f = h - (double)Math.Floor(h);
                double p = brightness * (1.0f - saturation);
                double q = brightness * (1.0f - saturation * f);
                double t = brightness * (1.0f - (saturation * (1.0f - f)));
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
}

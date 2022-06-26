using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// Converts media lenght format such as 1:25
/// </summary>
public class MediaTimeConverter : IValueConverter
{
    public static MediaTimeConverter Instance { get; } = new();
    
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value != null)
        {
            TimeSpan timeValue;
            if (value.GetType() != typeof(TimeSpan))
            {
                timeValue = new TimeSpan(0, 0, 0, System.Convert.ToInt32(value, CultureInfo.InvariantCulture));
            }
            else
            {
                timeValue = (TimeSpan)value;
            }

            var format = timeValue.TotalHours < 1 ? "m\\:ss" : "h\\:mm\\:ss";
            return timeValue.ToString(format, CultureInfo.InvariantCulture);
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var strValue = value as string;
        if (!string.IsNullOrEmpty(strValue))
        {
            try
            {
                var sepCount = strValue.Count(x => x == ':');
                var format = sepCount > 1 ? "h\\:mm\\:ss" : "m\\:ss";
                var resultDate = DateTime.ParseExact(strValue, format, CultureInfo.InvariantCulture);
                return (short)resultDate.TimeOfDay.TotalSeconds;
            }
            catch (FormatException) { }
        }
        return null;
    }
}

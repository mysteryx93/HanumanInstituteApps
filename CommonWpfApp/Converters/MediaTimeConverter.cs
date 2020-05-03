using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace HanumanInstitute.CommonWpfApp
{
    /// <summary>
    /// Converts media lenght format such as 1:25
    /// </summary>
    [ValueConversion(typeof(short?), typeof(string))]
    public class MediaTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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

                if (timeValue.TotalHours < 1)
                {
                    return timeValue.ToString("m\\:ss", CultureInfo.InvariantCulture);
                }
                else
                {
                    return timeValue.ToString("h\\:mm\\:ss", CultureInfo.InvariantCulture);
                }
            }
            return string.Empty;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var strValue = value as string;
            if (!string.IsNullOrEmpty(strValue))
            {
                try
                {
                    var sepCount = strValue.Count(x => x == ':');
                    DateTime resultDate;
                    if (sepCount > 1)
                    {
                        resultDate = DateTime.ParseExact(strValue, "h\\:mm\\:ss", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        resultDate = DateTime.ParseExact(strValue, "m\\:ss", CultureInfo.InvariantCulture);
                    }
                    return (short)resultDate.TimeOfDay.TotalSeconds;
                }
                catch (FormatException) { }
            }
            return null;
        }
    }
}

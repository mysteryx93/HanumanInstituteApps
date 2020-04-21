using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace HanumanInstitute.CommonWpfApp
{
    /// <summary>
    /// Converts media lenght format such as 1:25
    /// </summary>
    [ValueConversion(typeof(short?), typeof(String))]
    public class MediaTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                TimeSpan TimeValue;
                if (value.GetType() != typeof(TimeSpan))
                {
                    TimeValue = new TimeSpan(0, 0, 0, System.Convert.ToInt32(value, CultureInfo.InvariantCulture));
                }
                else
                {
                    TimeValue = (TimeSpan)value;
                }

                if (TimeValue.TotalHours < 1)
                {
                    return TimeValue.ToString("m\\:ss", CultureInfo.InvariantCulture);
                }
                else
                {
                    return TimeValue.ToString("h\\:mm\\:ss", CultureInfo.InvariantCulture);
                }
            }
            return String.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strValue = value as string;
            if (!string.IsNullOrEmpty(strValue))
            {
                try
                {
                    int SepCount = strValue.Count(x => x == ':');
                    DateTime ResultDate;
                    if (SepCount > 1)
                    {
                        ResultDate = DateTime.ParseExact(strValue, "h\\:mm\\:ss", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        ResultDate = DateTime.ParseExact(strValue, "m\\:ss", CultureInfo.InvariantCulture);
                    }
                    return (short)ResultDate.TimeOfDay.TotalSeconds;
                }
                catch (FormatException) { }
            }
            return null;
        }
    }
}

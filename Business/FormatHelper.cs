using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Business {
    public static class FormatHelper {
        public static Nullable<T> Parse<T>(string input) where T : struct {
            try {
                var Result = TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(input);
                return (T)Result;
            } catch {
                return null;
            }
        }

        public static string FormatTimeSpan(decimal? value) {
            if (value != null)
                return new TimeSpan(0, 0, 0, 0, (int)(value * 1000)).ToString("g");
            else
                return "";
        }

        public static decimal? ParseTimeSpan(string value) {
            if (string.IsNullOrEmpty(value))
                return null;
            else {
                try {
                    // DateTime.ParseExact(s, "HH.mm", CultureInfo.InvariantCulture).TimeOfDay
                    return Math.Round((decimal)TimeSpan.Parse(value).TotalSeconds, 3);
                } catch {
                    return null;
                }
            }
        }
    }
}

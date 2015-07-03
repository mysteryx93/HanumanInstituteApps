using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

    public static class ExtensionMethods {
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject {
            if (depObj != null) {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T) {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child)) {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, CancellationToken cancel, Func<T, Task> body) {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(dop)
                select Task.Run(async delegate {
                    using (partition)
                        while (partition.MoveNext() && (cancel == null || !cancel.IsCancellationRequested))
                            await body(partition.Current).ContinueWith(t => {
                                //observe exceptions
                            });
                }));
        }
    }
}

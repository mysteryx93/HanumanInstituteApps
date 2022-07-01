using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

// ReSharper disable CheckNamespace
namespace HanumanInstitute.Common.Services;

public static class ExtensionMethods
{
    // https://medium.com/@alex.puiu/parallel-foreach-async-in-c-36756f8ebe62
    /// <summary>
    /// Loops through a list asynchronously in multiple threads.
    /// </summary>
    /// <typeparam name="T">The data type of the list to loop through.</typeparam>
    /// <param name="source">The list to loop through.</param>
    /// <param name="body">The work to execute for each item in the list.</param>
    /// <param name="maxDegreeOfParallelism">The maximum amount of concurrent threads to use for looping.</param>
    /// <param name="scheduler">The task schedule to use for creating threads.</param>
    /// <returns></returns>
    public static Task AsyncParallelForEach<T>(this IEnumerable<T> source, Func<T, Task> body, int maxDegreeOfParallelism = DataflowBlockOptions.Unbounded, TaskScheduler? scheduler = null)
    {
        source.CheckNotNull(nameof(source));

        var options = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };
        if (scheduler != null)
        {
            options.TaskScheduler = scheduler;
        }

        var block = new ActionBlock<T>(body, options);

        foreach (var item in source)
        {
            block.Post(item);
        }

        block.Complete();
        return block.Completion;
    }

    // /// <summary>
    // /// Loops through a list asynchronously in multiple threads.
    // /// </summary>
    // /// <typeparam name="T">The data type of the list to loop through.</typeparam>
    // /// <param name="source">The list to loop through.</param>
    // /// <param name="dop">The amount of concurrent threads to use for looping.</param>
    // /// <param name="cancel">A cancellation token to manage the cancellation of the task.</param>
    // /// <param name="body">The work to execute for each item in the list.</param>
    // /// <returns>The asynchronous task that can be awaited.</returns>
    //public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, CancellationToken cancel, Func<T, Task> body)
    //{
    //    return Task.WhenAll(
    //        from partition in Partitioner.Create(source).GetPartitions(dop)
    //        select Task.Run(async delegate
    //        {
    //            using (partition)
    //                while (partition.MoveNext() && (cancel == null || !cancel.IsCancellationRequested))
    //                    await body(partition.Current).ContinueWith(t =>
    //                    {
    //                        //observe exceptions
    //                    });
    //        }));
    //}

    /// <summary>
    /// Copies all fields from one instance of a class to another.
    /// </summary>
    /// <typeparam name="T">The type of class to copy.</typeparam>
    /// <param name="source">The class to copy.</param>
    /// <param name="target">The class to copy to.</param>
    public static void CopyAll<T>(T source, T target)
    {
        var type = typeof(T);
        foreach (var sourceProperty in type.GetProperties())
        {
            var targetProperty = type.GetProperty(sourceProperty.Name);
            if (targetProperty?.SetMethod != null)
            {
                targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
            }
        }
        foreach (var sourceField in type.GetFields())
        {
            var targetField = type.GetField(sourceField.Name);
            targetField?.SetValue(target, sourceField.GetValue(source));
        }
    }
    //
    // /// <summary>
    // /// Adds the elements of the specified collection to the end of the IList.
    // /// </summary>
    // /// <typeparam name="T">The type of list items.</typeparam>
    // /// <param name="list">The list to add elements to.</param>
    // /// <param name="items">The collection whose elements should be added to the end of the IList.</param>
    // public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
    // {
    //     list.CheckNotNull(nameof(list));
    //     items.CheckNotNull(nameof(items));
    //
    //     if (list is List<T> castedList)
    //     {
    //         castedList.AddRange(items);
    //     }
    //     else
    //     {
    //         foreach (var item in items)
    //         {
    //             list.Add(item);
    //         }
    //     }
    // }

    /// <summary>
    /// Parses a string value into specified data type.
    /// </summary>
    /// <typeparam name="T">The data type to parse into.</typeparam>
    /// <param name="input">The string value to parse.</param>
    /// <returns>The parsed value, or null if parsing failed.</returns>
    public static T? Parse<T>(this string input) 
        where T : struct
    {
        try
        {
            var result = TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(input);
            return (T)result;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }

    /// <summary>
    /// Formats a decimal value into a TimeSpan in 'g' format.
    /// </summary>
    /// <param name="value">The value to format, in seconds.</param>
    /// <returns>A string representation of the value.</returns>
    public static string FormatTimeSpan(this decimal? value)
    {
        return value.HasValue ?
            new TimeSpan(0, 0, 0, 0, (int)(value * 1000)).ToString("g", CultureInfo.InvariantCulture) :
            string.Empty;
    }

    /// <summary>
    /// Parses a TimeSpan string value into a decimal representing the amount of seconds.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <returns>The amount of seconds in the string value.</returns>
    public static decimal? ParseTimeSpan(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        else
        {
            try
            {
                // DateTime.ParseExact(s, "HH.mm", CultureInfo.InvariantCulture).TimeOfDay
                return Math.Round((decimal)TimeSpan.Parse(value, CultureInfo.InvariantCulture).TotalSeconds, 3);
            }
            catch (FormatException) { }
            catch (OverflowException) { }
            return null;
        }
    }

    /// <summary>
    /// Forces a value to be within specified range.
    /// </summary>
    /// <typeparam name="T">The type of value to clamp.</typeparam>
    /// <param name="value">The value to clamp.</param>
    /// <param name="min">The lowest value that can be returned.</param>
    /// <param name="max">The highest value that can be returned.</param>
    /// <returns>The clamped value.</returns>
    public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
        {
            return min;
        }
        else if (value.CompareTo(max) > 0)
        {
            return max;
        }
        return value;
    }
}

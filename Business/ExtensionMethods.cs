using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Business {
    public static class ExtensionMethods {
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

        /// <summary>
        /// Copies all fields from one instance of a class to another.
        /// </summary>
        /// <typeparam name="T">The type of class to copy.</typeparam>
        /// <param name="source">The class to copy.</param>
        /// <param name="target">The class to copy to.</param>
        public static void CopyAll<T>(T source, T target) {
            var type = typeof(T);
            foreach (var sourceProperty in type.GetProperties()) {
                var targetProperty = type.GetProperty(sourceProperty.Name);
                if (targetProperty.SetMethod != null)
                    targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
            }
            foreach (var sourceField in type.GetFields()) {
                var targetField = type.GetField(sourceField.Name);
                targetField.SetValue(target, sourceField.GetValue(source));
            }
        }
    }
}

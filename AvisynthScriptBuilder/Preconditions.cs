using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace HanumanInstitute.AvisynthScriptBuilder
{
    /// <summary>
    /// Provides helper methods to validate parameters.
    /// </summary>
    internal static class Preconditions
    {
        /// <summary>
        /// Validates whether specific value is not null, and throws an exception if it is null.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the parameter.</param>
        public static T CheckNotNull<T>(this T? value, string name) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
            return value;
        }

        /// <summary>
        /// Validates whether specific value is not null or empty, and throws an exception if it is null or empty.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the parameter.</param>
        public static string CheckNotNullOrEmpty(this string? value, string name)
        {
            CheckNotNull(value, name);
            if (string.IsNullOrEmpty(value))
            {
                ThrowArgumentNullOrEmpty(name);
            }
            return value!;
        }

        /// <summary>
        /// Validates whether specific listt is not null or empty, and throws an exception if it is null or empty.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="name">The name of the parameter.</param>
        public static IEnumerable<T> CheckNotNullOrEmpty<T>(this IEnumerable<T>? value, string name)
        {
            value.CheckNotNull(name);
            if (!value.Any())
            {
                ThrowArgumentNullOrEmpty(name);
            }
            return value!;
        }

        /// <summary>
        /// Throws an exception of type ArgumentException saying an argument is null or empty.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        public static void ThrowArgumentNullOrEmpty(this string name)
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Properties.Resources.ArgumentNullOrEmpty, name), name);
        }
    }
}

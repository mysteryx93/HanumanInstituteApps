using System;

namespace HanumanInstitute.CommonUI
{
    public static class ExtensionMethods
    {
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
}

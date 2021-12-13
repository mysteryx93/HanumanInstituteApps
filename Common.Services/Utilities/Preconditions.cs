using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using HanumanInstitute.Common.Services.Properties;
using JetBrains.Annotations;

namespace HanumanInstitute.Common.Services;

/// <summary>
/// Provides helper methods to validate parameters.
/// </summary>
public static class Preconditions
{
    /// <summary>
    /// Validates whether specific value is not null, and throws an exception if it is null.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="name">The name of the parameter.</param>
    public static T CheckNotNull<T>([NoEnumeration] this T value, string name)
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
    public static string CheckNotNullOrEmpty(this string value, string name)
    {
        value.CheckNotNull(name);
        if (string.IsNullOrEmpty(value))
        {
            ThrowArgumentNullOrEmpty(name);
        }
        return value;
    }

    /// <summary>
    /// Validates whether specific list is not null or empty, and throws an exception if it is null or empty.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="name">The name of the parameter.</param>
    public static IEnumerable<T> CheckNotNullOrEmpty<T>(this IEnumerable<T> value, string name)
    {
        value.CheckNotNull(name);
        if (!value.Any())
        {
            ThrowArgumentNullOrEmpty(name);
        }
        return value;
    }

    /// <summary>
    /// Validates whether specified type is assignable from specific base class.
    /// </summary>
    /// <param name="value">The Type to validate.</param>
    /// <param name="baseType">The base type that value type must derive from.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <returns></returns>
    public static Type CheckAssignableFrom(this Type value, Type baseType, string name)
    {
        value.CheckNotNull(name);
        baseType.CheckNotNull(nameof(baseType));

        if (!value.IsAssignableFrom(baseType))
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.TypeMustBeAssignableFromBase, name, value.Name, baseType.Name));
        }
        return value;
    }

    /// <summary>
    /// Validates whether specified type derives from specific base class.
    /// </summary>
    /// <param name="value">The Type to validate.</param>
    /// <param name="baseType">The base type that value type must derive from.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <returns></returns>
    public static Type CheckDerivesFrom(this Type value, Type baseType, string name)
    {
        value.CheckNotNull(name);
        baseType.CheckNotNull(nameof(baseType));

        if (!value.IsSubclassOf(baseType))
        {
            throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.TypeMustDeriveFromBase, name, value.Name, baseType.Name));
        }
        return value;
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
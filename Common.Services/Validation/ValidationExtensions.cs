using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HanumanInstitute.Common.Services.Validation;

/// <summary>
/// Provides methods to validate objects based on DataAnnotations.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates an object based on its DataAnnotations and throws an exception if the object is not valid.
    /// </summary>
    /// <param name="v">The object to validate.</param>
    public static T ValidateAndThrow<T>(this T v)
    {
        Validator.ValidateObject(v, new ValidationContext(v), true);
        return v;
    }

    /// <summary>
    /// Validates an object based on its DataAnnotations and returns a list of validation errors.
    /// </summary>
    /// <param name="v">The object to validate.</param>
    /// <returns>A list of validation errors.</returns>
    public static ICollection<ValidationResult>? Validate<T>(this T v)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(v);
        if (!Validator.TryValidateObject(v, context, results, true))
        {
            return results;
        }
        return null;
    }
}
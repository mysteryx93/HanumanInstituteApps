﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using HanumanInstitute.Services.Properties;

// ReSharper disable CheckNamespace
namespace HanumanInstitute.Services;

/// <summary>
/// Although Razor Pages validate properties marked with BindAttribute, manual validation doesn't. 
/// Setting this attribute allows manual validation to also include them. 
/// This is often used for unit testing to test the PageModel validation.
/// </summary>
public class ValidateObjectAttribute : ValidationAttribute
{
    /// <summary>
    /// Performs validation on the decorated class member and returns the validation result.
    /// </summary>
    /// <returns></returns>
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        value.CheckNotNull(nameof(value));
        validationContext.CheckNotNull(nameof(validationContext));

        var results = new List<ValidationResult>();
        var context = new ValidationContext(value, null, null);

        Validator.TryValidateObject(value, context, results, true);

        if (results.Count != 0)
        {
            var compositeResults = new CompositeValidationResult(string.Format(CultureInfo.InvariantCulture, Resources.ValidationFailed, validationContext.DisplayName));
            results.ForEach(compositeResults.AddResult);

            return compositeResults;
        }
        return ValidationResult.Success!;
    }
}

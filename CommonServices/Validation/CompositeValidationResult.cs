using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HanumanInstitute.CommonServices
{
    /// <summary>
    /// Used by ValidateObjectAttribute to hold combined validation results of multiple objects.
    /// </summary>
    public class CompositeValidationResult : ValidationResult
    {
        private readonly List<ValidationResult> _results = new List<ValidationResult>();

        /// <summary>
        /// Returns the combined validation results.
        /// </summary>
        public IEnumerable<ValidationResult> Results => _results;

        public CompositeValidationResult(string errorMessage) : base(errorMessage) { }

        public CompositeValidationResult(string errorMessage, IEnumerable<string> memberNames) : base(errorMessage, memberNames) { }
        
        protected CompositeValidationResult(ValidationResult validationResult) : base(validationResult) { }

        /// <summary>
        /// Adds validation results to the merged list.
        /// </summary>
        /// <param name="validationResult">The validation results to add.</param>
        public void AddResult(ValidationResult validationResult)
        {
            _results.Add(validationResult);
        }
    }
}

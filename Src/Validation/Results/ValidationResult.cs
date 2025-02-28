#region License
// Copyright (c) .NET Foundation and contributors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// The latest version of this file can be found at https://github.com/FluentValidation/FluentValidation
#endregion

namespace FastEndpoints.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The result of running a validator
    /// </summary>
    [Serializable]
    public class ValidationResult
    {
        private readonly List<ValidationFailure> _errors;

        /// <summary>
        /// Whether validation succeeded
        /// </summary>
        public virtual bool IsValid => Errors.Count == 0;

        /// <summary>
        /// A collection of errors
        /// </summary>
        public List<ValidationFailure> Errors => _errors;

        public string[] RuleSetsExecuted { get; internal set; }

        /// <summary>
        /// Creates a new validationResult
        /// </summary>
        public ValidationResult()
        {
            _errors = new List<ValidationFailure>();
        }

        /// <summary>
        /// Creates a new ValidationResult from a collection of failures
        /// </summary>
        /// <param name="failures">List of <see cref="ValidationFailure"/> which is later available through <see cref="Errors"/>. This list get's copied.</param>
        /// <remarks>
        /// Every caller is responsible for not adding <c>null</c> to the list.
        /// </remarks>
        public ValidationResult(IEnumerable<ValidationFailure> failures)
        {
            _errors = failures.Where(failure => failure != null).ToList();
        }

        internal ValidationResult(List<ValidationFailure> errors)
        {
            _errors = errors;
        }

        /// <summary>
        /// Generates a string representation of the error messages separated by new lines.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(Environment.NewLine);
        }

        /// <summary>
        /// Generates a string representation of the error messages separated by the specified character.
        /// </summary>
        /// <param name="separator">The character to separate the error messages.</param>
        /// <returns></returns>
        public string ToString(string separator)
        {
            return string.Join(separator, _errors.Select(failure => failure.ErrorMessage));
        }
    }
}

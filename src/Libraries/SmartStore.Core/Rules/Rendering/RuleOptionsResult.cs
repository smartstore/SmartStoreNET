using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Rules
{
    public enum RuleOptionsRequestReason
    {
        /// <summary>
        /// Get options for select list.
        /// </summary>
        SelectListOptions = 0,

        /// <summary>
        /// Get display names of selected options.
        /// </summary>
        SelectedDisplayNames
    }


    public class RuleOptionsResult
    {
        public RuleOptionsResult() : this(null)
        {
        }

        public RuleOptionsResult(params RuleValueSelectListOption[] options)
        {
            Options = new List<RuleValueSelectListOption>(options ?? Enumerable.Empty<RuleValueSelectListOption>());
        }

        /// <summary>
        /// Select list options or display names of selected values, depending on <see cref="RuleOptionsRequestReason"/>.
        /// </summary>
        public IList<RuleValueSelectListOption> Options { get; protected set; }

        /// <summary>
        /// Indicates whether the provided data is paged.
        /// </summary>
        public bool IsPaged { get; set; }

        /// <summary>
        /// Indicates whether further data is available.
        /// </summary>
        public bool HasMoreData { get; set; }
    }
}

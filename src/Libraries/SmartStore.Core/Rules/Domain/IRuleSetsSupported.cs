using System.Collections.Generic;

namespace SmartStore.Rules.Domain
{
    /// <summary>
    /// Represents an entity which supports rule sets.
    /// </summary>
    public partial interface IRuleSetsSupported
    {
        /// <summary>
        /// Gets assigned rule sets.
        /// </summary>
        ICollection<RuleSetEntity> RuleSets { get; }
    }
}

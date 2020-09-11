using System.Collections.Generic;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;
using SmartStore.Rules.Domain;

namespace SmartStore.Core.Domain.Payments
{
    /// <summary>
    /// Represents a payment method
    /// </summary>
    [DataContract]
    public partial class PaymentMethod : BaseEntity, ILocalizedEntity, IStoreMappingSupported, IRulesContainer
    {
        private ICollection<RuleSetEntity> _ruleSets;

        /// <summary>
        /// Gets or sets the payment method system name
        /// </summary>
        [DataMember]
        public string PaymentMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets the full description
        /// </summary>
        [DataMember]
        public string FullDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to round the order total. Also known as "Cash rounding".
        /// </summary>
        /// <see cref="https://en.wikipedia.org/wiki/Cash_rounding"/>
        [DataMember]
        public bool RoundOrderTotalEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        [DataMember]
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets assigned rule sets.
        /// </summary>
        public virtual ICollection<RuleSetEntity> RuleSets
        {
            get => _ruleSets ?? (_ruleSets = new HashSet<RuleSetEntity>());
            protected set => _ruleSets = value;
        }
    }
}

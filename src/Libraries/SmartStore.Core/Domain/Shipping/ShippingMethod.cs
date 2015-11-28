using System.Collections.Generic;
using System.Runtime.Serialization;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Shipping
{
    /// <summary>
    /// Represents a shipping method (used for offline shipping rate computation methods)
    /// </summary>
    [DataContract]
	public partial class ShippingMethod : BaseEntity, ILocalizedEntity
    {
        private ICollection<Country> _restrictedCountries;

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [DataMember]
		public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
		[DataMember]
		public string Description { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
		[DataMember]
		public int DisplayOrder { get; set; }

		[DataMember]
		public bool IgnoreCharges { get; set; }

		/// <summary>
		/// Gets or sets identifiers of customer roles (comma separated) to be excluded in checkout
		/// </summary>
		[DataMember]
		public string ExcludedCustomerRoleIds { get; set; }

		/// <summary>
		/// Gets or sets the context identifier for country exclusion
		/// </summary>
		[DataMember]
		public int CountryExclusionContextId { get; set; }

		/// <summary>
		/// Gets or sets the country exclusion context
		/// </summary>
		[DataMember]
		public CountryRestrictionContextType CountryExclusionContext
		{
			get
			{
				return (CountryRestrictionContextType)this.CountryExclusionContextId;
			}
			set
			{
				this.CountryExclusionContextId = (int)value;
			}
		}

        /// <summary>
        /// Gets or sets the restricted countries
        /// </summary>
		[DataMember]
		public virtual ICollection<Country> RestrictedCountries
        {
			get { return _restrictedCountries ?? (_restrictedCountries = new HashSet<Country>()); }
            protected set { _restrictedCountries = value; }
        }
    }
}
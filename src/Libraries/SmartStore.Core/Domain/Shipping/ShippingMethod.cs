using System.Runtime.Serialization;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Core.Domain.Shipping
{
	/// <summary>
	/// Represents a shipping method (used for offline shipping rate computation methods)
	/// </summary>
	[DataContract]
	public partial class ShippingMethod : BaseEntity, ILocalizedEntity, IStoreMappingSupported
	{
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

		/// <summary>
		/// Gets or sets whether to ignore charges
		/// </summary>
		[DataMember]
		public bool IgnoreCharges { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
		/// </summary>
		[DataMember]
		public bool LimitedToStores { get; set; }
    }
}
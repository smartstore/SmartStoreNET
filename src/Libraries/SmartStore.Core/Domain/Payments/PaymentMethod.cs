using System.Runtime.Serialization;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Core.Domain.Payments
{
	/// <summary>
	/// Represents a payment method
	/// </summary>
	[DataContract]
	public partial class PaymentMethod : BaseEntity, ILocalizedEntity
	{
		/// <summary>
		/// Gets or sets the payment method system name
		/// </summary>
		[DataMember]
		public string PaymentMethodSystemName { get; set; }

		/// <summary>
		/// Gets or sets identifiers of customer roles (comma separated) to be excluded in checkout
		/// </summary>
		[DataMember]
		public string ExcludedCustomerRoleIds { get; set; }

		/// <summary>
		/// Gets or sets identifiers of countries (comma separated) to be excluded in checkout
		/// </summary>
		[DataMember]
		public string ExcludedCountryIds { get; set; }

		/// <summary>
		/// Gets or sets shipping methods (comma separated) to be excluded in checkout
		/// </summary>
		[DataMember]
		public string ExcludedShippingMethodIds { get; set; }

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
		/// Gets or sets the minimum order amount for which to offer the payment method
		/// </summary>
		[DataMember]
		public decimal? MinimumOrderAmount { get; set; }

		/// <summary>
		/// Gets or sets the maximum order amount for which to offer the payment method
		/// </summary>
		[DataMember]
		public decimal? MaximumOrderAmount { get; set; }

		/// <summary>
		/// Gets or sets the context identifier for amount restriction
		/// </summary>
		[DataMember]
		public int AmountRestrictionContextId { get; set; }

		/// <summary>
		/// Gets or sets the amount restriction context
		/// </summary>
		[DataMember]
		public AmountRestrictionContextType AmountRestrictionContext
		{
			get
			{
				return (AmountRestrictionContextType)this.AmountRestrictionContextId;
			}
			set
			{
				this.AmountRestrictionContextId = (int)value;
			}
		}

		/// <summary>
		/// Gets or sets the full description
		/// </summary>
		[DataMember]
		public string FullDescription { get; set; }
	}
}

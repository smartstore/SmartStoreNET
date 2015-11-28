
namespace SmartStore.Core.Domain.Common
{
	public enum CountryRestrictionContextType
	{
		BillingAddress = 0,
		ShippingAddress
	}

	public enum AmountRestrictionContextType
	{
		SubtotalAmount = 0,
		TotalAmount
	}
}

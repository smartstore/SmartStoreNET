
namespace SmartStore.Core.Domain.Payments
{
	public enum CountryExclusionContextType
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

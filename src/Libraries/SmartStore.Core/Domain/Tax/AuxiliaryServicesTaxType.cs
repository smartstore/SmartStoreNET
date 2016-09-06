namespace SmartStore.Core.Domain.Tax
{
	/// <summary>
	/// Specifies how to calculate the tax of auxiliary services like shipping and payment fees
	/// </summary>
	public enum AuxiliaryServicesTaxType
	{
		/// <summary>
		/// Calculate tax of auxiliary services with the tax category specified in settings
		/// </summary>
		SpecifiedTaxCategory = 0,

		/// <summary>
		/// Calculate tax with the tax rate that has the highest amount in the cart
		/// </summary>
		HighestCartAmount = 10,

		/// <summary>
		/// Calculate tax by the highest tax rate in the cart
		/// </summary>
		HighestTaxRate = 15,

		/// <summary>
		/// Calculate tax pro rata in accordance with main service (proportion of line subtotal and sum of all line subtotals)
		/// </summary>
		/// <remarks>commented out cause requires several plugins to be updated and migration of Order.OrderShippingTaxRate and Order.PaymentMethodAdditionalFeeTaxRate</remarks>
		///ProRata = 20
	}
}

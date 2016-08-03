namespace SmartStore.Core.Domain.Tax
{
	/// <summary>
	/// Specifies how to calculate the tax of subsidiary services like shipping and payment fees
	/// </summary>
	public enum SubsidiaryServicesTaxType
	{
		/// <summary>
		/// Calculate tax of subsidiary services by the tax category specified in settings
		/// </summary>
		SpecifiedTaxCategory = 0,

		/// <summary>
		/// Calculate tax by the tax rate of the product that has the highest cart subtotal
		/// </summary>
		HighestCartAmount = 10,

		/// <summary>
		/// Calculate tax pro rata in accordance with main service (proportion of line subtotal and sum of all line subtotals)
		/// </summary>
		ProRata = 20
	}
}

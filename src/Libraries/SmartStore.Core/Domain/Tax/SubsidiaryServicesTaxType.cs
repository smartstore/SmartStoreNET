namespace SmartStore.Core.Domain.Tax
{
	/// <summary>
	/// Specifies how to calculate the tax of subsidiary services like shipping and payment fees
	/// </summary>
	public enum SubsidiaryServicesTaxType
	{
		/// <summary>
		/// Calculate tax of subsidiary services by the tax rate specified in settings
		/// </summary>
		SpecifiedRate = 0,

		/// <summary>
		/// Calculate tax by the proportion of cart subtotal and sum of all subtotals
		/// </summary>
		ProRata = 10,

		/// <summary>
		/// Calculate tax by the tax rate of the product that has the highest cart subtotal
		/// </summary>
		HighestCartAmount = 20
	}
}

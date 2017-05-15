namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents the product sorting
    /// </summary>
    public enum ProductSortingEnum
    {
        /// <summary>
        /// Initial state
        /// </summary>
        Initial = 0,
		/// <summary>
		/// Relevance
		/// </summary>
		Relevance = 1,
        /// <summary>
        /// Name: A to Z
        /// </summary>
        NameAsc = 5,
        /// <summary>
        /// Name: Z to A
        /// </summary>
        NameDesc = 6,
        /// <summary>
        /// Price: Low to High
        /// </summary>
        PriceAsc = 10,
        /// <summary>
        /// Price: High to Low
        /// </summary>
        PriceDesc = 11,
        /// <summary>
        /// Product creation date
        /// </summary>
        CreatedOn = 15, // eigentlich CreatedOnDesc (wegen Lokalisierung bleibt das aber so)
        /// <summary>
        /// Product creation date
        /// </summary>
        CreatedOnAsc = 16
    }
}

namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents types of product prices to display
    /// </summary>
    public enum PriceDisplayType
    {
        /// <summary>
        /// The lowest possible price of a product (default)
        /// </summary>
        LowestPrice = 0,

        /// <summary>
        /// The product price initially displayed on the product detail page
        /// </summary>
        PreSelectedPrice = 10,

        /// <summary>
        /// The product price without associated data like discounts, tier prices, attributes or attribute combinations
        /// </summary>
        PriceWithoutDiscountsAndAttributes = 20,

        /// <summary>
        /// Do not display a product price
        /// </summary>
        Hide = 30
    }
}

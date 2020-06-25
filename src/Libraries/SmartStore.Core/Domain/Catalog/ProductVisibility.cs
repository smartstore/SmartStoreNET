namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Gradually restricts the visibility of products.
    /// </summary>
    public enum ProductVisibility
    {
        /// <summary>
        /// Product is fully visible.
        /// </summary>
        Full = 0,

        /// <summary>
        /// Product is visible in search results.
        /// </summary>
        SearchResults = 10,

        /// <summary>
        /// Product is not visible in lists but clickable on product pages.
        /// </summary>
        ProductPage = 20,

        /// <summary>
        /// Product is not visible but appears on grouped product pages.
        /// </summary>
        Hidden = 30
    }
}

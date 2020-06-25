namespace SmartStore.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product condition.
    /// <see cref="https://schema.org/OfferItemCondition"/>
    /// </summary>
    public enum ProductCondition
    {
        New = 0,
        Refurbished = 10,
        Used = 20,
        Damaged = 30
    }
}

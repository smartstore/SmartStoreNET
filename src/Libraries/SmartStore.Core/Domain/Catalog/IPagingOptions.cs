namespace SmartStore.Core.Domain.Catalog
{
    public interface IPagingOptions
    {
        int? PageSize { get; }
        bool? AllowCustomersToSelectPageSize { get; }
        string PageSizeOptions { get; }
    }
}

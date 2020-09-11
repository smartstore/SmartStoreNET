namespace SmartStore.Services.Catalog.Modelling
{
    public class GiftCardQueryItem
    {
        public GiftCardQueryItem(string name, string value)
        {
            Guard.NotEmpty(name, nameof(name));

            Name = name.ToLower();
            Value = value ?? string.Empty;

            if (Name.StartsWith("."))
            {
                Name = Name.Substring(1);
            }
        }

        public static string CreateKey(int productId, int bundleItemId, string name)
        {
            if (name.HasValue())
            {
                return $"giftcard{productId}-{bundleItemId}-.{name.EmptyNull().ToLower()}";
            }

            // Just return field prefix for partial views.
            return $"giftcard{productId}-{bundleItemId}-";
        }

        public string Name { get; private set; }
        public string Value { get; private set; }

        public int ProductId { get; set; }
        public int BundleItemId { get; set; }

        public override string ToString()
        {
            return CreateKey(ProductId, BundleItemId, Name);
        }
    }
}

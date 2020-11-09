using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Data.Mapping.Orders
{
    public partial class ShoppingCartItemMap : EntityTypeConfiguration<ShoppingCartItem>
    {
        public ShoppingCartItemMap()
        {
            this.ToTable("ShoppingCartItem");
            this.HasKey(sci => sci.Id);

            this.Property(sci => sci.CustomerEnteredPrice).HasPrecision(18, 4);
            this.Property(sci => sci.AttributesXml).IsMaxLength();

            this.Ignore(sci => sci.ShoppingCartType);
            this.Ignore(sci => sci.IsFreeShipping);
            this.Ignore(sci => sci.IsShipEnabled);
            this.Ignore(sci => sci.IsTaxExempt);

            this.HasRequired(sci => sci.Customer)
                .WithMany(c => c.ShoppingCartItems)
                .HasForeignKey(sci => sci.CustomerId);

            this.HasRequired(sci => sci.Product)
                .WithMany()
                .HasForeignKey(sci => sci.ProductId);

            this.HasOptional(sci => sci.BundleItem)
                .WithMany()
                .HasForeignKey(sci => sci.BundleItemId)
                .WillCascadeOnDelete(false);
        }
    }
}

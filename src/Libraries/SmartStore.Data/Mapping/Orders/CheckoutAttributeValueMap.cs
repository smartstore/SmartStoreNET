using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Data.Mapping.Orders
{
    public partial class CheckoutAttributeValueMap : EntityTypeConfiguration<CheckoutAttributeValue>
    {
        public CheckoutAttributeValueMap()
        {
            ToTable("CheckoutAttributeValue");
            HasKey(cav => cav.Id);
            Property(cav => cav.Name).IsRequired().HasMaxLength(400);
            Property(cav => cav.PriceAdjustment).HasPrecision(18, 4);
            Property(cav => cav.WeightAdjustment).HasPrecision(18, 4);

            HasOptional(x => x.MediaFile)
                .WithMany()
                .HasForeignKey(x => x.MediaFileId)
                .WillCascadeOnDelete(false);

            HasRequired(cav => cav.CheckoutAttribute)
                .WithMany(ca => ca.CheckoutAttributeValues)
                .HasForeignKey(cav => cav.CheckoutAttributeId);
        }
    }
}
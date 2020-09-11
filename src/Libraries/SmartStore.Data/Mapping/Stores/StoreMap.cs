using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Data.Mapping.Stores
{
    public partial class StoreMap : EntityTypeConfiguration<Store>
    {
        public StoreMap()
        {
            ToTable("Store");
            HasKey(s => s.Id);
            Property(s => s.Name).IsRequired().HasMaxLength(400);
            Property(s => s.Url).IsRequired().HasMaxLength(400);
            Property(s => s.ContentDeliveryNetwork).HasMaxLength(400);
            Property(s => s.SecureUrl).HasMaxLength(400);
            Property(s => s.Hosts).HasMaxLength(1000);
            Property(x => x.LogoMediaFileId).HasColumnName("LogoMediaFileId");

            HasRequired(s => s.PrimaryStoreCurrency)
                .WithMany()
                .HasForeignKey(s => s.PrimaryStoreCurrencyId)
                .WillCascadeOnDelete(false);

            HasRequired(s => s.PrimaryExchangeRateCurrency)
                .WithMany()
                .HasForeignKey(s => s.PrimaryExchangeRateCurrencyId)
                .WillCascadeOnDelete(false);
        }
    }
}

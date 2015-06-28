using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Data.Mapping.Stores
{
	public partial class StoreMap : EntityTypeConfiguration<Store>
	{
		public StoreMap()
		{
			this.ToTable("Store");
			this.HasKey(s => s.Id);
			this.Property(s => s.Name).IsRequired().HasMaxLength(400);
			this.Property(s => s.Url).IsRequired().HasMaxLength(400);
			this.Property(s => s.ContentDeliveryNetwork).HasMaxLength(400);
			this.Property(s => s.SecureUrl).HasMaxLength(400);
			this.Property(s => s.Hosts).HasMaxLength(1000);

			this.HasRequired(s => s.PrimaryStoreCurrency)
				.WithMany()
				.HasForeignKey(s => s.PrimaryStoreCurrencyId)
				.WillCascadeOnDelete(false);

			this.HasRequired(s => s.PrimaryExchangeRateCurrency)
				.WithMany()
				.HasForeignKey(s => s.PrimaryExchangeRateCurrencyId)
				.WillCascadeOnDelete(false);
		}
	}
}

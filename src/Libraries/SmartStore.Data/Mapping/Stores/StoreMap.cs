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
			this.Property(s => s.SecureUrl).HasMaxLength(400);
			this.Property(s => s.Hosts).HasMaxLength(1000);
		}
	}
}

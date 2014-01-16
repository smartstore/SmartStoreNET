using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
	public partial class ProductBundleMap : EntityTypeConfiguration<ProductBundle>
	{
		public ProductBundleMap()
		{
			this.ToTable("ProductBundle");
			this.HasKey(pb => pb.Id);

			this.Property(pb => pb.Discount).HasPrecision(18, 4);
			this.Property(pb => pb.Name).HasMaxLength(400);
			this.Property(pb => pb.ShortDescription).IsMaxLength();
		}
	}
}

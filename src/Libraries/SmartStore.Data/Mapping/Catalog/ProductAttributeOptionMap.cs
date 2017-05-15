using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
	public partial class ProductAttributeOptionMap : EntityTypeConfiguration<ProductAttributeOption>
	{
		public ProductAttributeOptionMap()
		{
			ToTable("ProductAttributeOption");
			HasKey(x => x.Id);
			Property(x => x.Name).HasMaxLength(4000);
			Property(x => x.Alias).HasMaxLength(100);
			Property(x => x.Color).HasMaxLength(100);

			Property(x => x.PriceAdjustment).HasPrecision(18, 4);
			Property(x => x.WeightAdjustment).HasPrecision(18, 4);

			Ignore(x => x.ValueType);

			HasRequired(x => x.ProductAttributeOptionsSet)
				.WithMany(x => x.ProductAttributeOptions)
				.HasForeignKey(x => x.ProductAttributeOptionsSetId);
		}
	}
}

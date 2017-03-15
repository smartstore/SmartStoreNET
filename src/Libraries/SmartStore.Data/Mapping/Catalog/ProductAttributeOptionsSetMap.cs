using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
	public partial class ProductAttributeOptionsSetMap : EntityTypeConfiguration<ProductAttributeOptionsSet>
	{
		public ProductAttributeOptionsSetMap()
		{
			ToTable("ProductAttributeOptionsSet");
			HasKey(x => x.Id);
			Property(x => x.Name).HasMaxLength(400);

			HasRequired(x => x.ProductAttribute)
				.WithMany(x => x.ProductAttributeOptionsSets)
				.HasForeignKey(x => x.ProductAttributeId);
		}
	}
}

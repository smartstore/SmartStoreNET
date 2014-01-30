using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
	public partial class ProductBundleItemAttributeFilterMap : EntityTypeConfiguration<ProductBundleItemAttributeFilter>
	{
		public ProductBundleItemAttributeFilterMap()
		{
			this.ToTable("ProductBundleItemAttributeFilter");
			this.HasKey(biaf => biaf.Id);

			this.HasRequired(biaf => biaf.BundleItem)
				.WithMany(pbi => pbi.AttributeFilters)
				.HasForeignKey(biaf => biaf.BundleItemId)
				.WillCascadeOnDelete(true);
		}
	}
}

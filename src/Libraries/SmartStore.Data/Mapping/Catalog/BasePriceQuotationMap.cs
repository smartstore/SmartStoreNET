using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
	public partial class BasePriceQuotationMap : ComplexTypeConfiguration<BasePriceQuotation>
	{
		public BasePriceQuotationMap()
		{
			this.Property(x => x.Enabled).HasColumnName("BasePrice_Enabled");
			this.Property(x => x.MeasureUnit).HasColumnName("BasePrice_MeasureUnit");
			this.Property(x => x.Amount).HasColumnName("BasePrice_Amount");
			this.Property(x => x.BaseAmount).HasColumnName("BasePrice_BaseAmount");
		}
	}
}

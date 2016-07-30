using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Data.Mapping.Media
{
	public partial class BinaryDataMap : EntityTypeConfiguration<BinaryData>
	{
		public BinaryDataMap()
		{
			ToTable("BinaryData");
			HasKey(x => x.Id);
			Property(x => x.Data).IsRequired().IsMaxLength();
		}
	}
}

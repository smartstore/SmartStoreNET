using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Data.Mapping.Media
{
	public partial class MediaStorageMap : EntityTypeConfiguration<MediaStorage>
	{
		public MediaStorageMap()
		{
			ToTable("MediaStorage");
			HasKey(x => x.Id);
			Property(x => x.Data).IsRequired().IsMaxLength();
		}
	}
}

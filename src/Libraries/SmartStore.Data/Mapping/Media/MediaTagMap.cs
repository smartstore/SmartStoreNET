using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Data.Mapping.Media
{
    public partial class MediaTagMap : EntityTypeConfiguration<MediaTag>
    {
        public MediaTagMap()
        {
            ToTable("MediaTag");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(100);

            HasIndex(x => x.Name).HasName("IX_MediaTag_Name");
        }
    }
}

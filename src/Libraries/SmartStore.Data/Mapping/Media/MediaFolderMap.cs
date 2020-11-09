using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Data.Mapping.Media
{
    public partial class MediaFolderMap : EntityTypeConfiguration<MediaFolder>
    {
        public MediaFolderMap()
        {
            ToTable("MediaFolder");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(255);
            Property(c => c.Slug).HasMaxLength(255);
            Property(c => c.Metadata).IsMaxLength();

            HasOptional(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(false);
        }
    }

    public partial class MediaAlbumMap : EntityTypeConfiguration<MediaAlbum>
    {
        public MediaAlbumMap()
        {
            Property(c => c.ResKey).HasMaxLength(255);
        }
    }
}

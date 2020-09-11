using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Data.Mapping.Media
{
    public partial class MediaTrackMap : EntityTypeConfiguration<MediaTrack>
    {
        public MediaTrackMap()
        {
            ToTable("MediaTrack");
            HasKey(x => x.Id);

            Property(x => x.EntityId);
            Property(x => x.EntityName).IsRequired().HasMaxLength(255);
            Property(x => x.Property).HasMaxLength(255);
            Property(x => x.Album).IsRequired().HasMaxLength(50);

            HasRequired(x => x.MediaFile)
                .WithMany(x => x.Tracks)
                .HasForeignKey(x => x.MediaFileId)
                .WillCascadeOnDelete(true);

            HasIndex(x => x.Album).HasName("IX_Album");
        }
    }
}
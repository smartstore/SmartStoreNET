using System;
using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Data.Mapping.Media
{
    public partial class MediaRelationMap : EntityTypeConfiguration<MediaRelation>
    {
        public MediaRelationMap()
        {
            ToTable("MediaRelation");
            HasKey(x => x.Id);

            Property(x => x.EntityId);
            Property(x => x.EntityName).IsRequired().HasMaxLength(255);
            Property(x => x.Album).IsRequired().HasMaxLength(50);
            Property(x => x.HashCode).HasColumnOrder(100);

            HasIndex(x => x.Album).HasName("IX_Album");
            HasIndex(x => x.HashCode).HasName("IX_HashCode").IsUnique(true);
        }
    }
}
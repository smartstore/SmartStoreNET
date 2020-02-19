using System;
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
            Property(c => c.Name).IsRequired().HasMaxLength(100);
            Property(c => c.Slug).HasMaxLength(100);
            Property(c => c.Metadata).IsMaxLength();

            HasOptional(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .WillCascadeOnDelete(false);

            //Map<MediaFolder>(m => m.Requires("FolderType").HasValue(1).HasColumnType("int"));
        }
    }

    public partial class MediaRegionMap : EntityTypeConfiguration<MediaRegion>
    {
        public MediaRegionMap()
        {
            Property(c => c.ResKey).HasMaxLength(255);
            Property(c => c.Icon).HasMaxLength(100);
            Property(c => c.Color).HasMaxLength(100);

            //Map<MediaRegion>(m => m.Requires("FolderType").HasValue(0).HasColumnType("int"));
        }
    }

    //public partial class MediaFolderMap : EntityTypeConfiguration<MediaRegion>
    //{
    //    public MediaFolderMap()
    //    {
    //        ToTable("MediaFolder");
    //        HasKey(c => c.Id);
    //        Property(c => c.Name).IsRequired().HasMaxLength(100);
    //        Property(c => c.Slug).HasMaxLength(100);
    //        Property(c => c.Metadata).IsMaxLength();
    //        Property(c => c.ResKey).HasMaxLength(255);
    //        Property(c => c.Icon).HasMaxLength(100);
    //        Property(c => c.Color).HasMaxLength(100);

    //        HasOptional(x => x.Parent)
    //            .WithMany(x => x.Children)
    //            .HasForeignKey(x => x.ParentId)
    //            .WillCascadeOnDelete(false);

    //        Map<MediaFolder>(m => m.Requires("FolderType").HasValue(1).HasColumnType("int"));
    //        Map<MediaRegion>(m => m.Requires("FolderType").HasValue(0).HasColumnType("int"));
    //    }
    //}
}

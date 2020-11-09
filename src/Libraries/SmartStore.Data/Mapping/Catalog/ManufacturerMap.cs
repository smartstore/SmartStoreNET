using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class ManufacturerMap : EntityTypeConfiguration<Manufacturer>
    {
        public ManufacturerMap()
        {
            ToTable("Manufacturer");
            HasKey(m => m.Id);
            Property(m => m.Name).IsRequired().HasMaxLength(400);
            Property(m => m.Description).IsMaxLength();
            Property(c => c.BottomDescription).IsMaxLength();
            Property(m => m.MetaKeywords).HasMaxLength(400);
            Property(m => m.MetaTitle).HasMaxLength(400);
            Property(m => m.PageSizeOptions).HasMaxLength(200).IsOptional();
            Property(m => m.MediaFileId).HasColumnName("MediaFileId");

            HasOptional(p => p.MediaFile)
                .WithMany()
                .HasForeignKey(p => p.MediaFileId)
                .WillCascadeOnDelete(false);
        }
    }
}
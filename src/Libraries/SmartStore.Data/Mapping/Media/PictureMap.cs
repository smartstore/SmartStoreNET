using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Data.Mapping.Media
{
    public partial class PictureMap : EntityTypeConfiguration<Picture>
    {
        public PictureMap()
        {
            this.ToTable("Picture");
            this.HasKey(p => p.Id);

#pragma warning disable 612, 618
			this.Property(p => p.PictureBinary).IsMaxLength();
#pragma warning restore 612, 618

			this.Property(p => p.MimeType).IsRequired().HasMaxLength(40);
            this.Property(p => p.SeoFilename).HasMaxLength(300);

			HasOptional(x => x.MediaStorage)
				.WithMany()
				.HasForeignKey(x => x.MediaStorageId)
				.WillCascadeOnDelete(false);
        }
    }
}
using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Data.Mapping.Media
{
    public partial class DownloadMap : EntityTypeConfiguration<Download>
    {
        public DownloadMap()
        {
            this.ToTable("Download");
            this.HasKey(p => p.Id);

#pragma warning disable 612, 618
			this.Property(p => p.DownloadBinary).IsMaxLength();
#pragma warning restore 612, 618

			HasOptional(x => x.MediaStorage)
				.WithMany()
				.HasForeignKey(x => x.MediaStorageId)
				.WillCascadeOnDelete(false);
		}
    }
}
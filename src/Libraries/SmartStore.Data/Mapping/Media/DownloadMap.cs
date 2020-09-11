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

            HasOptional(x => x.MediaFile)
                .WithMany()
                .HasForeignKey(x => x.MediaFileId)
                .WillCascadeOnDelete(false);
        }
    }
}
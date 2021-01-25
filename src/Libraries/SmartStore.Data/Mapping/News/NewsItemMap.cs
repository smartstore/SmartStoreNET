using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.News;

namespace SmartStore.Data.Mapping.News
{
    public partial class NewsItemMap : EntityTypeConfiguration<NewsItem>
    {
        public NewsItemMap()
        {
            ToTable("News");
            HasKey(x => x.Id);
            Property(x => x.Title).IsRequired();
            Property(x => x.Short).IsRequired();
            Property(x => x.Full).IsRequired().IsMaxLength();
            Property(x => x.MetaKeywords).HasMaxLength(400);
            Property(x => x.MetaTitle).HasMaxLength(400);
            Property(x => x.MediaFileId).HasColumnName("MediaFileId");
            Property(x => x.PreviewMediaFileId).HasColumnName("PreviewMediaFileId");

            HasOptional(x => x.Language)
                .WithMany()
                .HasForeignKey(x => x.LanguageId)
                .WillCascadeOnDelete(false);

            HasOptional(bp => bp.MediaFile)
                .WithMany()
                .HasForeignKey(bp => bp.MediaFileId)
                .WillCascadeOnDelete(false);

            HasOptional(bp => bp.PreviewMediaFile)
                .WithMany()
                .HasForeignKey(bp => bp.PreviewMediaFileId)
                .WillCascadeOnDelete(false);
        }
    }
}
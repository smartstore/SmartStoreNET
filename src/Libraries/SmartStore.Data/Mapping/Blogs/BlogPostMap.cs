using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Blogs;

namespace SmartStore.Data.Mapping.Blogs
{
    public partial class BlogPostMap : EntityTypeConfiguration<BlogPost>
    {
        public BlogPostMap()
        {
            ToTable("BlogPost");
            HasKey(bp => bp.Id);
            Property(bp => bp.Title).IsRequired();
            Property(bp => bp.Body).IsRequired().IsMaxLength();
            Property(bp => bp.MetaKeywords).HasMaxLength(400);
            Property(bp => bp.MetaTitle).HasMaxLength(400);
            Property(bp => bp.MediaFileId).HasColumnName("MediaFileId");
            Property(bp => bp.PreviewMediaFileId).HasColumnName("PreviewMediaFileId");

            HasOptional(bp => bp.Language)
                .WithMany()
                .HasForeignKey(bp => bp.LanguageId)
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
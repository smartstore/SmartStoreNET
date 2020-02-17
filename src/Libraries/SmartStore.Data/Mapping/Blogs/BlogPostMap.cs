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
            Property(bp => bp.PictureId).HasColumnName("MediaFileId");
            Property(bp => bp.PreviewPictureId).HasColumnName("PreviewMediaFileId");

            this.HasRequired(bp => bp.Language)
                .WithMany()
                .HasForeignKey(bp => bp.LanguageId).WillCascadeOnDelete(true);
        }
    }
}
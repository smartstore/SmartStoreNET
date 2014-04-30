using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Blogs;

namespace SmartStore.Data.Mapping.Blogs
{
    public partial class BlogCommentMap : EntityTypeConfiguration<BlogComment>
    {
        public BlogCommentMap()
        {
            this.ToTable("BlogComment");
            //commented because it's already configured by CustomerContentMap class
            //this.HasKey(pr => pr.Id);

			this.Property(bc => bc.CommentText).IsMaxLength();
            this.HasRequired(bc => bc.BlogPost)
                .WithMany(bp => bp.BlogComments)
                .HasForeignKey(bc => bc.BlogPostId);
        }
    }
}
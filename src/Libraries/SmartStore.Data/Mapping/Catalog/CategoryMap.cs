using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class CategoryMap : EntityTypeConfiguration<Category>
    {
        public CategoryMap()
        {
            this.ToTable("Category");
            this.HasKey(c => c.Id);
            this.Property(c => c.Name).IsRequired().HasMaxLength(400);
			this.Property(c => c.FullName).HasMaxLength(400);
			this.Property(c => c.BottomDescription).IsMaxLength();
			this.Property(c => c.Description).IsMaxLength();
            this.Property(c => c.MetaKeywords).HasMaxLength(400);
            this.Property(c => c.MetaTitle).HasMaxLength(400);
			this.Property(c => c.PageSizeOptions).HasMaxLength(200).IsOptional();
			this.Property(c => c.Alias).HasMaxLength(100);
			this.HasOptional(p => p.Picture)
				.WithMany()
				.HasForeignKey(p => p.PictureId)
				.WillCascadeOnDelete(false);
        }
    }
}

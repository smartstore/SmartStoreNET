using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class CategoryMap : EntityTypeConfiguration<Category>
    {
        public CategoryMap()
        {
            ToTable("Category");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(400);
            Property(c => c.FullName).HasMaxLength(400);
            Property(c => c.BottomDescription).IsMaxLength();
            Property(c => c.ExternalLink).HasMaxLength(255).IsOptional();
            Property(c => c.Description).IsMaxLength();
            Property(c => c.MetaKeywords).HasMaxLength(400);
            Property(c => c.MetaTitle).HasMaxLength(400);
            Property(c => c.PageSizeOptions).HasMaxLength(200).IsOptional();
            Property(c => c.Alias).HasMaxLength(100);
            Property(c => c.MediaFileId).HasColumnName("MediaFileId");

            HasOptional(p => p.MediaFile)
                .WithMany()
                .HasForeignKey(p => p.MediaFileId)
                .WillCascadeOnDelete(false);

            HasMany(d => d.RuleSets)
                .WithMany(rs => rs.Categories)
                .Map(m => m.ToTable("RuleSet_Category_Mapping"));
        }
    }
}

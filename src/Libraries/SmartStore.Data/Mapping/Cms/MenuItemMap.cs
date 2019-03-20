using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Cms;

namespace SmartStore.Data.Mapping.Cms
{
    public class MenuItemMap : EntityTypeConfiguration<MenuItem>
    {
        public MenuItemMap()
        {
            ToTable("MenuItem");
            HasKey(x => x.Id);
            Property(x => x.SystemName).HasMaxLength(400);
            Property(x => x.Model).IsMaxLength();
            Property(x => x.Title).HasMaxLength(400);
            Property(x => x.ShortDescription).HasMaxLength(400);
            Property(x => x.HtmlId).HasMaxLength(100);
            Property(x => x.CssClass).HasMaxLength(100);

            HasRequired(x => x.Menu)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.MenuId);
        }
    }
}

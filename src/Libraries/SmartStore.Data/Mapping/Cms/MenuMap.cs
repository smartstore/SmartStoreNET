using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Cms;

namespace SmartStore.Data.Mapping.Cms
{
    public class MenuMap : EntityTypeConfiguration<Menu>
    {
        public MenuMap()
        {
            ToTable("Menu");
            HasKey(x => x.Id);
            Property(x => x.SystemName).IsRequired().HasMaxLength(400);
            Property(x => x.Title).HasMaxLength(400);
        }
    }
}

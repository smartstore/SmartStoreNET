using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Configuration;

namespace SmartStore.Data.Mapping.Configuration
{
    public partial class SettingMap : EntityTypeConfiguration<Setting>
    {
        public SettingMap()
        {
            ToTable("Setting");
            HasKey(x => x.Id);
            Property(x => x.Name).IsRequired().HasMaxLength(400);
            Property(x => x.Value).IsRequired().IsMaxLength(); //.HasMaxLength(2000);

            HasIndex(x => x.Name).HasName("IX_Setting_Name");
            HasIndex(x => x.StoreId).HasName("IX_Setting_StoreId");
        }
    }
}
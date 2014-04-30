using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Configuration;

namespace SmartStore.Data.Mapping.Configuration
{
    public partial class SettingMap : EntityTypeConfiguration<Setting>
    {
        public SettingMap()
        {
            this.ToTable("Setting");
            this.HasKey(s => s.Id);
            this.Property(s => s.Name).IsRequired().HasMaxLength(200);
            this.Property(s => s.Value).IsRequired().IsMaxLength(); //.HasMaxLength(2000);
        }
    }
}
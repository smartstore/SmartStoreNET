using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Plugins;

namespace SmartStore.Data.Mapping.Plugins
{
	public partial class LicenseMap : EntityTypeConfiguration<License>
	{
		public LicenseMap()
		{
			this.ToTable("License");
			this.HasKey(a => a.Id);

			this.Property(x => x.LicenseKey).IsRequired().HasMaxLength(400);
			this.Property(x => x.SystemName).IsRequired().HasMaxLength(400);
		}
	}
}

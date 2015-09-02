using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Data.Mapping.DataExchange
{
	public partial class SyncMappingMap : EntityTypeConfiguration<SyncMapping>
	{
		public SyncMappingMap()
		{
			this.ToTable("SyncMapping");
			this.HasKey(x => x.Id);

			this.Property(x => x.EntityName).IsRequired().HasMaxLength(100);
			this.Property(x => x.ContextName).IsRequired().HasMaxLength(100);
			this.Property(x => x.SourceKey).IsRequired().HasMaxLength(150);
			this.Property(x => x.SourceHash).HasMaxLength(40);
			this.Property(x => x.CustomString).IsMaxLength();
		}
	}
}
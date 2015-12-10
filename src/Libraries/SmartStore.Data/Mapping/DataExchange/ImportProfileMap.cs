using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain;

namespace SmartStore.Data.Mapping.DataExchange
{
	public partial class ImportProfileMap : EntityTypeConfiguration<ImportProfile>
	{
		public ImportProfileMap()
		{
			this.ToTable("ImportProfile");
			this.HasKey(x => x.Id);

			this.Property(x => x.Name).IsRequired().HasMaxLength(100);
			this.Property(x => x.FolderName).IsRequired().HasMaxLength(100);
			this.Property(x => x.FileTypeConfiguration).IsMaxLength();
			this.Property(x => x.ColumnMapping).IsMaxLength();

			this.Ignore(x => x.EntityType);

			this.HasRequired(x => x.ScheduleTask)
				.WithMany()
				.HasForeignKey(x => x.SchedulingTaskId)
				.WillCascadeOnDelete(false);
		}
	}
}

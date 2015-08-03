using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain;

namespace SmartStore.Data.Mapping.Export
{
	public partial class ExportProfileMap : EntityTypeConfiguration<ExportProfile>
	{
		public ExportProfileMap()
		{
			this.ToTable("ExportProfile");
			this.HasKey(x => x.Id);

			this.Property(x => x.Name).IsRequired().HasMaxLength(100);
			this.Property(x => x.ProviderSystemName).IsRequired().HasMaxLength(4000);
			this.Property(x => x.Partitioning).IsMaxLength();
			this.Property(x => x.Filtering).IsMaxLength();
			this.Property(x => x.LastExecutionMessage).HasMaxLength(4000);

			this.Ignore(x => x.FileType);

			this.HasRequired(x => x.ScheduleTask)
				.WithMany()
				.HasForeignKey(x => x.SchedulingTaskId)
				.WillCascadeOnDelete(false);
		}
	}
}

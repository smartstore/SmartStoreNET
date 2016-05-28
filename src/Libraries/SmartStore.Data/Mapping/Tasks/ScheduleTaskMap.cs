using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Data.Mapping.Tasks
{
    public partial class ScheduleTaskMap : EntityTypeConfiguration<ScheduleTask>
    {
        public ScheduleTaskMap()
        {
            this.ToTable("ScheduleTask");
            this.HasKey(t => t.Id);
            this.Property(t => t.Name).HasMaxLength(500).IsRequired();
            this.Property(t => t.Type).HasMaxLength(800).IsRequired();
			this.Property(t => t.Alias).HasMaxLength(500);
			this.Property(t => t.LastError).HasMaxLength(1000);
			this.Property(t => t.ProgressMessage).HasMaxLength(1000).IsOptional();
			this.Property(t => t.CronExpression).HasMaxLength(1000);
			this.Property(t => t.RowVersion).IsConcurrencyToken();

			this.Ignore(t => t.IsRunning);
			this.Ignore(t => t.IsPending);
        }
    }
}
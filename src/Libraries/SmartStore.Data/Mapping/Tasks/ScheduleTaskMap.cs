using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Data.Mapping.Tasks
{
    public partial class ScheduleTaskMap : EntityTypeConfiguration<ScheduleTask>
    {
        public ScheduleTaskMap()
        {
            ToTable("ScheduleTask");
            HasKey(t => t.Id);
            Property(t => t.Name).HasMaxLength(500).IsRequired();
            Property(t => t.Type).HasMaxLength(800).IsRequired();
            Property(t => t.Alias).HasMaxLength(500);
            Property(t => t.CronExpression).HasMaxLength(1000);

            Ignore(t => t.IsPending);
            Ignore(t => t.LastHistoryEntry);
        }
    }
}
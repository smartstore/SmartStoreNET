using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Tasks;

namespace SmartStore.Data.Mapping.Tasks
{
    public partial class ScheduleTaskHistoryMap : EntityTypeConfiguration<ScheduleTaskHistory>
    {
        public ScheduleTaskHistoryMap()
        {
            ToTable("ScheduleTaskHistory");
            HasKey(x => x.Id);
            Property(x => x.MachineName).IsRequired().HasMaxLength(400);
            Property(x => x.Error);
            Property(x => x.ProgressMessage).HasMaxLength(1000);

            HasRequired(x => x.ScheduleTask)
                .WithMany(x => x.ScheduleTaskHistory)
                .HasForeignKey(x => x.ScheduleTaskId);
        }
    }
}
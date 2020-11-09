namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Domain.Tasks;
    using SmartStore.Data.Setup;

    public partial class ScheduleTaskPriority : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.ScheduleTask", "Priority", c => c.Int(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.ScheduleTask", "Priority");
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            var highPriorityTaskTypes = new[]
            {
                "SmartStore.Services.Messages.QueuedMessagesSendTask, SmartStore.Services",
                "SmartStore.Services.Directory.UpdateExchangeRateTask, SmartStore.Services",
                //"SmartStore.MegaSearch.IndexingTask, SmartStore.MegaSearch",
            };

            context.MigrateLocaleResources(MigrateLocaleResources);

            var tasks = context.Set<ScheduleTask>().Where(x => highPriorityTaskTypes.Contains(x.Type)).ToList();
            foreach (var task in tasks)
            {
                task.Priority = TaskPriority.High;
            }

            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate(
                "Admin.System.ScheduleTasks.Priority",
                "Priority",
                "Wichtigkeit",
                "Tasks with higher priority run first when multiple tasks are pending.",
                "Aufgaben mit höherer Wichtigkeit werden zuerst ausgeführt, wenn mehrere Aufgaben zur Ausführung anstehen.");

            builder.AddOrUpdate("Admin.System.ScheduleTasks.Priority.Low", "Low", "Niedrig");
            builder.AddOrUpdate("Admin.System.ScheduleTasks.Priority.Normal", "Normal", "Normal");
            builder.AddOrUpdate("Admin.System.ScheduleTasks.Priority.High", "High", "Hoch");
        }
    }
}

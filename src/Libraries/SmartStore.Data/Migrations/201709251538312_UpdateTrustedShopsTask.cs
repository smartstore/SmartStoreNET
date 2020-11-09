namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Core.Domain.Tasks;
    using Setup;

    public partial class UpdateTrustedShopsTask : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
        }

        public override void Down()
        {
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            var table = context.Set<ScheduleTask>();
            var taskToDelete = table.Where(x => x.Type.Contains("SmartStore.TrustedShops.UpdateRatingWidgetStateTask")).FirstOrDefault();

            if (taskToDelete != null)
            {
                // remove old task
                table.Remove(taskToDelete);

                // if task to remnove is not null we assume the plugin is currently installed and so we add the new task without any further request
                var taskToAdd = new ScheduleTask
                {
                    Name = "Trusted Shops Review Import",
                    CronExpression = "0 4 * * *", // At 04:00 am
                    Type = "SmartStore.TrustedShops.ImportProductReviewsTask, SmartStore.TrustedShops",
                    Enabled = false,
                    StopOnError = true
                };

                table.Add(taskToAdd);
            }

            context.SaveChanges();
        }
    }
}

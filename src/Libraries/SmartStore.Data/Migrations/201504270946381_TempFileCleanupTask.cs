namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using SmartStore.Core.Domain.Tasks;
	using SmartStore.Data.Setup;

	public partial class TempFileCleanupTask : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
        }
        
        public override void Down()
        {
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.Set<ScheduleTask>().AddOrUpdate(x => x.Type,
				new ScheduleTask
				{
					Name = "Cleanup temporary files",
					CronExpression = "30 3 * * *",
					Type = "SmartStore.Services.Common.TempFileCleanupTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false
				}
			);

			context.SaveChanges();
		}
    }
}

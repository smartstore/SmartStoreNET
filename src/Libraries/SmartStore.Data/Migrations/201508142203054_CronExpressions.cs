namespace SmartStore.Data.Migrations
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using SmartStore.Core.Domain.Tasks;
	using SmartStore.Data.Setup;

	public partial class CronExpressions : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
			AddColumn("dbo.ScheduleTask", "CronExpression", c => c.String(maxLength: 1000, defaultValue: "0 */1 * * *" /* Every hour */));
            DropColumn("dbo.ScheduleTask", "Seconds");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ScheduleTask", "Seconds", c => c.Int(nullable: false));
            DropColumn("dbo.ScheduleTask", "CronExpression");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			// Seconds > CronExpressions
			var table = context.Set<ScheduleTask>();
			var tasks = table.ToList();

			foreach (var task in tasks)
			{
				if (task.Type.Contains(".QueuedMessagesSendTask"))
				{
					task.CronExpression = "* * * * *"; // every Minute
				}
				else if (task.Type.Contains(".DeleteGuestsTask"))
				{
					task.CronExpression = "*/10 * * * *"; // every 10 Minutes
				}
				else if (task.Type.Contains(".ClearCacheTask"))
				{
					task.CronExpression = "0 */4 * * *"; // every 4 hrs
				}
				else if (task.Type.Contains(".UpdateExchangeRateTask"))
				{
					task.CronExpression = "0/15 * * * *"; // every 15 Minutes
				}
				else if (task.Type.Contains(".DeleteLogsTask"))
				{
					task.CronExpression = "0 1 * * *"; // At 01:00
				}
				else if (task.Type.Contains(".TransientMediaClearTask"))
				{
					task.CronExpression = "30 1,13 * * *"; // At 01:30 and 13:30
				}
				else if (task.Type.Contains(".QueuedMessagesClearTask"))
				{
					task.CronExpression = "0 2 * * *"; // At 02:00
				}
				else if (task.Type.Contains(".UpdateRatingWidgetStateTask"))
				{
					task.CronExpression = "0 3 * * *"; // At 03:00
				}
				else if (task.Type.Contains(".MailChimpSynchronizationTask"))
				{
					task.CronExpression = "0 */1 * * *"; // Every hour
				}
				else if (task.Type.Contains(".AmazonPay.DataPollingTask"))
				{
					task.CronExpression = "*/30 * * * *"; // Every 30 minutes
				}
				else if (task.Type.Contains(".NewsImportTask"))
				{
					task.CronExpression = "30 */12 * * *"; // At 30 minutes past the hour, every 12 hours
				}
				else if (task.Type.Contains(".TempFileCleanupTask"))
				{
					task.CronExpression = "30 3 * * *"; // At 03:30
				}
				else if (task.Type.Contains(".BMEcat.FileImportTask"))
				{
					task.CronExpression = "30 2 * * *"; // At 02:30
				}
				else if (task.Type.Contains(".StaticFileGenerationTask"))
				{
					task.CronExpression = "0 */6 * * *"; // Every 06 hours
				}
				else
				{
					task.CronExpression = "0 */1 * * *"; // Every hour
				}
			}

			context.SaveChanges();
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			// ...
		}
    }
}

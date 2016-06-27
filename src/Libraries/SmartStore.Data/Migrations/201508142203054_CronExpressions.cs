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
			AddColumn("dbo.ScheduleTask", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
            DropColumn("dbo.ScheduleTask", "Seconds");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ScheduleTask", "Seconds", c => c.Int(nullable: false));
			DropColumn("dbo.ScheduleTask", "RowVersion");
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
			builder.Delete(
				"Admin.System.ScheduleTasks.Seconds",
				"Admin.System.ScheduleTasks.Seconds.Positive", 
				"Admin.System.ScheduleTasks.RunNow.Completed");
			
			builder.AddOrUpdate("Common.Rule",
				"Rule",
				"Regel");
			builder.AddOrUpdate("Common.Scheduled",
				"Scheduled",
				"Geplant");
			builder.AddOrUpdate("Common.Unscheduled",
				"Unscheduled",
				"Ungeplant");
	
			builder.AddOrUpdate("Admin.System.ScheduleTasks.CronExpression",
				"Cron Expression",
				"Cron Ausdruck",
				"An expression that defines the schedule for the automatic execution of the task.",
				"Ein Ausdruck, der den Zeitplan für die automatische Ausführung der Aufgabe festlegt.");
			builder.AddOrUpdate("Admin.System.ScheduleTasks.Enabled.Hint",
				"Enables the scheduled execution of the task in accordance with the cron expression",
				"Aktiviert die geplante Ausführung der Aufgabe gemäß Cron Ausdruck");
			builder.AddOrUpdate("Admin.System.ScheduleTasks.StopOnError",
				"Disable on error",
				"Bei Fehler deaktivieren",
				"Check the box if the task should be disabled automatically when an error occurs during execution",
				"Aktivieren Sie das Kästchen, wenn die Aufgabe bei Auftreten eines Fehlers während der Ausführung deaktiviert werden soll");
			builder.AddOrUpdate("Admin.System.ScheduleTasks.LastStart.Hint",
				"Start date of the last execution",
				"Startdatum der letzten Ausführung");
			builder.AddOrUpdate("Admin.System.ScheduleTasks.LastSuccess.Hint",
				"Start date of the last successful execution",
				"Startdatum der letzten erfolgreichen Ausführung");
			builder.AddOrUpdate("Admin.System.ScheduleTasks.Duration.Hint",
				"Duration of the latest execution ([h]:[min]:[sec])",
				"Dauer der letzten Ausführung ([Std.]:[Min.]:[Sek.])");
			builder.AddOrUpdate("Admin.System.ScheduleTasks.NextRun.Hint",
				"Date of the next scheduled execution",
				"Datum der nächsten geplanten Ausführung");

			builder.AddOrUpdate("Admin.System.ScheduleTasks.CronHelp",
				"<a href='{0}' target='_blank'>Cron Expressions</a> help",
				"Hilfe zu <a href='{0}' target='_blank'>Cron-Ausdrücken</a>");

			builder.AddOrUpdate("Admin.System.ScheduleTasks.FutureSchedules",
				"Future schedules",
				"Zukünftige Zeitpläne");
			builder.AddOrUpdate("Admin.System.ScheduleTasks.EditTask",
				"Edit task",
				"Aufgabe bearbeiten");
			builder.AddOrUpdate("Admin.System.ScheduleTasks.ScheduleExecution",
				"Schedule execution",
				"Ausführung planen");
			builder.AddOrUpdate("Admin.System.ScheduleTasks.InvalidCronExpression",
				"The cron expression is invalid",
				"Der Cron-Ausdruck ist ungültig");
			builder.AddOrUpdate("Admin.System.ScheduleTasks.RunNow.Success",
				"The task has been executed successfully",
				"Aufgabe wurde erfolgreich ausgeführt");
			builder.AddOrUpdate("Admin.System.ScheduleTasks.UpdateSuccess",
				"The task has been updated successfully",
				"Die Aufgabe wurde erfolgreich bearbeitet");
		}
    }
}

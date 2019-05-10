namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Domain.Tasks;
    using SmartStore.Data.Setup;

    public partial class ScheduleTaskHistory : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            DropIndex("dbo.ScheduleTask", "IX_LastStart_LastEnd");
            CreateTable(
                "dbo.ScheduleTaskHistory",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ScheduleTaskId = c.Int(nullable: false),
                    IsRunning = c.Boolean(nullable: false),
                    MachineName = c.String(maxLength: 400),
                    StartedOnUtc = c.DateTime(nullable: false),
                    FinishedOnUtc = c.DateTime(),
                    SucceededOnUtc = c.DateTime(),
                    Error = c.String(maxLength: 1000),
                    ProgressPercent = c.Int(),
                    ProgressMessage = c.String(maxLength: 1000),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ScheduleTask", t => t.ScheduleTaskId, cascadeDelete: true)
                .Index(t => t.ScheduleTaskId)
                .Index(t => new { t.MachineName, t.IsRunning })
                .Index(t => new { t.StartedOnUtc, t.FinishedOnUtc }, name: "IX_Started_Finished");

            AddColumn("dbo.ScheduleTask", "RunPerMachine", c => c.Boolean(nullable: false));
            DropColumn("dbo.ScheduleTask", "LastStartUtc");
            DropColumn("dbo.ScheduleTask", "LastEndUtc");
            DropColumn("dbo.ScheduleTask", "LastSuccessUtc");
            DropColumn("dbo.ScheduleTask", "LastError");
            DropColumn("dbo.ScheduleTask", "ProgressPercent");
            DropColumn("dbo.ScheduleTask", "ProgressMessage");
            DropColumn("dbo.ScheduleTask", "RowVersion");
        }

        public override void Down()
        {
            AddColumn("dbo.ScheduleTask", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
            AddColumn("dbo.ScheduleTask", "ProgressMessage", c => c.String(maxLength: 1000));
            AddColumn("dbo.ScheduleTask", "ProgressPercent", c => c.Int());
            AddColumn("dbo.ScheduleTask", "LastError", c => c.String(maxLength: 1000));
            AddColumn("dbo.ScheduleTask", "LastSuccessUtc", c => c.DateTime());
            AddColumn("dbo.ScheduleTask", "LastEndUtc", c => c.DateTime());
            AddColumn("dbo.ScheduleTask", "LastStartUtc", c => c.DateTime());
            DropForeignKey("dbo.ScheduleTaskHistory", "ScheduleTaskId", "dbo.ScheduleTask");
            DropIndex("dbo.ScheduleTaskHistory", "IX_Started_Finished");
            DropIndex("dbo.ScheduleTaskHistory", new[] { "MachineName", "IsRunning" });
            DropIndex("dbo.ScheduleTaskHistory", new[] { "ScheduleTaskId" });
            DropColumn("dbo.ScheduleTask", "RunPerMachine");
            DropTable("dbo.ScheduleTaskHistory");
            CreateIndex("dbo.ScheduleTask", new[] { "LastStartUtc", "LastEndUtc" }, name: "IX_LastStart_LastEnd");
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
            context.SaveChanges();

            // Search index task is the first one to run per machine. Better not create a second plugin migration for these few lines.
            var taskSet = context.Set<ScheduleTask>();
            var indexingTask = taskSet.FirstOrDefault(x => x.Type == "SmartStore.MegaSearch.IndexingTask, SmartStore.MegaSearch");
            if (indexingTask != null)
            {
                indexingTask.RunPerMachine = true;
                context.SaveChanges();
            }
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Common.History", "History", "Historie");
            builder.AddOrUpdate("Common.ExecutedOn", "Executed on", "Ausgeführt am");
            builder.AddOrUpdate("Common.FinishedOn", "Finished on", "Beendet am");
            builder.AddOrUpdate("Common.Status", "Status", "Status");
            builder.AddOrUpdate("Common.Succeeded", "Succeeded", "Erfolgreich");
            builder.AddOrUpdate("Common.MachineName", "Machine name", "Maschinenname");

            builder.AddOrUpdate("Admin.System.ScheduleTasks.HistoryCleanupNote",
                "The history is cleaned up once a day: maximum {0} entries and none older than {1} days.",
                "Die Historie wird einmal täglich bereinigt: Maximal {0} Einträge und keine älter als {1} Tage.");
        }
    }
}

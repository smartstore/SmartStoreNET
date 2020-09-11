namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class DataExchangeEnhancements : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            DropIndex("dbo.ScheduleTaskHistory", new[] { "MachineName", "IsRunning" });
            AddColumn("dbo.ImportProfile", "ImportRelatedData", c => c.Boolean(nullable: false));
            AddColumn("dbo.ExportProfile", "ExportRelatedData", c => c.Boolean(nullable: false));
            AlterColumn("dbo.ScheduleTaskHistory", "MachineName", c => c.String(nullable: false, maxLength: 400));
            CreateIndex("dbo.ScheduleTaskHistory", new[] { "MachineName", "IsRunning" });
        }

        public override void Down()
        {
            DropIndex("dbo.ScheduleTaskHistory", new[] { "MachineName", "IsRunning" });
            AlterColumn("dbo.ScheduleTaskHistory", "MachineName", c => c.String(maxLength: 400));
            DropColumn("dbo.ExportProfile", "ExportRelatedData");
            DropColumn("dbo.ImportProfile", "ImportRelatedData");
            CreateIndex("dbo.ScheduleTaskHistory", new[] { "MachineName", "IsRunning" });
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.DataExchange.Export.ExportRelatedData",
                "Export associated data",
                "Zugehörige Daten exportieren",
                "Specifies whether to also export associated data (e.g. tier prices).",
                "Legt fest, ob auch zugehörige Daten (z.B. Staffelpreise) exportiert werden sollen.");

            builder.AddOrUpdate("Admin.DataExchange.Import.ImportRelatedData",
                "Import associated data",
                "Zugehörige Daten importieren",
                "Specifies whether to also import associated data (e.g. tier prices).",
                "Legt fest, ob auch zugehörige Daten (z.B. Staffelpreise) importiert werden sollen.");
        }
    }
}

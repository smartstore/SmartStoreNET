namespace SmartStore.Data.Migrations
{
	using System;
	using System.Linq;
	using System.Data.Entity.Migrations;
	using Setup;
	using Core.Data;
	using System.Web.Hosting;

	public partial class log4net : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
		public override void Up()
        {
			// Custom START
			if (DataSettings.Current.IsSqlServer)
			{
				//DropIndex("dbo.Log", "IX_Log_ContentHash");
				Sql("IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Log_ContentHash' AND object_id = OBJECT_ID('[dbo].[Log]')) DROP INDEX [IX_Log_ContentHash] ON [dbo].[Log];");
				Sql(@"Truncate Table [Log]");
			}
			// Custom END

			AddColumn("dbo.Log", "Logger", c => c.String(nullable: false, maxLength: 400));
            AddColumn("dbo.Log", "HttpMethod", c => c.String(maxLength: 10));
            AddColumn("dbo.Log", "UserName", c => c.String(maxLength: 100));
            AlterColumn("dbo.Log", "ShortMessage", c => c.String(nullable: false, maxLength: 4000));
            AlterColumn("dbo.Log", "PageUrl", c => c.String(maxLength: 1500));
            AlterColumn("dbo.Log", "ReferrerUrl", c => c.String(maxLength: 1500));
            CreateIndex("dbo.Log", "LogLevelId", name: "IX_Log_Level");
            CreateIndex("dbo.Log", "Logger", name: "IX_Log_Logger");
            DropColumn("dbo.Log", "UpdatedOnUtc");
            DropColumn("dbo.Log", "Frequency");
            DropColumn("dbo.Log", "ContentHash");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Log", "ContentHash", c => c.String(maxLength: 40));
            AddColumn("dbo.Log", "Frequency", c => c.Int(nullable: false));
            AddColumn("dbo.Log", "UpdatedOnUtc", c => c.DateTime());
            DropIndex("dbo.Log", "IX_Log_Logger");
            DropIndex("dbo.Log", "IX_Log_Level");
            AlterColumn("dbo.Log", "ReferrerUrl", c => c.String());
            AlterColumn("dbo.Log", "PageUrl", c => c.String());
            AlterColumn("dbo.Log", "ShortMessage", c => c.String(nullable: false));
            DropColumn("dbo.Log", "UserName");
            DropColumn("dbo.Log", "HttpMethod");
            DropColumn("dbo.Log", "Logger");

			// Custom START
			if (DataSettings.DatabaseIsInstalled())
			{
				CreateIndex("dbo.Log", "ContentHash", name: "IX_Log_ContentHash");
			}
			// Custom END
		}

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			var prefix = "Admin.System.Log.";
			var deleteKeys = (new[] { "Fields.Frequency", "Fields.ContentHash", "Fields.UpdatedOn", "List.MinFrequency" }).Select(x => prefix + x).ToArray();
			deleteKeys = deleteKeys.Concat(deleteKeys.Select(x => x + ".Hint")).ToArray();

			builder.Delete(deleteKeys);

			builder.AddOrUpdate("Admin.System.Log.Fields.Logger",
				"Logger",
				"Logger");
			builder.AddOrUpdate("Admin.System.Log.Fields.HttpMethod",
				"HTTP method",
				"HTTP Methode");
			builder.AddOrUpdate("Admin.System.Log.Fields.UserName",
				"User",
				"Benutzer");
		}
	}
}

namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using System.Linq;
	using Core.Domain;
	using Setup;

	public partial class ExportRevision : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            AlterColumn("dbo.ExportProfile", "FolderName", c => c.String(nullable: false, maxLength: 400));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ExportProfile", "FolderName", c => c.String(nullable: false, maxLength: 100));
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			var rootPath = "~/App_Data/ExportProfiles/";
			var exportProfiles = context.Set<ExportProfile>().ToList();

			foreach (var profile in exportProfiles)
			{
				if (!profile.FolderName.EmptyNull().StartsWith(rootPath))
				{
					profile.FolderName = rootPath + profile.FolderName;
				}
			}

			context.SaveChanges();
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.DataExchange.Export.FolderName",
				"Folder path",
				"Ordnerpfad",
				"Specifies the path of the folder where to export the data.",
				"Legt den Pfad des Ordners fest, in den die Daten exportiert werden.");

		}
	}
}

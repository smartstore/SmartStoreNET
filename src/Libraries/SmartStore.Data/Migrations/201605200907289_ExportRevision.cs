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
				"Specifies the relative path of the folder where to export the data.",
				"Legt den relativen Pfad des Ordners fest, in den die Daten exportiert werden.");

			builder.AddOrUpdate("Admin.DataExchange.Export.FileNamePattern.Validate",
				"Please enter a valid pattern for file names. Example for file names: %Store.Id%-%Profile.Id%-%File.Index%-%Profile.SeoName%",
				"Bitte ein gültiges Muster für Dateinamen eingeben. Beispiel: %Store.Id%-%Profile.Id%-%File.Index%-%Profile.SeoName%");

			builder.AddOrUpdate("Admin.DataExchange.Export.FolderName.Validate",
				"Please enter a valid, relative folder path for the export data.",
				"Bitte einen gültigen, relativen Ordnerpfad für die zu exportierenden Daten eingeben.");

			builder.Delete("Admin.DataExchange.Export.FolderAndFileName.Validate");
		}
	}
}

namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using System.Linq;
	using Core.Domain;
	using Core.Domain.DataExchange;
	using Setup;

	public partial class ExportRevision : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            AddColumn("dbo.ExportDeployment", "SubFolder", c => c.String(maxLength: 400));
            AlterColumn("dbo.ExportProfile", "FolderName", c => c.String(nullable: false, maxLength: 400));
            DropColumn("dbo.ExportDeployment", "CreateZip");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ExportDeployment", "CreateZip", c => c.Boolean(nullable: false));
            AlterColumn("dbo.ExportProfile", "FolderName", c => c.String(nullable: false, maxLength: 100));
            DropColumn("dbo.ExportDeployment", "SubFolder");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			// migrate folder name to folder path
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

			// migrate public file system deployment to new public deployment
			if (context.ColumnExists("ExportDeployment", "IsPublic"))
			{
				var fileSystemDeploymentTypeId = (int)ExportDeploymentType.FileSystem;
				var publicFolderDeploymentTypeId = (int)ExportDeploymentType.PublicFolder;

				context.ExecuteSqlCommand("Update [ExportDeployment] Set DeploymentTypeId = {0} Where DeploymentTypeId = {1} And IsPublic = 1",
					true, null, publicFolderDeploymentTypeId, fileSystemDeploymentTypeId);

				context.ColumnDelete("ExportDeployment", "IsPublic");
			}
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

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDeploymentType.Http", "HTTP POST", "HTTP POST");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDeploymentType.PublicFolder", "Public folder", "Öffentlicher Ordner");

			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.SubFolder",
				"Name of subfolder",
				"Name des Unterordners",
				"Specifies the name of a subfolder where to deploy the data.",
				"Legt den Namen eines Unterordners fest, in den die Daten bereitgestellt werden sollen.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.ZipUsageNote",
				"If there are a large number of export files, it is recommended to use the option <b>Create ZIP archive</b>. This saves time and avoids problems, such as a full email mailbox.",
				"Bei einer großen Anzahl an Exportdateien wird empfohlen die Option <b>ZIP-Archiv erstellen</b> zu benutzen. Das spart Zeit und vermeidet Probleme, wie z.B. ein volles E-Mail Postfach.");

			builder.Delete(
				"Admin.DataExchange.Export.FolderAndFileName.Validate",
				"Admin.DataExchange.Export.Deployment.IsPublic",
				"Admin.DataExchange.Export.Deployment.CreateZip");
		}
	}
}

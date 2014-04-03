namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class Packaging : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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
			context.MigrateLocaleResources(MigrateLocaleResources);
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{

			builder.AddOrUpdate("Admin.Packaging.UploadTheme",
				"Upload theme",
				"Theme hochladen");

			builder.AddOrUpdate("Admin.Packaging.UploadPlugin",
				"Upload plugin",
				"Plugin hochladen");

			builder.AddOrUpdate("Admin.Packaging.Dialog.File",
				"Package file",
				"Paket Datei");

			builder.AddOrUpdate("Admin.Packaging.Dialog.Upload",
				"Upload & Install",
				"Hochladen & installieren");

			builder.AddOrUpdate("Admin.Packaging.Dialog.ThemeInfo",
				"Choose the theme package file (SmartStore.Theme.*.nupkg) to upload to your server. The package will be automatically extracted and installed. If an older version of the theme already exists, it will be backed up for you.",
				"Wählen Sie die Theme Paket-Datei (SmartStore.Theme.*.nupkg), die Sie auf den Server hochladen möchten. Das Paket wird autom. extrahiert und installiert. Wenn eine ältere Version des Themes bereits existiert, wird eine Sicherungskopie davon erstellt.");

			builder.AddOrUpdate("Admin.Packaging.Dialog.PluginInfo",
				"Choose a plugin package file (SmartStore.Plugin.*.nupkg) to upload to your server. The package will be automatically extracted and installed. If an older version of the plugin already exists, it will be backed up for you.",
				"Wählen Sie die Plugin Paket-Datei (SmartStore.Plugin.*.nupkg), die Sie auf den Server hochladen möchten. Das Paket wird autom. extrahiert und installiert. Wenn eine ältere Version des Plugins bereits existiert, wird eine Sicherungskopie davon erstellt.");

			builder.AddOrUpdate("Admin.Packaging.NotAPackage",
				"Package file is invalid. Please upload a 'SmartStore.*.nupkg' file.",
				"Paket-Datei ist ungültig. Bitte laden Sie eine 'SmartStore.*.nupkg' Datei hoch.");

			builder.AddOrUpdate("Admin.Packaging.InstallSuccess",
				"Package was uploaded and installed successfully.",
				"Paket wurde hochgeladen und erfolgreich installiert.");

			builder.AddOrUpdate("Admin.Packaging.StreamError",
				"Unable to create NuGet package from stream.",
				"Stream kann nicht in ein NuGet Paket konvertiert werden.");

			builder.AddOrUpdate("Admin.Packaging.BackupError",
				"Unable to backup existing local package directory.",
				"Kann bereits vorhandenen Paket-Ordner nicht sichern.");

			builder.AddOrUpdate("Admin.Packaging.UninstallError",
				"Unable to un-install local package before updating.",
				"Kann bereits vorhandenes Paket nicht deinstallieren.");

			builder.AddOrUpdate("Admin.Packaging.IsIncompatible",
				"The package is not compatible the current app version {0}. Please update SmartStore.NET or install another version of this package.",
				"Das Paket ist nicht kompatibel mit der aktuallen Programmversion {0}. Bitte aktualisieren Sie SmartStore.NET oder nutzen Sie eine andere, kompatible Paket-Version.");

			builder.AddOrUpdate("Admin.Packaging.NotFound",
				"Package not found: {0}",
				"Paket nicht gefunden: {0}");

			builder.AddOrUpdate("Admin.Packaging.TooManyBackups",
				"Backup folder {0} has too many backups subfolder (limit is 1.000)",
				"Sicherungsordner {0} enthält zu viele Unterordner (Obergrenze ist 1.000)");

			builder.AddOrUpdate("Admin.Packaging.RestoreSuccess",
				"Successfully restored local package to local folder '{0}'.",
				"Lokales Paket im Ordner '{0}' erfolgreich wiederhergestellt.");

			builder.AddOrUpdate("Admin.Packaging.BackupSuccess",
				"Successfully backed up local package to local folder '{0}'",
				"Lokales Paket im Ordner '{0}' erfolgreich gesichert.");

		}
    }
}

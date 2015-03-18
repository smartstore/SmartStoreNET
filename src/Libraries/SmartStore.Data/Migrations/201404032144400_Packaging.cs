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
			context.MigrateSettings(x =>
			{
				x.Add("ShoppingCartSettings.ShowDeliveryTimes", true);
				x.Add("ShoppingCartSettings.ShowShortDesc", true);
				x.Add("ShoppingCartSettings.ShowBasePrice", false);
			});
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

			builder.AddOrUpdate("Admin.Catalog.Products.Fields.IsShipEnabled")
				.Value("de", "Versand erforderlich");



			builder.Delete("Admin.Configuration.Plugins.Description.Step5");

			builder.AddOrUpdate("Admin.Configuration.Plugins.Description",
				"Install or update plugins",
				"Plugins installieren oder aktualisieren");

			builder.AddOrUpdate("Admin.Configuration.Plugins.Description.Step1",
				"Use the <a id='{0}' href='{1}' data-toggle='modal'>package uploader</a> or upload the plugin manually - eg. via FTP - to the <i>/Plugins</i> folder in your SmartStore.NET directory.",
				"Verwenden Sie den <a id='{0}' href='{1}' data-toggle='modal'>Paket Uploader</a> oder laden Sie das Plugin manuell - bspw. per FTP - in den <i>/Plugins</i> Ordner hoch.");

			builder.AddOrUpdate("Admin.Configuration.Plugins.Description.Step2",
				"With manual uploads: restart your application (or click <i>Reload list of plugins</i> button).",
				"Bei manuellem Upload: starten Sie die Anwendung neu (oder klicken Sie <i>Plugin-Liste erneut laden</i>).");

			builder.AddOrUpdate("Admin.Configuration.Plugins.Description.Step3",
				"Scroll down the list to find the newly installed plugin.",
				"Scrollen Sie runter, um die neu installierten Plugins zu sehen.");

			builder.AddOrUpdate("Admin.Configuration.Plugins.Description.Step4",
				"Click the <i>Install</i> button to install the plugin.",
				"Klicken Sie <i>Installieren</i>, um das Plugin zu installieren.");



			builder.AddOrUpdate("Products.Bundle.BundleIncludes")
				.Value("Bundle includes")
				.Value("de", "Besteht aus");

			builder.AddOrUpdate("Basket.Bundle.BundleIncludes")
				.Value("Bundle includes")
				.Value("de", "Besteht aus");

			builder.AddOrUpdate("Products.Bundle.YouSave")
				.Value("Bundle for just {0} instead of {1}")
				.Value("de", "Im Set für nur {0} statt {1}");


		}
    }
}

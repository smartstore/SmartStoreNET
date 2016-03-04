namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class ExportFramework2 : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.ExportProfile", "ResultInfo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExportProfile", "ResultInfo");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			context.Execute("DELETE FROM [dbo].[ScheduleTask] WHERE [Type] = 'SmartStore.GoogleMerchantCenter.StaticFileGenerationTask, SmartStore.GoogleMerchantCenter'");
			context.Execute("DELETE FROM [dbo].[ScheduleTask] WHERE [Type] = 'SmartStore.BMEcat.StaticFileGenerationTask, SmartStore.BMEcat'");

			context.MigrateSettings(x =>
			{
				x.DeleteGroup("FroogleSettings");
				x.DeleteGroup("BMEcatExportSettings");
				x.DeleteGroup("OpenTransSettings");
			});
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Common.Example", "Example", "Beispiel");
			builder.AddOrUpdate("Common.ShowAll", "Show all", "Alle anzeigen");
			builder.AddOrUpdate("Admin.Common.Selected", "Selected", "Ausgewählte");
			builder.AddOrUpdate("Admin.Common.Entity", "Object", "Objekt");
			builder.AddOrUpdate("Admin.Common.Placeholder", "Placeholder", "Platzhalter");


			builder.AddOrUpdate("Admin.Common.FilesDeleted",
				"{0} files were deleted",
				"{0} Dateien wurden gelöscht");

			builder.AddOrUpdate("Admin.Common.FoldersDeleted",
				"{0} folders were deleted",
				"{0} Verzeichnisse wurden gelöscht");

			builder.AddOrUpdate("Admin.Common.ProviderNotLoaded",
				"Cannot load the provider {0}.",
				"Der Provider {0} konnte nicht geladen werden.");

			builder.AddOrUpdate("Admin.Common.NoEntriesSelected",
				"No entries have been selected.",
				"Es wurden keine Einträge ausgewählt.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Blog.ShowHeaderRSSUrl.Hint",
				"Check to enable the blog RSS feed link in customers browser address bar.",
				"Legt fest, ob der RSS-Feed-Link in der Adressleiste des Browsers angezeigt werden soll.");


			builder.AddOrUpdate("Admin.System.SeNames",	"SEO Names", "SEO Namen");
			builder.Delete("Admin.System.SeNames.DeleteSelected");

			builder.AddOrUpdate("Admin.System.SeNames.Name",
				"SEO Name",
				"SEO Name",
				"Specifies the SEO name.",
				"Legt den SEO Namen fest.");

			builder.AddOrUpdate("Admin.System.SeNames.EntityId",
				"Object ID",
				"Objekt-ID",
				"Specifies the ID of the associated object.",
				"Legt die ID des zugehörigen Objektes fest.");

			builder.AddOrUpdate("Admin.System.SeNames.EntityName",
				"Object",
				"Objekt",
				"Specifies the name of the associated object.",
				"Legt den Namen der zugehörigen Objektes fest.");

			builder.AddOrUpdate("Admin.System.SeNames.IsActive",
				"Is active",
				"Ist aktiv",
				"Specifies whether the SEO name is active or inactive.",
				"Legt fest, ob der SEO Name aktiv oder inaktiv ist.");

			builder.AddOrUpdate("Admin.System.SeNames.Language",
				"Language",
				"Sprache",
				"Specifies the language of the SEO name.",
				"Legt die Sprache des SEO Namens fest.");

			builder.AddOrUpdate("Admin.System.SeNames.SlugsPerEntity",
				"Names per object",
				"Namen pro Objekt",
				"The number of SEO names per object.",
				"Die Anzahl der SEO Namen pro Objekt.");

			builder.AddOrUpdate("Admin.System.SeNames.ActiveSlugAlreadyExist",
				"Only one active SEO name should be set per language.",
				"Pro Sprache darf nur ein aktiver SEO Name festgelegt werden.");


			builder.AddOrUpdate("Admin.DataExchange.Export.FileNamePatternDescriptions",
				"ID of export profil;Folder name of export profil;SEO name of export profil;Store ID;SEO name of store;One based file index;Random number;UTC timestamp",
				"ID des Exportprofils;Ordername des Exportprofils;SEO Name des Exportprofils;Shop ID;SEO Name des Shops;Mit 1 beginnender Dateiindex;Zufallszahl;UTC Zeitstempel");

			builder.AddOrUpdate("Admin.DataExchange.Export.NotPreviewCompatible",
				"This option is not taken into account in the preview.",
				"Diese Option wird in der Vorschau nicht berücksichtigt.");

			builder.AddOrUpdate("Admin.DataExchange.Export.CloneProfile",
				"Apply settings from",
				"Einstellungen übernehmen von",
				"Specifies an export profile from which to apply the settings.",
				"Legt das Exportprofil fest, von welchem die Einstellungen übernommen werden sollen.");

			builder.AddOrUpdate("Admin.DataExchange.Export.NonFileBasedExport.Note",
				"The export provider does not explicit support any file type. Therefore, the export provider is responsible for futher deployment of export data.",
				"Der Export-Provider unterstützt keinen expliziten Dateityp. Für eine weitere Bereitstellung der Exportdaten ist daher der Export-Provider verantwortlich.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Projection.NoGroupedProducts",
				"Do not export grouped products",
				"Keine Gruppenprodukte exportieren",
				"Specifies whether to export grouped products. If this option is activated, then the associated products will be exported.",
				"Legt fest, ob Gruppenprodukte exportiert werden sollen. Ist diese Option aktiviert, so werden die zur Gruppe gehörenden Produkte exportiert.");

			builder.AddOrUpdate("Admin.DataExchange.Export.NoFiltering",
				"There is no filtering available.",
				"Möglichkeiten der Filterung stehen nicht zur Verfügung.");

			builder.AddOrUpdate("Admin.DataExchange.Export.NoProjection",
				"There is no projection available.",
				"Möglichkeiten der Projektion stehen nicht zur Verfügung.");

			builder.AddOrUpdate("Admin.DataExchange.Export.NoPreview",
				"There is no preview available for this entity type.",
				"Eine Vorschau steht für diesen Entitätstyp nicht zur Verfügung.");


			builder.Delete(
				"Plugins.Feed.Froogle.TaskEnabled",
				"Plugins.Feed.Froogle.TaskEnabled.Hint",
				"Plugins.Feed.Froogle.StaticFileUrl",
				"Plugins.Feed.Froogle.StaticFileUrl.Hint",
				"Plugins.Feed.Froogle.GenerateStaticFileEachMinutes",
				"Plugins.Feed.Froogle.GenerateStaticFileEachMinutes.Hint",
				"Plugins.Feed.Froogle.Currency",
				"Plugins.Feed.Froogle.Currency.Hint",
				"Plugins.Feed.Froogle.ProductPictureSize",
				"Plugins.Feed.Froogle.ProductPictureSize.Hint",
				"Plugins.Feed.Froogle.AppendDescriptionText",
				"Plugins.Feed.Froogle.AppendDescriptionText.Hint",
				"Plugins.Feed.Froogle.BuildDescription",
				"Plugins.Feed.Froogle.BuildDescription.Hint",
				"Plugins.Feed.Froogle.Automatic",
				"Plugins.Feed.Froogle.DescShort",
				"Plugins.Feed.Froogle.DescLong",
				"Plugins.Feed.Froogle.DescTitleAndShort",
				"Plugins.Feed.Froogle.DescTitleAndLong",
				"Plugins.Feed.Froogle.DescManuAndTitleAndShort",
				"Plugins.Feed.Froogle.DescManuAndTitleAndLong",
				"Plugins.Feed.Froogle.UseOwnProductNo",
				"Plugins.Feed.Froogle.UseOwnProductNo.Hint",
				"Plugins.Feed.Froogle.DescriptionToPlainText",
				"Plugins.Feed.Froogle.DescriptionToPlainText.Hint",
				"Plugins.Feed.Froogle.Brand",
				"Plugins.Feed.Froogle.Brand.Hint",
				"Plugins.Feed.Froogle.Store",
				"Plugins.Feed.Froogle.Store.Hint",
				"Plugins.Feed.Froogle.ConvertNetToGrossPrices",
				"Plugins.Feed.Froogle.ConvertNetToGrossPrices.Hint",
				"Plugins.Feed.Froogle.LanguageId",
				"Plugins.Feed.Froogle.LanguageId.Hint",
				"Plugins.Feed.Froogle.Generate",
				"Plugins.Feed.Froogle.ConfigSaveNote",
				"Plugins.Feed.Froogle.AvailabilityAvailableForOrder",
				"Plugins.Feed.Froogle.GridEditNote",
				"Plugins.Feed.Froogle.General",
				"Plugins.Feed.Froogle.ProductData"
			);

			builder.Delete(
				"Plugins.Feed.BMEcat.TaskEnabled",
				"Plugins.Feed.BMEcat.TaskEnabled.Hint",
				"Plugins.Feed.BMEcat.StaticFileUrl",
				"Plugins.Feed.BMEcat.StaticFileUrl.Hint",
				"Plugins.Feed.BMEcat.GenerateStaticFileEachMinutes",
				"Plugins.Feed.BMEcat.GenerateStaticFileEachMinutes.Hint",
				"Plugins.Feed.BMEcat.UseOwnProductNo",
				"Plugins.Feed.BMEcat.UseOwnProductNo.Hint",
				"Plugins.Feed.BMEcat.ShippingCostAustria",
				"Plugins.Feed.BMEcat.ShippingCostAustria.Hint",
				"Plugins.Feed.BMEcat.Currency",
				"Plugins.Feed.BMEcat.Currency.Hint",
				"Plugins.Feed.BMEcat.ProductPictureSize",
				"Plugins.Feed.BMEcat.ProductPictureSize.Hint",
				"Plugins.Feed.BMEcat.AppendDescriptionText",
				"Plugins.Feed.BMEcat.AppendDescriptionText.Hint",
				"Plugins.Feed.BMEcat.BuildDescription",
				"Plugins.Feed.BMEcat.BuildDescription.Hint",
				"Plugins.Feed.BMEcat.Automatic",
				"Plugins.Feed.BMEcat.DescShort",
				"Plugins.Feed.BMEcat.DescLong",
				"Plugins.Feed.BMEcat.DescTitleAndShort",
				"Plugins.Feed.BMEcat.DescTitleAndLong",
				"Plugins.Feed.BMEcat.DescManuAndTitleAndShort",
				"Plugins.Feed.BMEcat.DescManuAndTitleAndLong",
				"Plugins.Feed.BMEcat.UseOwnProductNo",
				"Plugins.Feed.BMEcat.UseOwnProductNo.Hint",
				"Plugins.Feed.BMEcat.DescriptionToPlainText",
				"Plugins.Feed.BMEcat.DescriptionToPlainText.Hint",
				"Plugins.Feed.BMEcat.ShippingCost",
				"Plugins.Feed.BMEcat.ShippingCost.Hint",
				"Plugins.Feed.BMEcat.ShippingTime",
				"Plugins.Feed.BMEcat.ShippingTime.Hint",
				"Plugins.Feed.BMEcat.Brand",
				"Plugins.Feed.BMEcat.Brand.Hint",
				"Plugins.Feed.BMEcat.Store",
				"Plugins.Feed.BMEcat.Store.Hint",
				"Plugins.Feed.BMEcat.ConvertNetToGrossPrices",
				"Plugins.Feed.BMEcat.ConvertNetToGrossPrices.Hint",
				"Plugins.Feed.BMEcat.LanguageId",
				"Plugins.Feed.BMEcat.LanguageId.Hint",
				"Plugins.Feed.BMEcat.Generate",
				"Plugins.Feed.BMEcat.ConfigSaveNote"
			);

			builder.Delete("Plugins.Widgets.OpenTrans.IsLexwareCompatibe");
			builder.Delete("Admin.System.Maintenance.DeleteExportedFolders.TotalDeleted");

			// Common
			builder.AddOrUpdate("StoreClosed",
				"We'll be back.",
				"Wir sind bald wieder da.");
			builder.AddOrUpdate("StoreClosed.Hint",
				"We're busy updating our online store for you and will be back soon.",
				"Wir aktualisieren gerade das Angebot in unserem Online-Shop. Die Seite ist demnächst wieder verfügbar.");

			builder.AddOrUpdate("Admin.System.SystemInfo.UsedMemorySize",
				"Used memory (RAM)",
				"Benutzter Speicher (RAM)");
			builder.AddOrUpdate("Admin.System.SystemInfo.GarbageCollect",
				"Collect",
				"Aufräumen");
			builder.AddOrUpdate("Admin.System.SystemInfo.GarbageCollectSuccessful",
				"The memory has been successfully cleaned up.",
				"Der Arbeitsspeicher wurde erfolgreich aufgeräumt.");

			builder.AddOrUpdate("Admin.Configuration.Themes.NoConfigurationRequired",
				"Theme requires no configuration",
				"Theme benötigt keine Konfiguration");


			builder.AddOrUpdate("Tax.LegalInfoFooter2",
				"* All prices {0}, plus shipping",
				"* Alle Preise {0}, zzgl. Versandkosten");

			builder.AddOrUpdate("Tax.LegalInfoProductDetail2",
				"{0} {1} {2}plus shipping",
				"{0} {1} {2} zzgl. Versandkosten");

			builder.AddOrUpdate("ShoppingCart.ShippingInfoLink",
				"For a complete listing of all shipping costs please click <a href=\"{0}\">here</a>.",
				"Eine vollständige Liste aller Versandkosten finden Sie <a href=\"{0}\">hier</a>.");
		}
    }
}

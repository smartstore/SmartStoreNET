namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class AboutPage : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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
			builder.AddOrUpdate("Admin.Common.TaskSuccessfullyProcessed",
				"The task was successfully processed.",
				"Der Vorgang wurde erfolgreich ausgeführt.");

			builder.AddOrUpdate("Common.Description.Hint",
				"Description",
				"Beschreibung");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.HideBuyButtonInLists",
				"Hide buy-button in product lists",
				"Verberge Kaufen-Button in Produktlisten",
				"Check to hide the buy-button in product lists.",
				"Legt fest, ob der Kaufen-Button in Produktlisten ausgeblendet werden soll.");

			builder.AddOrUpdate("Admin.Common.About",
				"About",
				"Über");

			builder.AddOrUpdate("Admin.Common.License",
				"License",
				"Lizenz");

			builder.AddOrUpdate("Admin.Help.NopCommerceNote",
				"SmartStore.NET is a derivation of the ASP.NET open source e-commerce solution {0}.",
				"SmartStore.NET ist ein Derivat der ASP.NET Open-Source E-Commerce-Lösung {0}.");

			builder.AddOrUpdate("Admin.Help.OtherWorkNote",
				"SmartStore.NET includes works distributed under the licenses listed below. Please refer to the specific resources for more detailed information about the authors, copyright notices and licenses.",
				"SmartStore.NET beinhaltet Werke, die unter den unten aufgeführten Lizenzen vertrieben werden. Bitte beachten Sie die betreffenden Ressourcen für ausführlichere Informationen über Autoren, Copyright-Vermerke und Lizenzen.");
		}
	}
}

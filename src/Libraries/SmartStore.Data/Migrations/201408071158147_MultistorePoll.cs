namespace SmartStore.Data.Migrations
{
	using System;
	using System.Linq;
	using System.Data.Entity.Migrations;
	using SmartStore.Core.Domain.Stores;
	using SmartStore.Data.Setup;

	public partial class MultistorePoll : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
			AddColumn("dbo.Poll", "LimitedToStores", c => c.Boolean(nullable: false));
			AddColumn("dbo.NewsLetterSubscription", "StoreId", c => c.Int(nullable: false));
			AddColumn("dbo.Campaign", "LimitedToStores", c => c.Boolean(nullable: false));

			CreateIndex("NewsLetterSubscription", "Email", false, "IX_NewsLetterSubscription_Email", false);
        }
        
        public override void Down()
        {
			DropIndex("NewsLetterSubscription", "IX_NewsLetterSubscription_Email");

			DropColumn("dbo.Campaign", "LimitedToStores");
			DropColumn("dbo.NewsLetterSubscription", "StoreId");
			DropColumn("dbo.Poll", "LimitedToStores");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			int storeId = context.SqlQuery<int>("Select Top 1 Id From Store").FirstOrDefault();
			context.Execute("Update [dbo].[NewsLetterSubscription] Set StoreId = {0} Where StoreId = 0".FormatWith(storeId));
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Promotions.NewsLetterSubscriptions.ImportEmailsSuccess",
				"{0} email(s) were imported and {1} updated.",
				"Es wurden {0} E-Mail(s) importiert und {1} aktualisiert.");


			builder.AddOrUpdate("Admin.Catalog.Products.Fields.ProductUrl",
				"URL of the product page",
				"URL der Produktseite",
				"The URL of the product page.",
				"Die URL der Produktseite.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.EnableHtmlTextCollapser",
				"Truncate long texts",
				"Langtexte kürzen",
				"Option to truncate long texts and to only show on click in full length.",
				"Option, bei der Langtexte gekürzt und erst auf Klick in voller Länge angezeigt werden.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.HtmlTextCollapsedHeight",
				"Height of truncated long text",
				"Höhe des gekürzten Langtextes",
				"Determines the height of the truncated long text.",
				"Legt die Höhe (in Pixel) des gekürzten Langtextes fest.");


			builder.AddOrUpdate("Admin.System.SystemInfo.DataProviderFriendlyName",
				"Data provider",
				"Daten-Provider",
				"The name of the data provider.",
				"Der Name des Daten-Providers.");

			builder.AddOrUpdate("Admin.System.SystemInfo.AppDate",
				"Created on",
				"Erstellt am",
				"The creation date of this SmartStore.NET version.",
				"Das Erstellungsdatum dieser SmartStore.NET Version.");

			builder.AddOrUpdate("Admin.Orders.Fields.OrderWeight",
				"The total weight of this order",
				"Das Gesamtgewicht des Auftrags",
				"The total weight of this order.",
				"Das Gesamtgewicht des Auftrags.");
		}
    }
}

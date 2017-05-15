namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class V21Final : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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
			// New stuff from MH
			builder.AddOrUpdate("Products.CallForPrice.GoToForm",
				"Ask for price",
				"Preis anfragen");
			builder.AddOrUpdate("ThemeVar.Alpha.DisplayNavbar",
				"Display Mega Menu",
				"Zeige Mega Menu");

			// Tax & VAT stuff
			builder.Delete("ShoppingCart.Totals.TaxRateLine");
			builder.AddOrUpdate("ShoppingCart.Totals.SubTotal",
				"Subtotal",
				"Zwischensumme");
			builder.AddOrUpdate("ShoppingCart.Totals.Tax",
				"Tax",
				"MwSt");
			builder.AddOrUpdate("ShoppingCart.Totals.TaxRateLineIncl",
				"Incl. {0} % Tax",
				"inkl. {0} % MwSt");
			builder.AddOrUpdate("ShoppingCart.Totals.TaxRateLineExcl",
				"Plus {0} % Tax",
				"zzgl. {0} % MwSt");
			builder.AddOrUpdate("Admin.Catalog.Products.Fields.IsEsd",
				"Is Electronic Service",
				"Ist elektronische Leistung",
				"Specifies whether the product is an electronic service bound to EU VAT regulations for digital goods (2008/8/EG directive)",
				"Legt fest, ob das Produkt elektronisch vertrieben wird und daher gemäß EU Richtlinie 2008/8/EG versteuert werden muss.");

			// Some german fixes
			builder.AddOrUpdate("Products.ProductHasBeenAddedToTheWishlist").Value("de", "Das Produkt wurde Ihrer Wunschliste hinzugefügt");
			builder.AddOrUpdate("Products.ProductHasBeenAddedToTheWishlist.Link").Value("de", "Das Produkt wurde Ihrer <a href='{0}'>Wunschliste</a> hinzugefügt");
			builder.AddOrUpdate("Products.ProductHasBeenAddedToTheCart").Value("de", "Das Produkt wurde Ihrem Warenkorb hinzugefügt");
			builder.AddOrUpdate("Products.ProductHasBeenAddedToTheCart.Link").Value("de", "Das Produkt wurde Ihrem <a href='{0}'>Warenkorb</a> hinzugefügt");
			builder.AddOrUpdate("Products.ProductNotAddedToTheCart.Link").Value("de", "Produkt konnte Ihrem Warenkorb nicht hinzugefügt werden.");
			builder.AddOrUpdate("Products.RecentlyViewedProducts").Value("de", "Zuletzt angesehen");

			builder.AddOrUpdate("Admin.Catalog.Attributes.CheckoutAttributes.Deleted").Value("de", "Das Attribut wurde erfolgreich gelöscht.");

			// Theme inheritance
			builder.AddOrUpdate("Admin.Configuration.Themes.IsBasedOn",
				"Based on",
				"Basiert auf");
			builder.AddOrUpdate("Admin.Configuration.Themes.MissingBaseTheme",
				"Error: Base theme '{0}' not found",
				"Fehler: Basis-Theme '{0}' nicht gefunden");
			builder.AddOrUpdate("Admin.Configuration.Themes.Reload",
				"Reload themes",
				"Themes aktualisieren");

			// Theme Preview
			builder.AddOrUpdate("Admin.Configuration.Themes.Preview",
				"Preview",
				"Vorschau");
			builder.AddOrUpdate("Admin.Configuration.Themes.Theme",
				"Theme",
				"Theme");
			builder.AddOrUpdate("Admin.Configuration.Themes.PreviewMode",
				"Preview Mode",
				"Vorschaumodus");
			builder.AddOrUpdate("Admin.Configuration.Themes.ExitPreviewMode",
				"Exit preview mode",
				"Vorschau beenden");

			builder.AddOrUpdate("Common.Apply", "Apply", "Übernehmen");

			builder.AddOrUpdate("Common.Reload", "Reload", "Neu laden");
			builder.AddOrUpdate("Common.Refresh", "Refresh", "Aktualisieren");

			builder.AddOrUpdate("Admin.Orders.PdfInvoice", "Order as PDF", "Auftrag als PDF");
			builder.AddOrUpdate("Order.GetPDFInvoice", "Order as PDF", "Auftrag als PDF");

			builder.AddOrUpdate("Admin.Orders.Fields.Affiliate", "Affiliate", "Partner");
			builder.AddOrUpdate("Admin.Customers.Customers.Fields.Affiliate", "Affiliate", "Partner");

			builder.AddOrUpdate("Admin.Configuration.DeliveryTime.BackToList", "Back to delivery times list", "Zurück zur Lieferzeitenliste");

			builder.AddOrUpdate("Admin.Configuration.Measures.Weights.Description",
				"NOTE: if you change your primary weight, then do not forget to update the appropriate ratios of the units.",
				"Achtung: Wenn die Standardgewichtseinheit geändert wird, müssen auch die zugehörigen Umrechnungseinheiten (Verhältnis) angepasst werden.");

			builder.AddOrUpdate("Admin.System.ScheduleTasks.Seconds.Positive",
				"Seconds should be positive.",
				"Sekunden müssen größer als 0 sein.");

			builder.Delete("Admin.Affiliates.Customers.Name");

			// Avatars
			builder.AddOrUpdate("Account.Avatar.MaximumUploadedFileSize",
				"Maximum avatar size is {0}",
				"Die maximale Größe des Avatars beträgt {0}");
			builder.AddOrUpdate("Account.Avatar.UploadRules",
				"Avatar must be in GIF, PNG or JPG format with the maximum size of {0}",
				"Ein Avatar muss im GIF-, PNG- oder JPG-Format vorliegen und darf {0} nicht überschreiten.");
			
			// Misc
			builder.AddOrUpdate("Admin.Catalog.Attributes.AttributeControlType",
				"Control type",
				"Typ");
			builder.AddOrUpdate("Admin.Catalog.Attributes.AttributeControlType.Hint",
				"Choose how to display your attribute values.",
				"Bestimmt den Steuerelement-Typen für die Erfassung der Attribut-Werte");
			builder.AddOrUpdate("Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Fields.Sku").Value("#");
			builder.AddOrUpdate("Admin.Catalog.Products.Fields.SeName").Value("en", "URL Alias");
			builder.AddOrUpdate("Admin.Catalog.Manufacturers.Fields.SeName").Value("en", "URL alias");
			builder.AddOrUpdate("Admin.Catalog.Categories.Fields.SeName").Value("en", "URL alias");


			// Marketplace
			builder.AddOrUpdate("Admin.Marketplace",
				"Marketplace",
				"Marketplace");
			builder.AddOrUpdate("Admin.Marketplace.News",
				"Marketplace News",
				"Marketplace News");
			builder.AddOrUpdate("Admin.Marketplace.ComingSoon",
				"In the SmartStore.NET Marketplace we offer modules, themes & language packages, which will make your shop better and more successful. Once we are ready to go, you'll be informed about the latest extensions here. Stay tuned...",
				"Im SmartStore.NET Marketplace werden Module, Themes & Sprachpakete angeboten, die Ihren Onlineshop besser, flexibler und erfolgreicher machen sollen. Sobald wir die Arbeiten am Marketplace abgeschlossen haben, werden Sie hier über die neuesten Erweiterungen informiert.");
			builder.AddOrUpdate("Admin.Marketplace.Visit",
				"Visit Marketplace",
				"Zum Marketplace");
		}


	}
}

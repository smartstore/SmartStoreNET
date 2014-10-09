namespace SmartStore.Data.Migrations
{
    using System;
	using System.Linq;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class Providers : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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
			builder.AddOrUpdate("Providers.FriendlyName.CurrencyExchange.ECB",
				"ECB exchange rate provider",
				"EZB-Wechselkursdienst");

			builder.AddOrUpdate("Providers.CurrencyExchange.ECB.SetCurrencyToEURO",
				"You can use ECB (European central bank) exchange rate provider only when exchange rate currency code is set to EURO",
				"Der EZB-Wechselkursdienst kann nur genutzt werden, wenn der Wechselkurs-Währungscode auf EUR gesetzt ist.");

			builder.AddOrUpdate("Providers.FriendlyName.Tax.Free",
				"Free tax rate provider",
				"Steuerbefreit");

			builder.AddOrUpdate("Providers.FriendlyName.CurrencyExchange.MoneyConverter",
				"Money converter exchange rate provider",
				"Money Converter Wechselkursdienst");

			var knownGroups = new string[] { "admin", "analytics", "api", "cms", "currencyexchange", "developer", "discountrequirement", "externalauth", "import", "marketing", "misc", "mobile", "payment", "promotionfeed", "security", "seo", "shipping", "social", "tax", "widget" };
			builder.Delete(knownGroups.Select(x => "plugins.knowngroup." + x).ToArray());

			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Admin", "Administration", "Administration");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Marketing", "Marketing", "Marketing");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Payment", "Payment & Gateways", "Zahlungsschnittstellen");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Shipping", "Shipping & Logistics", "Versand & Logistik");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Tax", "Tax", "Steuern");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Analytics", "Analytics & Stats", "Analyse & Statistiken");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.CMS", "Content Management", "Content Management");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Media", "Media", "Medien");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.SEO", "SEO", "SEO");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Data", "Data", "Daten");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Globalization", "Globalization", "Globalisierung");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Api", "API", "API");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Mobile", "Mobile", "Mobile");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Social", "Social", "Social");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Security", "Security", "Sicherheit");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Developer", "Developer", "Entwickler");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Sales", "Sales", "Vertrieb");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Design", "Design", "Design");
			builder.AddOrUpdate("Admin.Plugins.KnownGroup.Misc", "Miscellaneous", "Sonstige");

			builder.AddOrUpdate("Admin.Providers.ProvidingPlugin", "Providing plugin", "Bereitstellendes Plugin");

			builder.AddOrUpdate("Admin.PromotionFeeds", "Promotion feeds", "Promotion Feeds");

			// some admin menu renaming / new entries
			builder.AddOrUpdate("Admin.Configuration.Tax.Providers.Fields.MarkAsPrimaryProvider").Value("de", "Als Standard festlegen");
			builder.AddOrUpdate("Admin.Common.Activate").Value("de", "Aktivieren");
			builder.AddOrUpdate("Admin.Configuration.ActivityLog.ActivityLog").Value("de", "Aktivitätslog");
			builder.AddOrUpdate("Admin.Configuration.ActivityLog.ActivityLogTy pe").Value("de", "Aktivitätstypen");
			builder.AddOrUpdate("Admin.Configuration.RegionalSettings",
				"Regional Settings",
				"Regionale Einstellungen");
			builder.AddOrUpdate("Admin.Configuration.Lists",
				"Lists",
				"Listen");

			// new product-edit tab
			builder.AddOrUpdate("Admin.Catalog.Products.Inventory",
				"Inventory",
				"Inventar");
			builder.AddOrUpdate("Admin.SaveBeforeEdit",
				"In order to proceed, the record must be saved first.",
				"Um fortfahren zu können, muss der Datensatz zunächst gespeichert werden.");

			// Twisted german resources for tax number and vat id
			builder.AddOrUpdate("PDFInvoice.TaxNumber").Value("de", "Steuer-Nr.:");
			builder.AddOrUpdate("PDFInvoice.VatId").Value("de", "Ust-Id:");

			builder.AddOrUpdate("Account.Login.NewCustomerText").Value("en", "As a registered customer you will be able to shop faster, be up to date on an orders status, and keep track of the orders you have previously made.");

			// Filtering
			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.FilterEnabled")
				.Value("Activate filter")
				.Value("de", "Filterfunktion aktivieren");
			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.FilterEnabled.Hint")
				.Value("Activates the filter function for products within categories")
				.Value("de", "Aktiviert die Filterfunktion für Produkte in Warengruppen.");

			// Misc
			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.CartSettings")
				.Value("Shopping cart")
				.Value("de", "Warenkorb");
			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.WishlistSettings")
				.Value("Wishlist")
				.Value("de", "Wunschzettel");
			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.RoundPricesDuringCalculation")
				.Value("Round prices during calculation")
				.Value("de", "Preise bei der Berechnung runden");
			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.RoundPricesDuringCalculation.Hint")
				.Value("Determines whether the shop calculates with rounded price values (recommended for B2B)")
				.Value("de", "Bestimmt, ob der Shop bei Berechnungen gerundete Werte der Preise benutzt (empfohlen für B2B)");
			builder.AddOrUpdate("ShoppingCart.Weight")
				.Value("Weight")
				.Value("de", "Gewicht");
			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ShowWeight")
				.Value("Show product weight in shopping cart")
				.Value("de", "Zeige Produktgewicht im Warenkorb");
			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ShowWeight.Hint")
				.Value("Determines whether the product weight is shown in shopping cart")
				.Value("de", "Legt fest ob das Produktgewicht im Warenkorb angezeigt wird.");
			builder.AddOrUpdate("Plugins.ExternalAuth.Facebook.Login")
				.Value("Sign in with Facebook")
				.Value("de", "Mit Facebook anmelden");
			builder.AddOrUpdate("Plugins.ExternalAuth.Twitter.Login")
				.Value("Sign in with Twitter")
				.Value("de", "Mit Twitter anmelden");


			// Adding some new common provider resources & removing obsolete ones
			builder.AddOrUpdate("Common.SystemName").Value("System name").Value("de", "Systemname");
			builder.AddOrUpdate("Common.DisplayOrder").Value("Display order").Value("de", "Reihenfolge");
			builder.AddOrUpdate("Admin.Common.Deactivate").Value("Deactivate").Value("de", "Deaktivieren");
			builder.Delete("Admin.Configuration.Plugins.Fields.IsEnabled");

			// Tax providers
			string prefix = "Admin.Configuration.Tax.Providers.";
			builder.Delete(prefix + "Configure", prefix + "Fields", prefix + "Fields.FriendlyName", prefix + "Fields.SystemName");
			// Shipping methods
			prefix = "Admin.Configuration.Shipping.Providers.";
			builder.Delete(prefix + "DisplayOrder", prefix + "Fields.FriendlyName", prefix + "Fields.SystemName", prefix + "IsActive", prefix + "Fields.IsActive");
			// Widgets
			prefix = "Admin.ContentManagement.Widgets.";
			builder.Delete(prefix + "BackToList", prefix + "Configure", prefix + "Fields", prefix + "Fields.FriendlyName", prefix + "Fields.SystemName", prefix + "Fields.IsActive", prefix + "Fields.DisplayOrder");
			// ExternalAuth
			prefix = "Admin.Configuration.ExternalAuthenticationMethods.";
			builder.Delete(prefix + "Fields.DisplayOrder", prefix + "Fields.FriendlyName", prefix + "Fields.SystemName", prefix + "Fields.IsActive");
			// Payment
			prefix = "Admin.Configuration.Payment.Methods.";
			builder.Delete(prefix + "Fields.DisplayOrder", prefix + "Fields.FriendlyName", prefix + "Fields.SystemName", prefix + "Fields.IsActive");
		}
    }
}

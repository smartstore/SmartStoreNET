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

			builder.AddOrUpdate("Providers.Tax.Free.FriendlyName",
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

			builder.AddOrUpdate("Admin.PromotionFeeds", "Promotion feeds", "Promotion Feeds");

			// some admin menu renaming / new entries
			builder.AddOrUpdate("Admin.Configuration.ActivityLog.ActivityLog").Value("de", "Aktivitätslog");
			builder.AddOrUpdate("Admin.Configuration.ActivityLog.ActivityLogTy pe").Value("de", "Aktivitätstypen");
			builder.AddOrUpdate("Admin.Configuration.RegionalSettings",
				"Regional Settings",
				"Regionale Einstellungen");
			builder.AddOrUpdate("Admin.Configuration.Lists",
				"Lists",
				"Listen");

		}
    }
}

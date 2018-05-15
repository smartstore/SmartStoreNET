namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;
	using Setup;
	using SmartStore.Utilities;
	using SmartStore.Core.Domain.Media;
	using Core.Domain.Configuration;
	using SmartStore.Core.Domain.Customers;
	using SmartStore.Core.Domain.Seo;

	public sealed class MigrationsConfiguration : DbMigrationsConfiguration<SmartObjectContext>
	{
		public MigrationsConfiguration()
		{
			AutomaticMigrationsEnabled = false;
			AutomaticMigrationDataLossAllowed = true;
			ContextKey = "SmartStore.Core";
		}

		public void SeedDatabase(SmartObjectContext context)
		{
			Seed(context);
		}

		protected override void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
			MigrateSettings(context);

			context.SaveChanges();
        }

		public void MigrateSettings(SmartObjectContext context)
		{
			// SeoSettings.RedirectLegacyTopicUrls should be true when migrating (it is false by default after fresh install)
			var name = TypeHelper.NameOf<SeoSettings>(y => y.RedirectLegacyTopicUrls, true);
			context.MigrateSettings(x => x.Add(name, true));

			// remove setting which were moved from customer settings to privacy settings and have new default values which should be applied immediately 
			var settings = context.Set<Setting>();
			var storeLastIpAddressSetting = settings.FirstOrDefault(x => x.Name == "CustomerSettings.StoreLastIpAddress");
			if (storeLastIpAddressSetting != null) settings.Remove(storeLastIpAddressSetting);
			
			var displayPrivacyAgreementOnContactUs = settings.FirstOrDefault(x => x.Name == "CustomerSettings.DisplayPrivacyAgreementOnContactUs");
			if (displayPrivacyAgreementOnContactUs != null) settings.Remove(displayPrivacyAgreementOnContactUs);

		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ThirdPartyEmailHandOver.Hint",
				"Specifies whether customers can agree to a transferring of their email address to third parties when ordering, and whether the checkbox is enabled by default during checkout. Please note that the 'Show activated' option isn't legally compliant in line with the GDPR.",
				"Legt fest, ob Kunden bei einer Bestellung der Weitergabe ihrer E-Mail Adresse an Dritte zustimmen können und ob die Checkbox dafür standardmäßig aktiviert ist. Bitte beachten Sie, dass die Option 'Aktiviert anzeigen' im Rahmen der DSVGO nicht rechtskonform ist.");

			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy", "Privacy", "Datenschutz");
			
			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.EnableCookieConsent",
				"Enable cookie consent", 
				"Cookie-Hinweis aktivieren",
				"Specifies whether the cookie consent box will be displayed in the frontend.", 
				"Legt fest, ob ein Element für die Zustimmung zur Nutzung von Cookies im Frontend angezeigt wird.");

			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.CookieConsentBadgetext",
				"Cookie consent display text",
				"Cookie-Hinweistext",
				"Specifies the text, that will be displayed to your customers if they havn't agreed to the usage of cookis yet.",
				"Bestimmt den Text, der Ihren Kunden beim Besuch der Seite angezeigt wird, sofern Sie ihre Zustimmung zur Nutzung von Cookies noch nicht gegeben haben.");
			
			builder.AddOrUpdate("CookieConsent.BadgeText",
				"{0} is using cookies, to guarantee to best shopping experience. Partially cookies will be set by thrid parties. <a href='{1}'>Privacy Info</a>",
				"{0} benutzt Cookies, um Ihnen das beste Einkaufs-Erlebnis zu ermöglichen. Zum Teil werden Cookies auch von Drittanbietern gesetzt. <a href='{1}'>Datenschutzerklärung</a>");

			builder.AddOrUpdate("CookieConsent.Button", "Okay, got it", "Ok, verstanden");

			builder.Delete("ContactUs.PrivacyAgreement");

			builder.Delete("Admin.Configuration.Settings.CustomerUser.StoreLastIpAddress");
			builder.Delete("Admin.Configuration.Settings.CustomerUser.StoreLastIpAddress.Hint");
			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.StoreLastIpAddress",
				"Store IP address",
				"IP-Adresse speichern",
				"Specifies whether to store the IP address in the customer data set.",
				"Legt fest, ob die IP-Adresse im Kundendatensatz gespeichert werden soll.");

			builder.Delete("Admin.Configuration.Settings.CustomerUser.DisplayPrivacyAgreementOnContactUs");
			builder.Delete("Admin.Configuration.Settings.CustomerUser.DisplayPrivacyAgreementOnContactUs.Hint");
			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.DisplayPrivacyAgreementOnContactUs",
				"Get privacy consent for contact requests",
				"Einwilligungserklärung im Kontaktformular fordern",
				"Specifies whether a checkbox will be displayed on the contact page which requests the user to agree on storage of his data.",
				"Bestimmt ob im Kontaktformular eine Checkbox angezeigt wird, die den Benutzer auffordert der Speicherung seiner Daten zuzustimmen.");
		}
	}
}

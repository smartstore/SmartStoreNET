namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using Setup;
<<<<<<< HEAD
=======
	using SmartStore.Utilities;
	using SmartStore.Core.Domain.Media;
	using Core.Domain.Configuration;
	using SmartStore.Core.Domain.Customers;
	using SmartStore.Core.Domain.Seo;
>>>>>>> upstream/3.x

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
        }

		public void MigrateSettings(SmartObjectContext context)
		{
<<<<<<< HEAD
=======
			// SeoSettings.RedirectLegacyTopicUrls should be true when migrating (it is false by default after fresh install)
			var name = TypeHelper.NameOf<SeoSettings>(y => y.RedirectLegacyTopicUrls, true);
			context.MigrateSettings(x => x.Add(name, true));
>>>>>>> upstream/3.x
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
<<<<<<< HEAD
=======
			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ThirdPartyEmailHandOver.Hint",
				"Specifies whether customers can agree to a transferring of their email address to third parties when ordering, and whether the checkbox is enabled by default during checkout. Please keep in mind that the option 'Show activated' isn't legally justified in line with the GDPR.",
				"Legt fest, ob Kunden bei einer Bestellung der Weitergabe ihrer E-Mail Adresse an Dritte zustimmen können und ob die Checkbox dafür standardmäßig aktiviert ist. Bitte beachten Sie, dass die Option 'Aktiviert anzeigen' im Rahmen der DSVGO nicht rechtskonform ist.");
>>>>>>> upstream/3.x
		}
	}
}

namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class Licensing : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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
			builder.AddOrUpdate("Admin.Common.License",
				"License",
				"Lizenzieren");

			builder.AddOrUpdate("Admin.Common.Licensed",
				"Licensed",
				"Lizenziert");

			builder.AddOrUpdate("Admin.Common.ResourceNotFound",
				"The resource was not found.",
				"Die gewünschte Ressource wurde nicht gefunden.");

			builder.AddOrUpdate("Admin.Configuration.Plugins.LicenseActivated",
				"The plugin has been successfully licensed.",
				"Das Plugin wurde erfolgreich lizenziert.");

			builder.AddOrUpdate("Admin.Configuration.Plugins.LicenseKey",
				"License key",
				"Lizenzschlüssel",
				"Please enter the license key for the plugin.",
				"Bitte den Lizenzschlüssel für das Plugin eingeben.");

			builder.AddOrUpdate("Admin.Plugins.AddOnLicensing",
				"Add-on licensing",
				"Add-on Lizenzierung");

			builder.AddOrUpdate("Admin.Plugins.LicensingDemoRemainingDays",
				"Demo {0} day(s) remaining",
				"Demo {0} Tag(e) verbleibend");

			builder.AddOrUpdate("Admin.Plugins.LicensingDemoNotStarted",
				"Demo",
				"Demo");

			builder.AddOrUpdate("Admin.Plugins.LicensingDemoExpired",
				"Demo expired",
				"Demo abgelaufen");

			builder.AddOrUpdate("Admin.Plugins.LicensingResetStatusCheck",
				"Renew check",
				"Prüfung erneuern");


			builder.AddOrUpdate("Admin.Configuration.Payment.CannotActivatePaymentMethod",
				"Activating this payment method is forbidden by the plugin.",
				"Das Plugin erlaubt keine Aktivierung dieser Zahlungsmethode.");

			builder.AddOrUpdate("Admin.Configuration.Shipping.CannotActivateShippingRateComputationMethod",
				"Activating this shipping rate computation method is forbidden by the plugin.",
				"Das Plugin erlaubt keine Aktivierung dieser Berechnungsmethode für Versandkosten.");
		}
    }
}

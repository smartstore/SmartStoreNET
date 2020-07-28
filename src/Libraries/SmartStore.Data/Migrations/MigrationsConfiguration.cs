namespace SmartStore.Data.Migrations
{
	using System;
    using System.Linq;
	using System.Data.Entity.Migrations;
	using Setup;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Catalog;
	using SmartStore.Core.Domain.Common;
    using SmartStore.Core.Domain.Configuration;
    using SmartStore.Core.Domain.Media;
    using SmartStore.Core.Domain.Tasks;
    using SmartStore.Utilities;

	public sealed class MigrationsConfiguration : DbMigrationsConfiguration<SmartObjectContext>
	{
		public MigrationsConfiguration()
		{
			AutomaticMigrationsEnabled = false;
			AutomaticMigrationDataLossAllowed = true;
			ContextKey = "SmartStore.Core";

            if (DataSettings.Current.IsSqlServer)
            {
                var commandTimeout = CommonHelper.GetAppSetting<int?>("sm:EfMigrationsCommandTimeout");
                if (commandTimeout.HasValue)
                {
                    CommandTimeout = commandTimeout.Value;
                }

                CommandTimeout = 9999999;
            }
		}

		public void SeedDatabase(SmartObjectContext context)
		{
			using (var scope = new DbContextScope(context, hooksEnabled: false))
			{
				Seed(context);
				scope.Commit();
			}		
		}

		protected override void Seed(SmartObjectContext context)
		{
            context.MigrateLocaleResources(MigrateLocaleResources);
			MigrateSettings(context);
        }

		public void MigrateSettings(SmartObjectContext context)
		{

        }

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.Delete(
				"Admin.SalesReport.Bestsellers.RunReport",
				"Admin.SalesReport.NeverSold.RunReport",
				"Admin.Customers.Reports.RunReport",
				"Admin.Customers.Reports.BestBy.BestByNumberOfOrders",
				"Admin.Customers.Reports.BestBy.BestByNumberOfOrders");

			builder.AddOrUpdate("Admin.Customers.Reports.BestCustomers", "Top customers", "Top Kunden");

			builder.AddOrUpdate("Admin.Configuration.Settings.Search.SearchProductByIdentificationNumber",
				"Open product directly at SKU, MPN or GTIN",
				"Produkt bei SKU, MPN oder GTIN direkt öffnen",
				"Specifies whether the product page should be opened directly if the search term matches a SKU, MPN or GTIN.",
				"Legt fest, ob bei einer Übereinstimmung des Suchbegriffes mit einer SKU, MPN oder GTIN die Produktseite direkt geöffnet werden soll.");

			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.PasswordMinLength",
				"Minimum password length",
				"Mindestlänge eines Passworts",
				"Specifies the minimum length of a password.",
				"Legt die minimale Länge eines Passworts fest.");

			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.MinDigitsInPassword",
				"Minimum digits in password",
				"Mindestanzahl von Ziffern im Passwort",
				"Specifies the minimum number of digits for a password.",
				"Legt die Mindestanzahl von Ziffern eines Passworts fest.");

			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.MinSpecialCharsInPassword",
				"Minimum special characters in password",
				"Mindestanzahl von Sonderzeichen im Passwort",
				"Specifies the minimum number of special characters for a password.",
				"Legt die Mindestanzahl von Sonderzeichen eines Passworts fest.");

			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.MinUppercaseCharsInPassword",
				"Minimum uppercase letters in password",
				"Mindestanzahl von Großbuchstaben im Passwort",
				"Specifies the minimum number of uppercase letters for a password.",
				"Legt die Mindestanzahl von Großbuchstaben eines Passworts fest.");

			builder.AddOrUpdate("Account.Fields.Password.MustContainChars",
				"The password must contain at least {0}.",
				"Das Passwort muss mindestens {0} enthalten.");
			builder.AddOrUpdate("Account.Fields.Password.Digits", "{0} digits", "{0} Ziffern");
			builder.AddOrUpdate("Account.Fields.Password.SpecialChars", "{0} special characters", "{0} Sonderzeichen");
			builder.AddOrUpdate("Account.Fields.Password.UppercaseChars", "{0} uppercase letters", "{0} Großbuchstaben");

			builder.AddOrUpdate("Admin.Rules.FilterDescriptor.PaymentMethod", "Payment method", "Zahlart");
			builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Group.BrowserUserAgent", "Browser User Agent", "Browser User-Agent");

			builder.AddOrUpdate("Admin.Orders.NoOrdersSelected",
				"No orders are selected. Please select the desired orders.",
				"Es sind keine Aufträge ausgewählt. Bitte wählen Sie die gewünschten Aufträge aus.");

			builder.AddOrUpdate("Admin.Orders.ProcessSelectedOrders",
				"There are {0} orders selected. Would you like to proceed?",
				"Es sind {0} Aufträge ausgewählt. Möchten Sie fortfahren?");

			builder.AddOrUpdate("Admin.Orders.ProcessingResult",
				"<div>{0} of {1} orders were successfully processed.</div><div{3}>{2} orders cannot be changed as desired due to their current status and were skipped.</div>",
				"<div>Es wurden {0} von {1} Aufträgen erfolgreich verarbeitet.</div><div{3}>{2} Aufträge können aufgrund ihres aktuellen Status nicht wie gewünscht geändert werden und wurden übersprungen.</div>");
		}
	}
}

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

			builder.AddOrUpdate("Admin.Configuration.Countries.Fields.DefaultCurrency",
				"Default currency",
				"Standardwährung",
				"Specifies the default currency. Preselects the default currency in the shop according to the country to which the current IP address belongs.",
				"Legt die Standardwährung fest. Bewirkt im Shop eine Vorauswahl der Standardwährung anhand des Landes, zu dem die aktuelle IP-Adresse gehört.");

			builder.AddOrUpdate("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.MediaFile",
				"Picture",
				"Bild",
				"Specifies an image to be displayed as the selection element for the attribute.",
				"Legt ein Bild fest, welches als Auswahlelement für das Attribut angezeigt werden soll.");

			builder.AddOrUpdate("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.Color",
				"RGB color",
				"RGB-Farbe",
				"Specifies a color for the color squares control.",
				"Legt eine Farbe für das Farbflächen-Steuerelement fest.");

			builder.AddOrUpdate("Common.Entity.CheckoutAttributeValue", "Checkout attribute option", "Checkout-Attribut-Option");

			builder.AddOrUpdate("Checkout.MaxOrderSubtotalAmount",
				"The maximum order value for the subtotal is {0}.",
				"Der Höchstbestellwert für die Zwischensumme ist {0}.");

			builder.AddOrUpdate("Checkout.MaxOrderTotalAmount",
				"The maximum order value for the total is {0}.",
				"Der Höchstbestellwert der Gesamtsumme ist {0}.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Order.MinOrderAmount",
				"Min order amount",
				"Mindestbestellwert",
				"Enter minimum order amount.",
				"Legt den Mindestbestellwert fest.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Order.MaxOrderAmount",
				"Max order amount",
				"Höchstbestellwert",
				"Enter maximum order amount.",
				"Legt den Höchstbestellwert fest.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Order.ApplyToSubtotal",
				"Order amount related to subtotal",
				"Bestellwert bezogen auf Zwischensumme",
				"Determines whether the min/max order amount refers to the order subtotal, otherwise it refers to the total amount.",
				"Bestimmt, ob sich der Mindest-/Höchstbetrag auf die Auftragszwischensumme bezieht, andernfalls bezieht er sich auf den Gesamtbetrag.");

			builder.Delete("Admin.Configuration.Settings.Order.MaxOrderSubtotalAmount");
			builder.Delete("Admin.Configuration.Settings.Order.MaxOrderSubtotalAmount.Hint");
			builder.Delete("Admin.Configuration.Settings.Order.MaxOrderTotalAmount");
			builder.Delete("Admin.Configuration.Settings.Order.MaxOrderTotalAmount.Hint");
			builder.Delete("Admin.Configuration.Settings.Order.MinOrderSubtotalAmount");
			builder.Delete("Admin.Configuration.Settings.Order.MinOrderSubtotalAmount.Hint");
			builder.Delete("Admin.Configuration.Settings.Order.MinOrderTotalAmount");
			builder.Delete("Admin.Configuration.Settings.Order.MinOrderTotalAmount.Hint");

			builder.AddOrUpdate("Admin.Customers.CustomerRoles.Fields.MinOrderAmount",
				"Min order amount",
				"Mindestbestellwert",
				"Enter minimum order amount for user in this customer group.",
				"Legt den Mindestbestellwert für Nutzer in der Kundengruppe fest.");

			builder.AddOrUpdate("Admin.Customers.CustomerRoles.Fields.MaxOrderAmount",
				"Max order amount",
				"Höchstbestellwert",
				"Enter maximum order amount for user in this customer group.",
				"Legt den Höchstbestellwert für Nutzer in der Kundengruppe fest.");

			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CustomerFormFields.Description",				
				"Manage form fields that are displayed during registration.<br>" +
				"In order to ensure the address transfer from the registration form, " +
				"it is necessary that the following fields are activated and filled in by the customer: " +
				"<ul><li>First name</li><li>Last name</li><li>E-mail</li><li>and all fields that are selected as required in the tab 'Addresses'</li></ul>",
				"Verwalten Sie Formularfelder, die während der Registrierung angezeigt werden.<br>" +
				"Um die Adressübergabe aus dem Registrierungsformular zu gewährleisten, ist es notwending, " +
				"dass folgende Felder aktiviert und vom Kunden ausgefüllt sind:" +
				"<ul><li>Vorname</li><li>Nachname</li><li>E-Mail</li><li>und alle Felder die im Tab \"Adressen\" als erforderlich ausgewählt sind</li></ul>");

			builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.UseDefaultsOnHomepageOnly",
				"Use defaults on Homepage only",
				"Standards nur auf der Startseite verwenden",
				"Determines whether the default meta informations are used only on the home page, rather than as defaults for every page.",
				"Legt fest, ob die Standard-Meta-Informationen nur auf der Startseite anstatt als Standardeinstellung für jede Seite verwendet werden.");
		}
	}
}

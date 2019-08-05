namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using Setup;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Catalog;
	using SmartStore.Core.Domain.Common;
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
            builder.AddOrUpdate("Admin.Configuration.Languages.NoAvailableLanguagesFound",
                "There were no other available languages found for version {0}. On <a href=\"https://translate.smartstore.com/\" target=\"_blank\">translate.smartstore.com</a> you will find more details about available resources.",
                "Es wurden keine weiteren verfügbaren Sprachen für Version {0} gefunden. Auf <a href=\"https://translate.smartstore.com/\" target=\"_blank\">translate.smartstore.com</a> finden Sie weitere Details zu verfügbaren Ressourcen.");

            builder.AddOrUpdate("Checkout.OrderCompletes",
                "Your order will be completed.",
                "Ihre Bestellung wird abgeschlossen.");

            builder.AddOrUpdate("Admin.Catalog.Attributes.CheckoutAttributes.Fields.TextPrompt",
                "Text prompt",
                "Text Eingabeaufforderung",
                "Specifies the prompt text.",
                "Legt den Text zur Eingabeaufforderung fest.");

            builder.AddOrUpdate("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Fields.TextPrompt",
                "Text prompt",
                "Text Eingabeaufforderung");

            builder.AddOrUpdate("Admin.Catalog.Categories.Fields.ExternalLink",
                "External link",
                "Externer Link",
                "Alternative external link for this category in the main menu and in category listings. For example, to a landing page that contains a back link to the category.",
                "Abweichender, externer Verweis für diese Warengruppe im Hauptmenü und in Warengruppen-Listings. Z.B. auf eine Landingpage, die einen Rückverweis auf die Warengruppe enthält.");

            // Rule
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.TaxExempt", "Tax exempt", "Steuerbefreit");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.BillingCountry", "Billing country", "Rechnungsland");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ShippingCountry", "Shipping country", "Lieferland");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LastActivityDays", "Last activity days", "Tage seit letztem Besuch");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CompletedOrderCount", "Completed order count", "Anzahl abgeschlossener Bestellungen");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CancelledOrderCount", "Cancelled order count", "Anzahl stornierter Bestellungen");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.NewOrderCount", "New order count", "Anzahl neuer Bestellungen");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.HasPurchasedProduct", "Has purchased product", "Hat eines der folgenden Produkt gekauft");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.HasPurchasedAllProducts", "Has purchased all products", "Hat alle folgenden Produkte gekauft");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.RuleSet", "Other rule set", "Anderer Regelsatz");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Active", "Is active", "Ist aktiv");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LastLoginDays", "Last login days", "Tage seit letztem Login");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CreatedDays", "Created days", "Tage seit Registrierung");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Salutation", "Salutation", "Anrede");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Title", "Title", "Titel");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Company", "Company", "Firma");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CustomerNumber", "Customer number", "Kundennummer");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.BirthDate", "Birthdate", "Geburtstag");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Gender", "Gender", "Gender");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ZipPostalCode", "Zip postal code", "Plz");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.VatNumberStatusId", "Vat number status", "USt-IdNr.");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.TimeZoneId", "Time zone", "Zeitzone");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.TaxDisplayTypeId", "Tax display type", "Steueranzeigetyp");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CountryId", "Country", "Land");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CurrencyId", "Currency", "Währung");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LanguageId", "Language", "Sprache");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LastForumVisit", "Last forum visit days", "Tage seit letztem Forenbesuch");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LastUserAgent", "Last user agent", "Zuletzt genutzter User-Agent");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.IsInCustomerRole", "In customer role", "In Kundengruppe");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.StoreId", "Store", "Store");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LastOrderDateDays", "Last order date days", "Tage seit letzter Bestellung");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.AcceptThirdPartyEmailHandOver", "Accept third party email handover", "Akzeptiert Weitergabe der Emailadresse an Dritte");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.OrderTotal", "Order total", "Gesamtbetrag der Bestellung");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.OrderSubtotalInclTax", "Order subtotal incl. tax", "Gesamtbetrag der Bestellung (Brutto)");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.OrderSubtotalExclTax", "Order subtotal excl tax", "Gesamtbetrag der Bestellung (Netto)");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.PaymentMethodSystemName", "Payment method systemname", "Systemname der Zahlart");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ShippingRateComputationMethodSystemName", "Shipping rate computation method Systemname", "Systemname der Versandart");

            builder.AddOrUpdate("Admin.Rules.SystemName", "System name", "Systemname");
            builder.AddOrUpdate("Admin.Rules.Title", "Title", "Titel");
            builder.AddOrUpdate("Admin.Rules.Execute", "{0} Execute {1} Rules", "Bedingungen {0} Ausführen {1}");
            builder.AddOrUpdate("Admin.Configuration.RuleSets", "Rule sets", "Regelsätze");

            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.Name", "Name", "Name");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.Description", "Description", "Beschreibung");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.IsActive", "Is active", "Ist aktiv");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.Scope", "Scope", "Art");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.IsSubGroup", "Is sub group", "Titel");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.LogicalOperator", "Logical operator", "Logischer Operator");

            // RuleOperators
            builder.AddOrUpdate("Admin.Rules.RuleOperator.Contains", "Contains", "Enthält");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.EndsWith", "Ends with", "Endet auf");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.GreaterThan", "Greater than", "Größer als");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.GreaterThanOrEqualTo", "Greater than or equal to", "Größer oder gleich");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.In", "In", "Ist eine von");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.IsEmpty", "Is empty", "Ist leer");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.IsEqualTo", "Is equal to", "Gleich");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.IsNotEmpty", "Is not empty", "Ist nicht leer");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.IsNotEqualTo", "Is not equal to", "Ungleich");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.IsNotNull", "Is not null", "Ist nicht NULL");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.IsNull", "Is null", "Ist NULL");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.LessThan", "Less than", "Kleiner als");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.LessThanOrEqualTo", "Less than or equal to", "Kleiner oder gleich");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.NotContains", "Not contains", "Enthält nicht");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.NotIn", "Not in", "Ist KEINE von");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.StartsWith", "Starts with", "Beginnt mit");

        }
    }
}

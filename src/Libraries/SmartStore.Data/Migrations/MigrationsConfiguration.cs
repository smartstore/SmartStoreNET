namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using Setup;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Catalog;
	using SmartStore.Core.Domain.Common;
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
            // FluentValidation splits messages by dot for client validation. Don't use abbrevs.
            builder.AddOrUpdate("Validation.MinimumLengthValidator")
                .Value("de", "'{PropertyName}' muss mindestens {MinLength} Zeichen lang sein. Sie haben {TotalLength} Zeichen eingegeben.");
            builder.AddOrUpdate("Validation.MaximumLengthValidator")
                .Value("de", "'{PropertyName}' darf maximal {MaxLength} Zeichen lang sein. Sie haben {TotalLength} Zeichen eingegeben.");

            builder.AddOrUpdate("Admin.Configuration.Measures.Weights.AddWeight", "Add weight", "Gewicht hinzufügen");
            builder.AddOrUpdate("Admin.Configuration.Measures.Weights.EditWeight", "Edit weight", "Gewicht bearbeiten");

            builder.AddOrUpdate("Admin.Configuration.Measures.Dimensions.AddDimension", "Add dimension", "Abmessung hinzufügen");
            builder.AddOrUpdate("Admin.Configuration.Measures.Dimensions.EditDimension", "Edit dimension", "Abmessung bearbeiten");

            builder.AddOrUpdate("Admin.Configuration.QuantityUnit.AddQuantityUnit", "Add quantity unit", "Verpackungseinheit hinzufügen");
            builder.AddOrUpdate("Admin.Configuration.QuantityUnit.EditQuantityUnit", "Edit quantity unit", "Verpackungseinheit bearbeiten");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ApplyPercentageDiscountOnTierPrice",
                "Apply percentage discounts on tier prices",
                "Prozentuale Rabatte auf Staffelpreise anwenden",
                "Specifies whether to apply percentage discounts also on tier prices.",
                "Legt fest, ob prozentuale Rabatte auch auf Staffelpreise angewendet werden sollen.");

            builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ShowProductBundleImagesOnShoppingCart",
                "Show product images of bundle items",
                "Produktbilder von Bundle-Bestandteilen anzeigen",
                "Specifies whether to show product images of bundle items.",
                "Legt fest, ob Produktbilder von Bundle-Bestandteilen angezeigt werden sollen.");

            builder.AddOrUpdate("Admin.Promotions.Discounts.DiscountRequirementsCount",
                "Number of requirements",
                "Anzahl an Voraussetzungen");

            builder.AddOrUpdate("Admin.Common.CustomerRole.LimitedTo",
                "Limited to customer roles",
                "Auf Kundengruppen begrenzt",
                "Specifies whether the object is only available to certain customer groups.",
                "Legt fest, ob das Objekt nur für bestimmte Kundengruppen verfügbar ist.");

            builder.AddOrUpdate("Admin.Permissions.AllowInherited", "Allow (inherited)", "Erlaubt (geerbt)");
            builder.AddOrUpdate("Admin.Permissions.DenyInherited", "Deny (inherited)", "Verweigert (geerbt)");

            builder.AddOrUpdate("Admin.AccessDenied.DetailedDescription",
                "<div>You do not have permission to perform the selected operation.</div><div>Access right: {0}</div><div>System name: {1}</div>",
                "<div>Sie haben keine Berechtigung, diesen Vorgang durchzuführen.</div><div>Zugriffsrecht: {0}</div><div>Systemname: {1}</div>");

            builder.Delete(
                "Admin.Configuration.Measures.Weights.Fields.MarkAsPrimaryWeight",
                "Admin.Configuration.Measures.Dimensions.Fields.MarkAsPrimaryDimension",
                "Admin.Customers.Customers.Addresses.AddButton",
                "Admin.Address",
                "Admin.Catalog.Products.Acl",
                "Admin.Configuration.ACL.Updated",
                "Admin.Configuration.Stores.NoStoresDefined",
                "Admin.Configuration.Acl.NoRolesDefined",
                "Admin.Common.Acl.AvailableFor",
                "Admin.Catalog.Attributes.SpecificationAttributes.OptionsCount");

            builder.AddOrUpdate("Admin.ContentManagement.Blog.BlogPosts.Fields.Tags.Hint",
                "Tags are keywords that this blog post can also be identified by. Enter a comma separated list of the tags to be associated with this blog post.",
                "Dieser Blog-Eintrag kann durch die Verwendung von Tags (Stichwörter) gekennzeichnet und kategorisiert werden. Mehrere Tags können als kommagetrennte Liste eingegeben werden.");

            // Granular permission.
            builder.AddOrUpdate("Common.Read", "Read", "Lesen");
            builder.AddOrUpdate("Common.Create", "Create", "Erstellen");
            builder.AddOrUpdate("Common.Notify", "Notify", "Benachrichtigen");
            builder.AddOrUpdate("Common.Approve", "Approve", "Genehmigen");
            builder.AddOrUpdate("Common.Rules", "Rules", "Regeln");

            builder.AddOrUpdate("Common.Allow", "Allow", "Erlaubt");
            builder.AddOrUpdate("Common.Deny", "Deny", "Verweigert");

            builder.AddOrUpdate("Common.ExpandCollapseAll", "Expand\\collapse all", "Alle auf\\zuklappen");

            builder.AddOrUpdate("Admin.Customers.PermissionViewNote",
                "The view shows the permissions that apply to this customer based on the customer roles assigned to him. To change permissions, switch to the relevant <a class=\"alert-link\" href=\"{0}\">customer role</a>.",
                "Die Ansicht zeigt die Rechte, die für diesen Kunden auf Basis der ihm zugeordneten Kundengruppen gelten. Um Rechte zu ändern, wechseln Sie bitte zur betreffenden <a class=\"alert-link\" href=\"{0}\">Kundengruppe</a>.");

            builder.AddOrUpdate("Permissions.DisplayName.DisplayPrice", "Display prices", "Preise anzeigen");
            builder.AddOrUpdate("Permissions.DisplayName.AccessShop", "Access shop", "Zugang zum Shop");
            builder.AddOrUpdate("Permissions.DisplayName.AccessShoppingCart", "Access shoppping cart", "Auf Warenkorb zugreifen");
            builder.AddOrUpdate("Permissions.DisplayName.AccessWishlist", "Access wishlist", "Auf Wunschliste zugreifen");
            builder.AddOrUpdate("Permissions.DisplayName.AccessBackend", "Access backend", "Auf Backend zugreifen");
            builder.AddOrUpdate("Permissions.DisplayName.EditOrderItem", "Edit order items", "Auftragspositionen bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditShipment", "Edit shipment", "Sendungen bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditAnswer", "Edit answers", "Antworten bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditOptionSet", "Edit options sets", "Options-Sets bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditCategory", "Edit categories", "Warengruppen bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditManufacturer", "Edit manufacturers", "Hersteller bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditAssociatedProduct", "Edit associated products", "Verknüpfte Produkte bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditBundle", "Edit bundles", "Bundles bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditPromotion", "Edit promotion", "Promotion bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditPicture", "Edit pictures", "Bilder bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditTag", "Edit tags", "Tags bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditAttribute", "Edit attributes", "Attribute bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditVariant", "Edit variants", "Varianten bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditTierPrice", "Edit tier prices", "Staffelpreise bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditRecurringPayment", "Edit recurring payment", "Wiederkehrende Zahlungen bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditOption", "Edit options", "Optionen bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditAddress", "Edit addresses", "Adressen bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditCustomerRole", "Edit customer roles", "Kundengruppen bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.SendPM", "Send private messages", "Private Nachrichten senden");
            builder.AddOrUpdate("Permissions.DisplayName.EditProduct", "Edit products", "Produkte bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditComment", "Edit comments", "Kommentare bearbeiten");
            builder.AddOrUpdate("Permissions.DisplayName.EditResource", "Edit resources", "Ressourcen bearbeiten");

            builder.AddOrUpdate("Admin.ContentManagement.Blog.Heading.Publish", "Publishing", "Veröffentlichung");
            builder.AddOrUpdate("Admin.ContentManagement.Blog.Heading.Display", "Display", "Darstellung");
            builder.AddOrUpdate("Admin.ContentManagement.News.Heading.Publish", "Publishing", "Veröffentlichung");

            builder.AddOrUpdate("Admin.Validation.Url", "Please enter a valid URL.", "Bitte geben Sie eine gültige URL ein.");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.Visibility",
                "Visibility",
                "Sichtbarkeit",
                "Limits the visibility of the product. In the case of \"Not visible\", the product only appears as an associated product on the parent product detail page, but without a link to an individual page.",
                "Schränkt die Sichtbarkeit des Produktes ein. Bei \"Nicht sichtbar\" erscheint das Produkt nur noch als verknüpftes Produkt auf der übergeordneten Produktdetailseite, jedoch ohne Verlinkung auf eine eigenständige Seite.");

            builder.AddOrUpdate("Admin.DataExchange.Export.Filter.Visibility",
                "Visibility",
                "Sichtbarkeit",
                "Filter by visibility. \"In search results\" includes fully visible products.",
                "Nach Sichtbarkeit filter. \"In Suchergebnissen\" schließt überall sichtbare Produkte mit ein.");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductVisibility.Full", "Fully visible", "Überall sichtbar");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductVisibility.SearchResults", "In search results", "In Suchergebnissen");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductVisibility.ProductPage", "On product detail pages", "Auf Produktdetailseiten");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductVisibility.Hidden", "Not visible", "Nicht sichtbar");

            builder.Delete(
                "Admin.Catalog.Products.Fields.VisibleIndividually",
                "Admin.Catalog.Products.Fields.VisibleIndividually.Hint");

            // Rule
            builder.AddOrUpdate("Admin.Rules.SystemName", "System name", "Systemname");
            builder.AddOrUpdate("Admin.Rules.Title", "Title", "Titel");
            builder.AddOrUpdate("Admin.Rules.Execute", "{0} Execute {1} Rules", "Bedingungen {0} Ausführen {1}");
            builder.AddOrUpdate("Admin.Rules.AddGroup", "Add group", "Gruppe hinzufügen");
            builder.AddOrUpdate("Admin.Rules.DeleteGroup", "Delete group", "Gruppe löschen");
            builder.AddOrUpdate("Admin.Rules.AddCondition", "Add condition", "Bedingung hinzufügen");

            builder.AddOrUpdate("Admin.Rules.EditRule", "Edit rule", "Regel bearbeiten");
            builder.AddOrUpdate("Admin.Rules.AddRule", "Add rule", "Regel hinzufügen");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Added", "The rule has been successfully added.", "Die Regel wurde erfolgreich hinzugefügt.");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Deleted", "The rule has been successfully deleted.", "Die Regel wurde erfolgreich gelöscht.");
            builder.AddOrUpdate("Admin.Rules.Operator.All", "ALL", "ALLE");
            builder.AddOrUpdate("Admin.Rules.Operator.One", "ONE", "EINE");
            builder.AddOrUpdate("Admin.Rules.OneOrAllConditions",
                "<span>If at least</span> {0} <span>of the following conditions is true.</span>",
                "<span>Wenn mindestens</span> {0} <span>der folgenden Bedingungen zutrifft.</span>");

            // Rule fields
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.Name", "Name", "Name");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.Description", "Description", "Beschreibung");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.IsActive", "Is active", "Ist aktiv");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.Scope", "Scope", "Art");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.IsSubGroup", "Is sub group", "Titel");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.LogicalOperator", "Logical operator", "Logischer Operator");

            // Rule descriptors
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.TaxExempt", "Tax exempt", "Steuerbefreit");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.BillingCountry", "Billing country", "Rechnungsland");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ShippingCountry", "Shipping country", "Lieferland");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LastActivityDays", "Last activity days", "Tage seit letztem Besuch");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CompletedOrderCount", "Completed order count", "Anzahl abgeschlossener Bestellungen");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CancelledOrderCount", "Cancelled order count", "Anzahl stornierter Bestellungen");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.NewOrderCount", "New order count", "Anzahl neuer Bestellungen");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.OrderCountInStore", "Order count in shop", "Anzahl Bestellungen im Shop");
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
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ZipPostalCode", "Zip postal code", "PLZ");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.VatNumberStatusId", "Vat number status", "USt-IdNr.");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.TimeZoneId", "Time zone", "Zeitzone");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.TaxDisplayTypeId", "Tax display type", "Steueranzeigetyp");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.IPCountry", "IP associated with country", "IP gehört zu Land");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CountryId", "Country", "Land");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CurrencyId", "Currency", "Währung");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LanguageId", "Language", "Sprache");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LastForumVisit", "Last forum visit days", "Tage seit letztem Forenbesuch");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LastUserAgent", "Last user agent", "Zuletzt genutzter User-Agent");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.IsInCustomerRole", "In customer role", "In Kundengruppe");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.StoreId", "Store", "Shop");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LastOrderDateDays", "Last order date days", "Tage seit letzter Bestellung");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.AcceptThirdPartyEmailHandOver", "Accept third party email handover", "Akzeptiert Weitergabe der E-Mail Adresse an Dritte");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CartTotal", "Amount of cart", "Betrag des Warenkorbes");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.OrderTotal", "Order total", "Gesamtbetrag der Bestellung");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.OrderSubtotalInclTax", "Order subtotal incl. tax", "Gesamtbetrag der Bestellung (Brutto)");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.OrderSubtotalExclTax", "Order subtotal excl tax", "Gesamtbetrag der Bestellung (Netto)");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.PaymentMethodSystemName", "Payment method systemname", "Systemname der Zahlart");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ShippingRateComputationMethodSystemName", "Shipping rate computation method Systemname", "Systemname der Versandart");

            // Rule operators
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

            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.Cart", "Cart", "Warenkorb");
            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.OrderItem", "Order item", "Bestellposition");
            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.Customer", "Customer", "Kunde");
            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.Product", "Product", "Produkt");
        }
    }
}

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

            builder.AddOrUpdate("Admin.Permissions.AddedPermissions",
                "Added permissions: {0}",
                "Hinzugefügte Zugriffsrechte: {0}");

            builder.AddOrUpdate("Admin.Permissions.RemovedPermissions",
                "Permissions have been removed: {0}",
                "Zugriffsrechte wurden entfernt: {0}");

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
            
            builder.AddOrUpdate("Common.WrongInvisibleCaptcha",
                "The reCAPTCHA failed. Please try it again.",
                "reCAPTCHA ist fehlgeschlagen. Bitte versuchen Sie es erneut.");

            builder.AddOrUpdate("Common.CannotDisplayView",
                "The view \"{0}\" could not be displayed.",
                "Die Ansicht \"{0}\" konnte nicht angezeigt werden.");

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
                "Admin.Catalog.Products.Fields.VisibleIndividually.Hint",
                "Admin.Promotions.Discounts.NoDiscountsAvailable",
                "Admin.Orders.Shipments.TrackingNumber.Button");

            // Rule
            builder.AddOrUpdate("Admin.Rules.SystemName", "System name", "Systemname");
            builder.AddOrUpdate("Admin.Rules.Title", "Title", "Titel");
            builder.AddOrUpdate("Admin.Rules.Execute", "{0} Execute {1} Rules", "Bedingungen {0} Ausführen {1}");
            builder.AddOrUpdate("Admin.Rules.AddGroup", "Add group", "Gruppe hinzufügen");
            builder.AddOrUpdate("Admin.Rules.DeleteGroup", "Delete group", "Gruppe löschen");
            builder.AddOrUpdate("Admin.Rules.AddCondition", "Add condition", "Bedingung hinzufügen");
            builder.AddOrUpdate("Admin.Rules.SaveConditions", "Save all conditions", "Alle Bedingungen speichern");
            builder.AddOrUpdate("Admin.Rules.OpenRule", "Open rule", "Regel öffnen");

            builder.AddOrUpdate("Admin.Rules.EditRule", "Edit rule", "Regel bearbeiten");
            builder.AddOrUpdate("Admin.Rules.AddRule", "Add rule", "Regel hinzufügen");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Added", "The rule has been successfully added.", "Die Regel wurde erfolgreich hinzugefügt.");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Deleted", "The rule has been successfully deleted.", "Die Regel wurde erfolgreich gelöscht.");
            builder.AddOrUpdate("Admin.Rules.Operator.All", "ALL", "ALLE");
            builder.AddOrUpdate("Admin.Rules.Operator.One", "ONE", "EINE");

            builder.AddOrUpdate("Admin.Rules.OneCondition",
                "<span>If at least</span> {0} <span>of the following conditions is true.</span>",
                "<span>Wenn mindestens</span> {0} <span>der folgenden Bedingungen zutrifft.</span>");

            builder.AddOrUpdate("Admin.Rules.AllConditions",
                "<span>If</span> {0} <span>of the following conditions are true.</span>",
                "<span>Wenn</span> {0} <span>der folgenden Bedingungen erfüllt sind.</span>");

            builder.AddOrUpdate("Admin.Rules.NotFound", "The rule with ID {0} was not found.", "Die Regel mit der ID {0} wurde nicht gefunden.");
            builder.AddOrUpdate("Admin.Rules.GroupNotFound", "The group with ID {0} was not found.", "Die Gruppe mit der ID {0} wurde nicht gefunden.");
            builder.AddOrUpdate("Admin.Rules.NumberOfRules", "Number of rules", "Anzahl an Regeln");

            builder.AddOrUpdate("Admin.Rules.InvalidDescriptor",
                "Invalid rule. This rule is no longer supported and should be deleted.",
                "Ungültige Regel. Diese Regel wird nicht mehr unterstützt und sollte gelöscht werden.");

            builder.AddOrUpdate("Admin.Rules.Execute.MatchCustomers",
                "<b class=\"font-weight-medium\">{0}</b> customers match the rule conditions.",
                "<b class=\"font-weight-medium\">{0}</b> Kunden entsprechen den Regelbedingungen.");
            builder.AddOrUpdate("Admin.Rules.Execute.MatchProducts",
                "<b class=\"font-weight-medium\">{0}</b> products match the rule conditions.",
                "<b class=\"font-weight-medium\">{0}</b> Produkte entsprechen den Regelbedingungen.");
            builder.AddOrUpdate("Admin.Rules.Execute.MatchCart",
                "The rule conditions are <b class=\"font-weight-medium\">true</b> for the current customer {0}.",
                "Die Regelbedingungen sind für den aktuellen Kunden {0} <b class=\"font-weight-medium\">wahr</b>.");
            builder.AddOrUpdate("Admin.Rules.Execute.DoesNotMatchCart",
                "The rule conditions are <b class=\"font-weight-medium\">false</b> for the current customer {0}.",
                "Die Regelbedingungen sind für den aktuellen Kunden {0} <b class=\"font-weight-medium\">falsch</b>.");

            // Rule fields
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.Name", "Name", "Name");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.Description", "Description", "Beschreibung");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.IsActive", "Is active", "Ist aktiv");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.Scope", "Scope", "Art");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.IsSubGroup", "Is sub group", "Ist Untergruppe");
            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.LogicalOperator", "Logical operator", "Logischer Operator");

            // Rule descriptors
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.TaxExempt", "Tax exempt", "Steuerbefreit");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.BillingCountry", "Billing country", "Rechnungsland");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ShippingCountry", "Shipping country", "Lieferland");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LastActivityDays", "Days since last activity", "Tage seit letztem Besuch");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CompletedOrderCount", "Completed order count", "Anzahl abgeschlossener Bestellungen");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CancelledOrderCount", "Cancelled order count", "Anzahl stornierter Bestellungen");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.NewOrderCount", "New order count", "Anzahl neuer Bestellungen");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.OrderInStore", "Orders in store", "Bestellungen im Shop");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.PurchasedProduct", "Purchased product", "Gekauftes Produkt");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.PurchasedFromManufacturer", "Purchased from manufacturer", "Gekauft von Hersteller");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.HasPurchasedProduct", "Has purchased product", "Hat eines der folgenden Produkte gekauft");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.HasPurchasedAllProducts", "Has purchased all products", "Hat alle folgenden Produkte gekauft");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.RuleSet", "Other rule set", "Anderer Regelsatz");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Active", "Is active", "Ist aktiv");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LastLoginDays", "Days since last login", "Tage seit letztem Login");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CreatedDays", "Days since registration", "Tage seit Registrierung");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Salutation", "Salutation", "Anrede");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Title", "Title", "Titel");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Company", "Company", "Firma");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CustomerNumber", "Customer number", "Kundennummer");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.BirthDate", "Days since date of birth", "Tage seit dem Geburtsdatum");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Gender", "Gender", "Geschlecht");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ZipPostalCode", "Zip postal code", "PLZ");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.VatNumberStatus", "Vat number status", "Steuernummerstatus");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.TimeZone", "Time zone", "Zeitzone");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.TaxDisplayType", "Tax display type", "Steueranzeigetyp");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.IPCountry", "IP associated with country", "IP gehört zu Land");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Currency", "Currency", "Währung");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.MobileDevice", "Mobile device", "Mobiles Endgerät");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.DeviceFamily", "Device family", "Endgerätefamilie");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.OperatingSystem", "Operating system", "Betriebssystem");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.BrowserName", "Browser name", "Browser Name");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.BrowserMajorVersion", "Browser major version", "Browser Hauptversionsnummer");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.BrowserMinorVersion", "Browser minor version", "Browser Nebenversionsnummer");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Language", "Language", "Sprache");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LastForumVisit", "Days since last forum visit", "Tage seit letztem Forenbesuch");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LastUserAgent", "Last user agent", "Zuletzt genutzter User-Agent");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.IsInCustomerRole", "In customer role", "In Kundengruppe");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Store", "Store", "Shop");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.LastOrderDateDays", "Days since last order", "Tage seit letzter Bestellung");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.AcceptThirdPartyEmailHandOver", "Accept third party email handover", "Akzeptiert Weitergabe der E-Mail Adresse an Dritte");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CartTotal", "Total amount of cart", "Gesamtbetrag des Warenkorbes");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CartSubtotal", "Subtotal amount of cart", "Zwischensumme des Warenkorbes");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ProductInCart", "Product in cart", "Produkt im Warenkorb");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ProductFromCategoryInCart", "Product from category in cart", "Produkt aus Kategorie im Warenkorb");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ProductFromManufacturerInCart", "Product from manufacturer in cart", "Produkt von Hersteller im Warenkorb");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ProductOnWishlist", "Product on wishlist", "Produkt auf der Wunschliste");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ProductReviewCount", "Number of product reviews", "Anzahl der Produkt Rezensionen");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.RewardPointsBalance", "Number of reward points", "Anzahl der Bonuspunkte");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.OrderTotal", "Order total", "Gesamtbetrag der Bestellung");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.OrderSubtotalInclTax", "Order subtotal incl. tax", "Gesamtbetrag der Bestellung (Brutto)");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.OrderSubtotalExclTax", "Order subtotal excl tax", "Gesamtbetrag der Bestellung (Netto)");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.OrderCount", "Number of orders", "Anzahl der Aufträge");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.SpentAmount", "Amount spent", "Ausgegebener Betrag");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.PaymentMethod", "Selected payment method", "Gewählte Zahlart");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.PaymentStatus", "Payment status", "Zahlungsstatus");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.PaidBy", "Paid by", "Bezahlt mit");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ShippingRateComputationMethod", "Shipping rate computation method", "Berechnungsmethode für Versandkosten");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ShippingMethod", "Shipping method", "Versandart");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ShippingStatus", "Shipping status", "Lieferstatus");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.ReturnRequestCount", "Number of return requests", "Anzahl Rücksendeaufträge");

            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Manufacturer", "Manufacturer", "Hersteller");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Published", "Published", "Veröffentlicht");
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.SpecificationAttribute", "Specification Attribute", "Spezifikationsattribut");

            // Rule operators
            builder.AddOrUpdate("Admin.Rules.RuleOperator.ContainsOperator", "Contains", "Enthält");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.EndsWithOperator", "Ends with", "Endet auf");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.GreaterThanOperator", "Greater than", "Größer als");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.GreaterThanOrEqualOperator", "Greater than or equal to", "Größer oder gleich");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.IsEmptyOperator", "Is empty", "Ist leer");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.EqualOperator", "Is equal to", "Gleich");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.IsNotEmptyOperator", "Is not empty", "Ist nicht leer");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.NotEqualOperator", "Is not equal to", "Ungleich");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.IsNotNullOperator", "Is not null", "Ist nicht NULL");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.IsNullOperator", "Is null", "Ist NULL");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.LessThanOperator", "Less than", "Kleiner als");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.LessThanOrEqualOperator", "Less than or equal to", "Kleiner oder gleich");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.NotContainsOperator", "Not contains", "Enthält nicht");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.StartsWithOperator", "Starts with", "Beginnt mit");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.InOperator", "In", "Ist eine von");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.NotInOperator", "Not in", "Ist KEINE von");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.AllInOperator", "All in", "Sind alle von");
            builder.AddOrUpdate("Admin.Rules.RuleOperator.NotAllInOperator", "Not all in", "Sind nicht alle von");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.InOperator",
                "At least one value of the left list is contained in the right list",
                "Mind. ein Wert der linken Liste ist in der rechten Liste enthalten",
                "True for left {1,2,3} and right {5,4,3}. False for left {1,2,3} and right {6,5,4}.",
                "Wahr für links {1,2,3} und rechts {5,4,3}. Falsch für links {1,2,3} und rechts {6,5,4}.");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.NotInOperator",
                "At least one value of the left list is missing in the right list",
                "Mind. ein Wert der linken Liste fehlt in der rechten Liste",
                "True for left {1,2,3} and right {3,4,5,6}. False for left {1,2,3} and right {5,4,3,2,1}.",
                "Wahr für links {1,2,3} und rechts {3,4,5,6}. Falsch für links {1,2,3} und rechts {5,4,3,2,1}.");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.AllInOperator",
                "Right list contains ALL values of the left list",
                "Rechte Liste enthält ALLE Werte der linken Liste",
                "True for left {3,2,1} and right {0,1,2,3}. False for left {1,2,9} and right {9,8,2}.",
                "Wahr für links {3,2,1} und rechts {0,1,2,3}. Falsch für links {1,2,9} und rechts {9,8,2}.");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.NotAllInOperator",
                "Right list contains NO value of the left list",
                "Rechte Liste enthält KEINEN Wert der linken Liste",
                "True for left {1,2,3} and right {4,5}. False for left {1,2,3} and right {3,4,5}.",
                "Wahr für links {1,2,3} und rechts {4,5}. Falsch für links {1,2,3} und rechts {3,4,5}.");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.ContainsOperator",
                "Left list contains ALL values of the right list",
                "Linke Liste enthält ALLE Werte der rechten Liste",
                "True for left {3,2,1,0} and right {2,3}. False for left {3,2,1} and right {0,1,2,3}.",
                "Wahr für links {3,2,1,0} und rechts {2,3}. Falsch für links {3,2,1} und rechts {0,1,2,3}.");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.NotContainsOperator",
                "Left list contains NO value of the right list",
                "Linke Liste enthält KEINEN Wert der rechten Liste",
                "True for left {1,2,3} and right {9,8}. False for left {1,2,3} and right {9,8,2}.",
                "Wahr für links {1,2,3} und rechts {9,8}. Falsch für links {1,2,3} und rechts {9,8,2}.");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.EqualOperator",
                "Left and right list contain the same values",
                "Linke und rechte Liste enthalten dieselben Werte",
                "True for left {1,2,3} and right {3,1,2}. False for left {1,2,3} and right {1,2,3,4}.",
                "Wahr für links {1,2,3} und rechts {3,1,2}. Falsch für links {1,2,3} und rechts {1,2,3,4}.");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.NotEqualOperator",
                "Left and right list differ in at least one value",
                "Linke und rechte Liste unterscheiden sich in mind. einem Wert",
                "True for left {1,2,3} and right {1,2,3,4}. False for left {1,2,3} and right {3,1,2}.",
                "Wahr für links {1,2,3} und rechts {1,2,3,4}. Falsch für links {1,2,3} und rechts {3,1,2}.");

            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.Cart", "Cart", "Warenkorb");
            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.OrderItem", "Order item", "Bestellposition");
            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.Customer", "Customer", "Kunde");
            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.Product", "Product", "Produkt");

            builder.AddOrUpdate("SmartStore.Blog.Button", "Visit our blog", "Zum Blog");
            builder.AddOrUpdate("SmartStore.Blog.Button.Hint", "Click here to be forwarded to our blog", "Klicken Sie hier um zu unserem Blog weitergeleitet zu werden");

            builder.AddOrUpdate("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.ColorSquaresRgb",
                "RGB color",
                "RGB-Farbe",
                "Specifies a color for the color squares control.",
                "Legt eine Farbe für das Farbquadrat-Steuerelement fest.");

            builder.AddOrUpdate("Admin.Catalog.Attributes.SpecificationAttributes.Options.Fields.Picture",
                "Picture",
                "Bild",
                "Specifies an image as the selector element.",
                "Legt ein Bild als Auswahlelement fest.");

            builder.AddOrUpdate("Admin.Catalog.Manufacturers.Fields.BottomDescription",
                "Bottom description",
                "Untere Beschreibung",
                "Optional second description displayed below products on the category page.",
                "Optionale zweite Beschreibung, die auf der Herstellerseite unterhalb der Produkte angezeigt wird.");

            builder.AddOrUpdate("Admin.Promotions.Discounts.AppliedDiscounts",
                "Discounts",
                "Rabatte",
                "Specifies discounts to be applied to the object.",
                "Legt auf das Objekt anzuwendende Rabatte fest.");

            builder.AddOrUpdate("Admin.Promotions.Discounts.RuleSetRequirements",
                "Requirements",
                "Voraussetzungen",
                "Specifies requirements for the applying of the discount. The discount is applied when one of the selected rules is fulfilled.",
                "Legt Voraussetzungen für die Anwendung des Rabatts fest. Der Rabatt wird gewährt, wenn eine der ausgewählten Regeln erfüllt ist.");

            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.AssignedToDiscounts",
                "Assigned to discounts",
                "Rabatten zugeordnet",
                "A list of discounts to which the rule is assigned. The assignment can be made on the discount details page.",
                "Eine Liste von Rabatten, denen die Regel zugeordnet ist. Die Zuordnung kann auf der Rabattdetailseite vorgenommen werden.");

            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.AssignedToShippingMethods",
                "Assigned to shipping methods",
                "Versandarten zugeordnet",
                "A list of shipping methods to which the rule is assigned. The assignment can be made on the shipping method details page.",
                "Eine Liste von Versandarten, denen die Regel zugeordnet ist. Die Zuordnung kann auf der Detailseite der Versandart vorgenommen werden.");

            builder.AddOrUpdate("Admin.Rules.RuleSet.Fields.AssignedToPaymentMethods",
                "Assigned to payment methods",
                "Zahlarten zugeordnet",
                "A list of payment methods to which the rule is assigned. The assignment can be made on the payment method details page.",
                "Eine Liste von Zahlarten, denen die Regel zugeordnet ist. Die Zuordnung kann auf der Detailseite der Zahlart vorgenommen werden.");

            builder.AddOrUpdate("Admin.Configuration.Shipping.Methods.Fields.Requirements",
                "Requirements",
                "Voraussetzungen",
                "Specifies requirements for the availability of the shipping method. The shipping method is offered if one of the selected rules is fulfilled.",
                "Legt Voraussetzungen für die Verfügbarkeit der Versandart fest. Die Versandart wird angeboten, wenn eine der ausgewählten Regeln erfüllt ist.");

            builder.AddOrUpdate("Admin.Configuration.Payment.Methods.Requirements",
                "Requirements",
                "Voraussetzungen",
                "Specifies requirements for the availability of the payment method. The payment method is offered if one of the selected rules is fulfilled.",
                "Legt Voraussetzungen für die Verfügbarkeit der Zahlart fest. Die Zahlart wird angeboten, wenn eine der ausgewählten Regeln erfüllt ist.");

            builder.AddOrUpdate("Admin.Common.IsPublished", "Published", "Veröffentlicht");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Discounts.DiscountType.AssignedToOrderTotal")
                .Value("de", "Bezogen auf Gesamtsumme");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Discounts.DiscountType.AssignedToSkus")
                .Value("de", "Bezogen auf Produkte");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Discounts.DiscountType.AssignedToCategories")
                .Value("de", "Bezogen auf Warengruppen");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Discounts.DiscountType.AssignedToManufacturers")
                .Value("de", "Bezogen auf Hersteller");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Discounts.DiscountType.AssignedToShipping")
                .Value("de", "Bezogen auf Versandkosten");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Discounts.DiscountType.AssignedToOrderSubTotal")
                .Value("de", "Bezogen auf Zwischensumme");


            builder.AddOrUpdate("ActivityLog.EditSettings", 
                "The setting {0} has been changed. The new value is {1}.",
                "Die Einstellung {0} wurde geändert. Der neue Wert ist {1}.");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.Condition",
                "Product condition",
                "Artikelzustand",
                "Specifies the product condition.",
                "Legt den Artikelzustand fest.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowProductCondition",
                "Show product condition",
                "Artikelzustand anzeigen",
                "Specifies whether to show the product condition.",
                "Legt fest, ob der Artikelzustand im Shop angezeigt werden soll.");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductCondition.New", "New", "Neu");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductCondition.Refurbished", "Refurbished", "Generalüberholt");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductCondition.Used", "Used", "Gebraucht");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductCondition.Damaged", "Damaged", "Defekt");

            builder.AddOrUpdate("Products.Condition", "Product condition", "Artikelzustand");
            builder.AddOrUpdate("Common.OpenUrl", "Open URL", "URL öffnen");
            builder.AddOrUpdate("Common.NoPreview", "A preview is not available.", "Eine Vorschau ist nicht verfügbar.");

            builder.AddOrUpdate("Admin.Orders.Shipments.TrackingNumber",
                "Tracking number",
                "Tracking-Nummer",
                "Specifies the tracking number for tracking the shipment.",
                "Legt die Tracking-Nummer zur Sendungsverfolgung fest.");

            builder.AddOrUpdate("Admin.Orders.Shipments.TrackingUrl",
                "Tracking URL",
                "Tracking-URL",
                "Specifies the URL for tracking the shipment.",
                "Legt die URL zur Sendungsverfolgung fest.");

            builder.AddOrUpdate("Admin.Media.Album.Message", "Messages", "Nachrichten");
            builder.AddOrUpdate("Admin.Media.Album.File", "Files", "Dateien");

            builder.AddOrUpdate("Admin.Customers.CustomerRoles.AutomatedAssignmentRules",
                "Rules for automated assignment",
                "Regeln für automatische Zuordnung",
                "Customers are automatically assigned to this customer group by scheduled task if they fulfill one of the selected rules.",
                "Kunden werden automatisch per geplanter Aufgabe dieser Kundengruppe zugeordnet, wenn sie eine der gewählten Regeln erfüllen.");

            builder.AddOrUpdate("Admin.CustomerRoleMapping.IsSystemMapping", "Automatic assignment", "Automatische Zuordnung");
            builder.AddOrUpdate("Admin.CustomerRoleMapping.ReapplyRules", "Reapply rules", "Regeln neu anwenden");
            
            builder.AddOrUpdate("Admin.CustomerRoleMapping.RoleMappingListDescription",
                "The list shows customers who are assigned to this customer role. Customers are automatically assigned by scheduled task as long as the group is active and rules are specified for it. You can make a manual assignment using the customer role selection at the respective customer.",
                "Die Liste zeigt Kunden, die dieser Kundenruppe zugeordnet sind. Kunden werden automatisch per geplanter Aufgabe zugeordnet, sofern die Gruppe aktiv ist und für sie Regeln festgelegt sind. Eine manuelle Zuordnung können Sie über die Kundengruppenauswahl beim jeweiligen Kunden vornehmen.");

            builder.AddOrUpdate("Admin.System.ScheduleTasks.TaskNotFound",
                "The scheduled task \"{0}\" was not found.",
                "Die geplante Aufgabe \"{0}\" wurde nicht gefunden.");
        }
    }
}

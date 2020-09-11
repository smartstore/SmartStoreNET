namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web.Hosting;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Configuration;
    using SmartStore.Core.Domain.Media;
    using SmartStore.Data.Setup;

    public partial class V400Resources : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
            {
                // MaxUploadFileSize setting is stored with an empty string in some databases, which leads to type conversion problems.
                Sql("UPDATE [dbo].[Setting] SET [Value] = '102400' WHERE [Name] = 'MediaSettings.MaxUploadFileSize' And [Value] = ''");
            }
        }

        public override void Down()
        {
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
            MigrateSettings(context);
            context.SaveChanges();
        }

        public void MigrateSettings(SmartObjectContext context)
        {
            var prefix = nameof(MediaSettings) + ".";

            ChangeMediaSetting(nameof(MediaSettings.AvatarPictureSize), "256", x => x == 250);
            ChangeMediaSetting(nameof(MediaSettings.ProductThumbPictureSize), "256", x => x == 250);
            ChangeMediaSetting(nameof(MediaSettings.CategoryThumbPictureSize), "256", x => x == 250);
            ChangeMediaSetting(nameof(MediaSettings.ManufacturerThumbPictureSize), "256", x => x == 250);
            ChangeMediaSetting(nameof(MediaSettings.CartThumbPictureSize), "256", x => x == 250);
            ChangeMediaSetting(nameof(MediaSettings.MiniCartThumbPictureSize), "256", x => x == 250);
            ChangeMediaSetting(nameof(MediaSettings.ProductThumbPictureSizeOnProductDetailsPage), "72", x => x == 70);
            ChangeMediaSetting(nameof(MediaSettings.MessageProductThumbPictureSize), "72", x => x == 70);
            ChangeMediaSetting(nameof(MediaSettings.BundledProductPictureSize), "72", x => x == 70);
            ChangeMediaSetting(nameof(MediaSettings.VariantValueThumbPictureSize), "72", x => x == 70);
            ChangeMediaSetting(nameof(MediaSettings.AttributeOptionThumbPictureSize), "72", x => x == 70);

            void ChangeMediaSetting(string propName, string newVal, Func<int, bool> predicate)
            {
                var name = prefix + propName;
                var settings = context.Set<Setting>().Where(x => x.Name == name).ToList();
                foreach (var setting in settings)
                {
                    if (predicate(setting.Value.Convert<int>()))
                    {
                        setting.Value = newVal;
                    }
                }
            }
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
            builder.AddOrUpdate("Common.ExpandCollapseAll", @"Expand\collapse all", @"Alle auf-\zuklappen");
            builder.AddOrUpdate("Common.Trash", "Trash", "Papierkorb");
            builder.AddOrUpdate("Common.Cut", "Cut", "Ausschneiden");
            builder.AddOrUpdate("Common.Copy", "Copy", "Kopieren");
            builder.AddOrUpdate("Common.Paste", "Paste", "Einfügen");
            builder.AddOrUpdate("Common.SelectAll", "Select all", "Alles auswählen");
            builder.AddOrUpdate("Common.Rename", "Rename", "Umbenennen");

            builder.AddOrUpdate("Common.CtrlKey", "Ctrl", "Strg");
            builder.AddOrUpdate("Common.ShiftKey", "Shift", "Umschalt");
            builder.AddOrUpdate("Common.AltKey", "Alt", "Alt");
            builder.AddOrUpdate("Common.DelKey", "Del", "Entf");
            builder.AddOrUpdate("Common.EnterKey", "Enter", "Eingabe");
            builder.AddOrUpdate("Common.EscKey", "Esc", "Esc");

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
            builder.AddOrUpdate("Permissions.DisplayName.ReadStats", "Display dashboard", "Übersicht anzeigen");

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
                "Admin.Orders.Shipments.TrackingNumber.Button",
                "Admin.Catalog.Products.Copy.CopyImages");

            // Rule
            builder.AddOrUpdate("Admin.Rules.SystemName", "System name", "Systemname");
            builder.AddOrUpdate("Admin.Rules.Title", "Title", "Titel");
            builder.AddOrUpdate("Admin.Rules.TestConditions", "{0} Test {1} Rules", "Bedingungen {0} Testen {1}");
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
            builder.AddOrUpdate("Admin.Rules.OperatorNotSupported", "The rule scope does not support this operator.", "Die Regelart unterstützt diesen Operator nicht.");

            builder.AddOrUpdate("Admin.Rules.InvalidDescriptor",
                "Invalid rule. This rule is no longer supported and should be deleted.",
                "Ungültige Regel. Diese Regel wird nicht mehr unterstützt und sollte gelöscht werden.");

            builder.AddOrUpdate("Admin.Rules.SaveToCreateConditions",
                "Conditions can only be created after saving the rule.",
                "Bedingungen können erst nach Speichern der Regel festgelegt werden.");

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
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.AvailableByDate", "Available by date", "Nach Datum verfügbar");

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
                "At least one value left is included right",
                "Mind. ein Wert links ist rechts enthalten",
                "True for left {1,2,3} and right {5,4,3}. False for left {1,2,3} and right {6,5,4}.",
                "Wahr für links {1,2,3} und rechts {5,4,3}. Falsch für links {1,2,3} und rechts {6,5,4}.");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.NotInOperator",
                "At least one value left is missing right",
                "Mind. ein Wert links fehlt rechts",
                "True for left {1,2,3} and right {3,4,5,6}. False for left {1,2,3} and right {5,4,3,2,1}.",
                "Wahr für links {1,2,3} und rechts {3,4,5,6}. Falsch für links {1,2,3} und rechts {5,4,3,2,1}.");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.AllInOperator",
                "Right contains ALL values of left",
                "Rechts enthält ALLE Werte von links",
                "True for left {3,2,1} and right {0,1,2,3}. False for left {1,2,9} and right {9,8,2}.",
                "Wahr für links {3,2,1} und rechts {0,1,2,3}. Falsch für links {1,2,9} und rechts {9,8,2}.");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.NotAllInOperator",
                "Right contains NO value of left",
                "Rechts enthält KEINEN Wert von links",
                "True for left {1,2,3} and right {4,5}. False for left {1,2,3} and right {3,4,5}.",
                "Wahr für links {1,2,3} und rechts {4,5}. Falsch für links {1,2,3} und rechts {3,4,5}.");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.ContainsOperator",
                "Left contains ALL values of right",
                "Links enthält ALLE Werte von rechts",
                "True for left {3,2,1,0} and right {2,3}. False for left {3,2,1} and right {0,1,2,3}.",
                "Wahr für links {3,2,1,0} und rechts {2,3}. Falsch für links {3,2,1} und rechts {0,1,2,3}.");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.NotContainsOperator",
                "Left contains NO value of right",
                "Links enthält KEINEN Wert von rechts",
                "True for left {1,2,3} and right {9,8}. False for left {1,2,3} and right {9,8,2}.",
                "Wahr für links {1,2,3} und rechts {9,8}. Falsch für links {1,2,3} und rechts {9,8,2}.");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.EqualOperator",
                "Left and right contain the same values",
                "Links und rechts enthalten dieselben Werte",
                "True for left {1,2,3} and right {3,1,2}. False for left {1,2,3} and right {1,2,3,4}.",
                "Wahr für links {1,2,3} und rechts {3,1,2}. Falsch für links {1,2,3} und rechts {1,2,3,4}.");

            builder.AddOrUpdate("Admin.Rules.RuleOperator.Sequence.NotEqualOperator",
                "Left and right differ in at least one value",
                "Links und rechts unterscheiden sich in mind. einem Wert",
                "True for left {1,2,3} and right {1,2,3,4}. False for left {1,2,3} and right {3,1,2}.",
                "Wahr für links {1,2,3} und rechts {1,2,3,4}. Falsch für links {1,2,3} und rechts {3,1,2}.");

            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.Cart", "Cart", "Warenkorb");
            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.OrderItem", "Order item", "Bestellposition");
            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.Customer", "Customer", "Kunde");
            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.Product", "Product", "Produkt");

            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.Cart.Hint",
                "Rule to grant discounts to the customer or offer shipping and payment methods.",
                "Regel, um dem Kunden Rabatte zu gewähren oder Versand- und Zahlarten anzubieten.");

            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.Customer.Hint",
                "Rule to automatically assign customers to customer roles per scheduled task.",
                "Regel, um Kunden automatisch per geplanter Aufgabe Kundengruppen zuzuordnen.");

            builder.AddOrUpdate("Enums.SmartStore.Rules.RuleScope.Product.Hint",
                "Rule to automatically assign products to categories per scheduled task.",
                "Regel, um Produkte automatisch per geplanter Aufgabe Warengruppen zuzuordnen.");

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

            builder.AddOrUpdate("Admin.Rules.RuleSet.AssignedObjects",
                "Assigned {0}",
                "Zugeordnete {0}",
                "A list of objects to which the rule is assigned. The assignment can be made on the details page of the object.",
                "Eine Liste von Objekten, denen die Regel zugeordnet ist. Die Zuordnung kann auf der Detailseite des Objektes vorgenommen werden.");

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

            builder.AddOrUpdate("Admin.Configuration.Settings.Blog.NotifyAboutNewBlogComments.Hint")
                .Value("de", "Der Administrator erhält eine Benachrichtigungen bei neuen Blogkommentaren.");

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
            builder.AddOrUpdate("Admin.Media.Album.Content", "Content", "Inhalte");

            builder.AddOrUpdate("Admin.SalesReport.LatestOrders", "Latest orders", "Neueste Bestellungen");
            builder.AddOrUpdate("Admin.SalesReport.TopCustomers", "Top customers", "Top Kunden");

            builder.AddOrUpdate("Admin.Customers.CustomerRoles.AutomatedAssignmentRules",
                "Rules for automated assignment",
                "Regeln für automatische Zuordnung",
                "Customers are automatically assigned to this customer group by scheduled task if they fulfill one of the selected rules.",
                "Kunden werden automatisch per geplanter Aufgabe dieser Kundengruppe zugeordnet, wenn sie eine der gewählten Regeln erfüllen.");

            builder.AddOrUpdate("Admin.Catalog.Categories.AutomatedAssignmentRules",
                "Rules for automated assignment",
                "Regeln für automatische Zuordnung",
                "Products are automatically assigned to this category by scheduled task if they fulfill one of the selected rules.",
                "Produkte werden automatisch per geplanter Aufgabe dieser Warengruppe zugeordnet, wenn sie eine der gewählten Regeln erfüllen.");

            builder.AddOrUpdate("Admin.Rules.AddedByRule", "Added by rule", "Durch Regel hinzugefügt");
            builder.AddOrUpdate("Admin.Rules.ReapplyRules", "Reapply rules", "Regeln neu anwenden");

            builder.AddOrUpdate("Admin.CustomerRoleMapping.RoleMappingListDescription",
                "The list shows customers who are assigned to this customer role. Customers are automatically assigned by scheduled task as long as the group is active and active rules are specified for it. You can make a manual assignment using the customer role selection at the respective customer.",
                "Die Liste zeigt Kunden, die dieser Kundengruppe zugeordnet sind. Kunden werden automatisch per geplanter Aufgabe zugeordnet, sofern die Gruppe aktiv ist und für sie aktive Regeln festgelegt sind. Eine manuelle Zuordnung können Sie über die Kundengruppenauswahl beim jeweiligen Kunden vornehmen.");

            builder.AddOrUpdate("Admin.Catalog.Categories.ProductListDescription",
                "The list shows products that are assigned to this category. Products are automatically assigned by scheduled task as long as the category is published and active rules are specified for it.",
                "Die Liste zeigt Produkte, die dieser Warengruppe zugeordnet sind. Produkte werden automatisch per geplanter Aufgabe zugeordnet, sofern die Warengruppe veröffentlicht ist und für sie aktive Regeln festgelegt sind.");

            builder.AddOrUpdate("Admin.System.ScheduleTasks.TaskNotFound",
                "The scheduled task \"{0}\" was not found.",
                "Die geplante Aufgabe \"{0}\" wurde nicht gefunden.");

            builder.AddOrUpdate("Admin.SalesReport.ByQuantity", "By quantity", "Nach Anzahl");
            builder.AddOrUpdate("Admin.SalesReport.ByAmount", "By amount", "Nach Betrag");
            builder.AddOrUpdate("Admin.SalesReport.Attribute", "Attributes", "Attribute");
            builder.AddOrUpdate("Admin.SalesReport.Value", "Values", "Werte");
            builder.AddOrUpdate("Admin.SalesReport.NewOrders", "Total new orders", "Neue Aufträge");
            builder.AddOrUpdate("Admin.SalesReport.NoIncompleteOrders", "No incomplete orders", "Keine unvollständigen Aufträge");
            builder.AddOrUpdate("Admin.Report.StoreStatistics", "Statistics", "Statistiken");
            builder.AddOrUpdate("Admin.Report.CustomerRegistrations", "Customer registrations", "Kundenregistrierungen");
            builder.AddOrUpdate("Admin.Report.Today", "Today", "Heute");
            builder.AddOrUpdate("Admin.Report.Yesterday", "Yesterday", "Gestern");
            builder.AddOrUpdate("Admin.Report.LastWeek", "Last 7 days", "Letzte 7 Tage");
            builder.AddOrUpdate("Admin.Report.LastMonth", "Last 28 days", "Letzte 28 Tage");
            builder.AddOrUpdate("Admin.Report.ThisYear", "This year", "Dieses Jahr");
            builder.AddOrUpdate("Admin.Report.Overall", "Overall", "Insgesamt");
            builder.AddOrUpdate("Admin.Promotions.NewsLetterSubscriptions.Short", "Newsletter abos", "Newsletter Abos");
            builder.AddOrUpdate("Admin.Promotions.SalesReport.Sales", "Sales", "Verkäufe");
            builder.AddOrUpdate("Common.Amount", "Amount", "Betrag");
            builder.AddOrUpdate("Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.Short", "Combinations", "Kombinationen");
            builder.AddOrUpdate("Admin.CurrentWishlists.Short", "Whishlists", "Wunschlisten");
            builder.AddOrUpdate("Admin.CurrentCarts.Short", "Shopping carts", "Warenkörbe");
            builder.AddOrUpdate("Admin.SalesReport.Sales", "Sales", "Umsatz");
            builder.AddOrUpdate("Admin.SalesReport.Sales.Hint", "Total value of all orders", "Gesamtwert aller Aufträge");
            builder.AddOrUpdate("Admin.Report.OnlineCustomers", "Customers online within last 15 minutes", "Kunden in den letzten 15 Minuten online");
            builder.AddOrUpdate("Admin.Orders.Overall", "Orders overall", "Aufträge insgesamt");
            builder.AddOrUpdate("Admin.Report.Registrations", "Registrations", "Registrierungen");

            builder.AddOrUpdate("Common.FileUploader.Upload", "To upload files drop them here or click.", "Zum Hochladen Dateien hier ablegen oder klicken.");
            builder.AddOrUpdate("FileUploader.Dropzone.Message", "To upload files drop them here or click", "Zum Hochladen Dateien hier ablegen oder klicken");
            builder.AddOrUpdate("FileUploader.MultiFiles.MainMediaFile", "Main media file", "Hauptbild");
            builder.AddOrUpdate("FileUploader.Preview.SetMainMedia.Title", "Set as main picture", "Zum Hauptbild machen");
            builder.AddOrUpdate("FileUploader.Preview.DeleteEntityMedia.Title", "Remove assignment", "Zuordnung entfernen");

            builder.AddOrUpdate("FileUploader.StatusWindow.Uploading.File", "file is uploading", "Datei wird hochgeladen");
            builder.AddOrUpdate("FileUploader.StatusWindow.Uploading.Files", "files are uploading", "Dateien werden hochgeladen");
            builder.AddOrUpdate("FileUploader.StatusWindow.Complete.File", "upload complete", "Upload abgeschlossen");
            builder.AddOrUpdate("FileUploader.StatusWindow.Complete.Files", "uploads complete", "Uploads abgeschlossen");
            builder.AddOrUpdate("FileUploader.StatusWindow.Canceled.File", "upload canceled", "Upload abgebrochen");
            builder.AddOrUpdate("FileUploader.StatusWindow.Canceled.Files", "uploads canceled", "Uploads abgebrochen");
            builder.AddOrUpdate("FileUploader.StatusWindow.Collapse.Title", "Minimize", "Minimieren");

            builder.AddOrUpdate("FileUploader.DuplicateDialog.Title", "Replace or skip", "Ersetzen oder überspringen");
            builder.AddOrUpdate("FileUploader.DuplicateDialog.Intro", "A file with the name <span class='current-file'></span> already exists in the target.", "Im Ziel ist bereits eine Datei mit dem Namen <span class='current-file'></span> vorhanden.");
            builder.AddOrUpdate("FileUploader.DuplicateDialog.DupeFile.Title", "Source file", "Quelldatei");
            builder.AddOrUpdate("FileUploader.DuplicateDialog.ExistingFile.Title", "Destination file", "Zieldatei");
            builder.AddOrUpdate("FileUploader.DuplicateDialog.Option.Skip", "Skip this file", "Diese Datei überspringen");
            builder.AddOrUpdate("FileUploader.DuplicateDialog.Option.Replace", "Replace file in target", "Datei im Ziel ersetzen");
            builder.AddOrUpdate("FileUploader.DuplicateDialog.Option.Rename", "Rename file", "Datei umbenennen");
            builder.AddOrUpdate("FileUploader.DuplicateDialog.Option.SaveSelection", "Remember selection and apply to remaing conflicts", "Auswahl merken und auf verbleibende Konflikte anwenden");

            builder.AddOrUpdate("FileUploader.Dropzone.DictDefaultMessage", "Drop files here to upload", "Dateien zum Hochladen hier ablegen");
            builder.AddOrUpdate("FileUploader.Dropzone.DictFallbackMessage", "Your browser does not support drag'n'drop file uploads.", "Ihr Browser unterstützt keine Datei-Uploads per Drag'n'Drop.");
            builder.AddOrUpdate("FileUploader.Dropzone.DictFallbackText", "Please use the fallback form below to upload your files like in the olden days.", "Bitte benutzen Sie das untenstehende Formular, um Ihre Dateien wie in längst vergangenen Zeiten hochzuladen.");
            builder.AddOrUpdate("FileUploader.Dropzone.DictFileTooBig", "File is too big ({{filesize}}MiB). Max filesize: {{maxFilesize}}MiB.", "Die Datei ist zu groß ({{filesize}}MB). Maximale Dateigröße: {{maxFilesize}}MB.");
            builder.AddOrUpdate("FileUploader.Dropzone.DictInvalidFileType", "You can't upload files of this type.", "Dateien dieses Typs können nicht hochgeladen werden.");
            builder.AddOrUpdate("FileUploader.Dropzone.DictResponseError", "Server responded with {{statusCode}} code.", "Der Server gab die Antwort {{statusCode}} zurück.");
            builder.AddOrUpdate("FileUploader.Dropzone.DictCancelUpload", "Cancel upload", "Upload abbrechen");
            builder.AddOrUpdate("FileUploader.Dropzone.DictUploadCanceled", "Upload canceled.", "Upload abgebrochen.");
            builder.AddOrUpdate("FileUploader.Dropzone.DictCancelUploadConfirmation", "Are you sure you want to cancel this upload?", "Sind Sie sicher, dass Sie den Upload abbrechen wollen?");
            builder.AddOrUpdate("FileUploader.Dropzone.DictRemoveFile", "Remove file", "Datei entfernen");
            builder.AddOrUpdate("FileUploader.Dropzone.DictMaxFilesExceeded", "You can not upload any more files.", "Sie können keine weiteren Dateien hochladen.");

            builder.AddOrUpdate("Admin.Catalog.Products.ProductPictures.Delete.Success", "The assignment was successfully removed.", "Die Zuordnung wurde erfolgreich entfernt.");

            builder.AddOrUpdate("Common.Entity.Customer", "Customer", "Kunde");
            builder.AddOrUpdate("Common.Entity.ProductAttributeOption", "Product attribute options set", "Produktattribut-Options-Set");
            builder.AddOrUpdate("Common.Entity.ProductVariantAttributeValue", "Product attribute option", "Produktattribut-Option");
            builder.AddOrUpdate("Common.Entity.SpecificationAttributeOption", "Specification attribute option", "Spezifikationsattribut-Option");
            builder.AddOrUpdate("Common.Entity.BlogPost", "Blog post", "Blog-Eintrag");
            builder.AddOrUpdate("Common.Entity.NewsItem", "News item", "News");
            builder.AddOrUpdate("Common.Entity.Download", "Download", "Download");
            builder.AddOrUpdate("Common.Entity.MessageTemplate", "Message template", "Nachrichtenvorlage");

            builder.AddOrUpdate("Common.Download.Version.Hint", "Enter the version number in the correct format (e.g.: 1.0.0.0, 2.0 or 3.1.5).", "Geben Sie die Versionsnummer in korrektem Format an (z.B.: 1.0.0.0, 2.0 oder 3.1.5).");
            builder.AddOrUpdate("Common.Download.Version.Placeholder", "e.g.: 1.0.0.0, 2.0 or 3.1.5", "z.B.: 1.0.0.0, 2.0 oder 3.1.5");

            builder.AddOrUpdate("Admin.Configuration.Settings.Media.MaxUploadFileSize",
                "Maximum file size",
                "Maximale Dateigröße",
                "Specifies the maximum file size of an upload (in KB). The default is 102,400 (100 MB).",
                "Legt die maximale Dateigröße eines Uploads in KB fest. Der Standardwert ist 102.400 (100 MB).");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.XmlSitemapEnabled",
                "Enable XML Sitemap",
                "XML-Sitemap aktivieren",
                "The XML sitemap contains URLs to store pages which can be automatically read and indexed by search engines like Google or Bing.",
                "Die XML-Sitemap enthält URLs zu Shop-Seiten, welche von Suchmaschinen wie Google oder Bing automatisch gelesen und indiziert werden können.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.XmlSitemapIncludesBlog",
                "Include blog posts",
                "Blog-Einträge einbeziehen",
                "Adds blog pages to sitemap.",
                "Fügt Blog-Seiten zur Sitemap hinzu.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.XmlSitemapIncludesCategories",
                "Include categories",
                "Warengruppen einbeziehen",
                "Adds category pages to sitemap.",
                "Fügt Warengruppen-Seiten zur Sitemap hinzu.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.XmlSitemapIncludesForum",
                "Include forums",
                "Foren einbeziehen",
                "Adds forum pages to sitemap.",
                "Fügt Forum-Seiten zur Sitemap hinzu.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.XmlSitemapIncludesManufacturers",
                "Include brands",
                "Hersteller einbeziehen",
                "Adds brand pages to sitemap.",
                "Fügt Hersteller-Seiten zur Sitemap hinzu.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.XmlSitemapIncludesNews",
                "Include news",
                "News einbeziehen",
                "Adds news pages to sitemap.",
                "Fügt News-Seiten zur Sitemap hinzu.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.XmlSitemapIncludesProducts",
                "Include products",
                "Produkte einbeziehen",
                "Adds product pages to sitemap.",
                "Fügt Produkt-Seiten zur Sitemap hinzu.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.XmlSitemapIncludesTopics",
                "Include topics",
                "Seiten einbeziehen",
                "Adds topic pages to sitemap.",
                "Fügt Inhalts-Seiten zur Sitemap hinzu.");

            builder.AddOrUpdate("Admin.System.XMLSitemap", "XML Sitemap", "XML-Sitemap");

            builder.AddOrUpdate("Admin.Configuration.Settings.Media.MakeFilesTransientWhenOrphaned",
                "Automatically delete orphaned files",
                "Verwaiste Dateien automatisch löschen",
                "Specifies whether orphaned media files should be automatically deleted during the next cleanup operation.",
                "Legt fest, ob verwaiste Mediendateien beim nächsten Aufräumvorgang automatisch gelöscht werden sollen.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Media.MediaTypes", "Media types", "Medientypen");

            builder.AddOrUpdate("Admin.Configuration.Settings.Media.MediaTypesNotes",
                "Media types control which files can be uploaded. All other media types are generally rejected. Please enter types separated by spaces and without dots.",
                "Medientypen steuern, welche Dateien hochgeladen werden können. Alle anderen Medientypen werden grundsätzlich abgelehnt. Die Typen bitte Leerzeichen getrennt und ohne Punkt angeben.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Media.Type.Image", "Image", "Bild");
            builder.AddOrUpdate("Admin.Configuration.Settings.Media.Type.Video", "Video", "Video");
            builder.AddOrUpdate("Admin.Configuration.Settings.Media.Type.Audio", "Audio", "Audio");
            builder.AddOrUpdate("Admin.Configuration.Settings.Media.Type.Document", "Document", "Dokument");
            builder.AddOrUpdate("Admin.Configuration.Settings.Media.Type.Text", "Text", "Text");
            builder.AddOrUpdate("Admin.Configuration.Settings.Media.Type.Bin", "Other", "Sonstige");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowDiscountSign",
                "Show discount sign",
                "Specifies whether a discount sign should be displayed on product pictures when discounts were applied",
                "Rabattzeichen anzeigen",
                "Legt fest, ob ein Rabattzeichen auf dem Produktbild angezeigt werden soll, wenn Rabatte angewendet wurden.");

            builder.AddOrUpdate("Admin.Common.IsPublished", "Published", "Veröffentlicht");

            builder.AddOrUpdate("Admin.CheckUpdate.AutoUpdatePossibleInfo",
                "&lt;p&gt;This update can be installed automatically. For this Smartstore downloads an installation package to your webserver, executes it and restarts the application. Before the installation your shop directory is backed up, except the folders &lt;i&gt;App_Data&lt;/i&gt; and &lt;i&gt;Media&lt;/i&gt;, as well as the SQL Server database file. &lt;/p&gt;&lt;p&gt;Click the &lt;b&gt;Update now&lt;/b&gt; button to download and install the package. As an alternative to this, you can download the package to your local PC further below and perform the installation at a later time manually.&lt;/p&gt;",
                "&lt;p&gt;Dieses Update kann automatisch installiert werden. Hierfür lädt Smartstore ein Installationspaket auf Ihren Webserver herunter, führt die Installation durch und startet die Anwendung neu. Vor der Installation wird der Verzeichnisinhalt Ihres Shops gesichert, mit Ausnahme der Ordner &lt;i&gt;App_Data&lt;/i&gt; und &lt;i&gt;Media&lt;/i&gt; sowie der SQL Server Datenbank. &lt;/p&gt;&lt;p&gt;Klicken Sie die Schaltfläche &lt;b&gt;Jetzt aktualisieren&lt;/b&gt;, um das Paket downzuloaden und zu installieren. Alternativ hierzu können Sie weiter unten das Paket auf Ihren lokalen PC downloaden und die Installation zu einem späteren Zeitpunkt manuell durchführen.&lt;/p&gt;");

            builder.AddOrUpdate("Admin.AppNews",
                "Smartstore News",
                "Smartstore News");

            builder.AddOrUpdate("Admin.CheckUpdate.IsUpToDate",
                "Smartstore is up to date",
                "Smartstore ist auf dem neuesten Stand");

            builder.AddOrUpdate("Admin.Common.About",
                "About Smartstore",
                "Über Smartstore");

            builder.AddOrUpdate("Admin.Help.OtherWorkNote",
                "Smartstore includes works distributed under the licenses listed below. Please refer to the specific resources for more detailed information about the authors, copyright notices and licenses.",
                "Smartstore beinhaltet Werke, die unter den unten aufgeführten Lizenzen vertrieben werden. Bitte beachten Sie die betreffenden Ressourcen für ausführlichere Informationen über Autoren, Copyright-Vermerke und Lizenzen.");

            builder.AddOrUpdate("Admin.Marketplace.ComingSoon",
                "In the Smartstore Marketplace we offer modules, themes &amp; language packages, which will make your shop better and more successful. Once we are ready to go, you'll be informed about the latest extensions here. Stay tuned...",
                "Im Smartstore Marketplace werden Module, Themes &amp; Sprachpakete angeboten, die Ihren Onlineshop besser, flexibler und erfolgreicher machen sollen. Sobald wir die Arbeiten am Marketplace abgeschlossen haben, werden Sie hier über die neuesten Erweiterungen informiert.");

            builder.AddOrUpdate("Admin.Packaging.IsIncompatible",
                "The package is not compatible the current app version {0}. Please update Smartstore or install another version of this package.",
                "Das Paket ist nicht kompatibel mit der aktuellen Programmversion {0}. Bitte aktualisieren Sie Smartstore oder nutzen Sie eine andere, kompatible Paket-Version.");

            builder.AddOrUpdate("Admin.PageTitle",
                "Smartstore administration",
                "Smartstore Administration");

            builder.AddOrUpdate("Admin.System.SystemInfo.AppDate.Hint",
                "The creation date of this Smartstore version.",
                "Das Erstellungsdatum dieser Smartstore Version.");

            builder.AddOrUpdate("Admin.System.SystemInfo.Appversion",
                "Smartstore version",
                "Smartstore Version");

            builder.AddOrUpdate("Admin.System.SystemInfo.AppVersion.Hint",
               "Smartstore version",
               "Smartstore Version");

            builder.AddOrUpdate("Admin.System.Warnings.IncompatiblePlugin",
                "'{0}' plugin is incompatible with your Smartstore version. Delete it or update to the latest version.",
                "'{0}' Plugin ist nicht kompatibel mit Ihrer Smartstore-Version. Löschen Sie es oder installieren Sie die richtige Version.");


            builder.AddOrUpdate("Admin.Media.Exception.FileNotFound",
                "Media file with Id '{0}' does not exist.",
                "Die Mediendatei mit der Id '{0}' existiert nicht.");

            builder.AddOrUpdate("Admin.Media.Exception.FolderNotFound",
                "Media folder '{0}' does not exist.",
                "Der Medienordner '{0}' existiert nicht.");

            builder.AddOrUpdate("Admin.Media.Exception.DuplicateFile",
                "File '{0}' already exists.",
                "Die Datei '{0}' existiert bereits.");

            builder.AddOrUpdate("Admin.Media.Exception.DuplicateFolder",
                "Folder '{0}' already exists.",
                "Der Ordner '{0}' existiert bereits.");

            builder.AddOrUpdate("Admin.Media.Exception.NotSameAlbum",
                "The file operation requires that the destination path belongs to the source album. Source: {0}, Destination: {1}.",
                "Die Dateioperation erfordert, dass der Zielpfad zum Ursprungsalbum gehört. Quelle: {0}, Ziel: {1}.");

            builder.AddOrUpdate("Admin.Media.Exception.DeniedMediaType",
                "The media type of '{0}' is not allowed. If you want the media type '{1}' supported, enter the file name extension to the media configuration under 'Configuration > Settings > Media > Media types'.",
                "Der Medientyp von '{0}' ist unzulässig. Wenn Sie wollen, dass der Medientyp '{1}' unterstützt wird, tragen Sie die Dateiendung in die Medienkonfiguration unter 'Konfiguration > Einstellungen > Medien > Medientypen' ein.");

            builder.AddOrUpdate("Admin.Media.Exception.DeniedMediaType.Hint",
                " Accepted: {0}, current: {1}.",
                " Akzeptiert: {0}, ausgewählt: {1}.");

            builder.AddOrUpdate("Admin.Media.Exception.ExtractThumbnail",
                "Thumbnail extraction for file '{0}' failed. Reason: {1}.",
                "Thumbnail-Erstellung für die Datei '{0}' ist fehlgeschlagen. Grund: {1}.");

            builder.AddOrUpdate("Admin.Media.Exception.MaxFileSizeExceeded",
               "The file '{0}' with a size of {1} exceeds the maximum allowed file size of {2}.",
               "Die Datei '{0}' mit einer Größe von {1} überschreitet die maximal zulässige Dateigröße von {2}.");

            builder.AddOrUpdate("Admin.Media.Exception.TopLevelAlbum",
                "Creating top-level (album) folders is not supported. Folder: {0}.",
                "Das Erstellen von Ordnern auf oberster Ebene (Album) wird nicht unterstützt. Ordner: {0}.");

            builder.AddOrUpdate("Admin.Media.Exception.AlterRootAlbum",
                "Moving or renaming root album folders is not supported. Folder: {0}.",
                "Das Verschieben oder Umbenennen von Album-Stammordnern wird nicht unterstützt. Ordner: {0}.");

            builder.AddOrUpdate("Admin.Media.Exception.DescendantFolder",
                "Destination folder '{0}' is not allowed to be a descendant of source folder '{1}'.",
                "Der Zielordner '{0}' darf kein Unterordner von '{1}' sein.");

            builder.AddOrUpdate("Admin.Media.Exception.CopyRootAlbum",
                "Copying root album folders is not supported. Folder: {0}.",
                "Das Kopieren von Album-Stammordnern wird nicht unterstützt. Ordner: {0}.");

            builder.AddOrUpdate("Admin.Media.Exception.InUse",
                "Cannot delete file '{0}' because it is being used by another process.",
                "Datei '{0}' kann nicht gelöscht werden, da sie von einem anderen Prozess verwendet wird.");

            builder.AddOrUpdate("Admin.Media.Exception.PathSpecification",
                "Invalid path specification '{0}' for '{1}' operation.",
                "Ungültige Pfadangabe '{0}' für den Befehl '{1}'.");

            builder.AddOrUpdate("Admin.Media.Exception.InvalidPath",
                "Invalid path '{0}'.",
                "Ungültiger Pfad '{0}'.");

            builder.AddOrUpdate("Admin.Media.Exception.InvalidPathExample",
                "Invalid path '{0}'. Valid path expression is: {{albumName}}[/subfolders]/{{fileName}}.{{extension}}",
                "Ungültiger Pfad '{0}'. Ein gültiger Pfadausdruck lautet: {{albumName}}[/subfolders]/{{fileName}}.{{extension}}");

            builder.AddOrUpdate("Admin.Media.Exception.FileExtension",
                "Cannot process files without file extension. Path: {0}",
                "Dateien ohne Dateiendung können nicht verarbeitet werden. Pfad: {0}");

            builder.AddOrUpdate("Admin.Media.Exception.Overwrite",
                "Overwrite operation is not possible if source and destination folders are identical.",
                "Ein Überschreibvorgang ist nicht möglich, wenn Quell- und Zielordner identisch sind.");

            builder.AddOrUpdate("Admin.Media.Exception.FolderAssignment",
                "Cannot operate on files without folder assignment.",
                "Kann nicht mit Dateien ohne Ordnerzuweisung arbeiten.");

            builder.AddOrUpdate("Admin.Media.Exception.MimeType",
                "The file operation '{0}' does not allow MIME type switching. Source MIME: {1}, target MIME: {2}.",
                "Die Dateioperation '{0}' erlaubt keine Änderung des MIME-Typs. Quell-MIME: {1}, Ziel-MIME: {2}.");

            builder.AddOrUpdate("Admin.Media.Exception.TrackUnassignedFile",
                "Cannot track a media file that is not assigned to any album.",
                "Es kann keine Mediendatei verfolgt werden, die keinem Album zugeordnet ist.");

            builder.AddOrUpdate("Admin.Media.Exception.AlbumNonexistent",
                "The album '{0}' does not exist.",
                "Das Album '{0}' existiert nicht.");

            builder.AddOrUpdate("Admin.Media.Exception.AlbumNoTrack",
                "The album '{0}' does not support track detection.",
                "Das Album '{0}' unterstützt die Erkennung von Verweisen nicht.");

            builder.AddOrUpdate("Admin.Media.Exception.NullInputStream",
                "Input stream was null",
                "Eingabe-Stream war null");

            builder.AddOrUpdate("Admin.Packaging.Dialog.Upload", "Upload & Install package file", "Paket-Datei hochladen & installieren");

            builder.AddOrUpdate("Admin.Packaging.InstallSuccess.Theme", "Theme was uploaded and installed successfully. Please reload the list.", "Theme wurde hochgeladen und erfolgreich installiert. Bitte laden Sie die Liste neu.");

            builder.AddOrUpdate("Admin.Media.Exception.DeleteReferenzedFile",
                "The file '{0}' is referenced by at least one entity. Permanently deleting referenced media files is not supported",
                "Die Datei '{0}' wird von mind. einer Entität referenziert. Endgültiges Löschen referenzierter Mediendateien wird nicht unterstützt.");

            builder.AddOrUpdate("Common.Resume", "Resume", "Fortsetzen");

            builder.AddOrUpdate("Admin.Media.Exception.FileNamesIdentical",
                "The source and destination file names are identical. Path: {0}",
                "Der Quell- und Zieldateiname sind identisch. Pfad: {0}");

            builder.Delete("ShoppingCart.MaximumUploadedFileSize");
            builder.Delete("Admin.Help.NopCommerceNote");

            builder.AddOrUpdate("Common.FileUploader.UploadAvatar", "To upload an avatar image drop files here or click.", "Zum Hochladen eines Avatarbildes Dateien hier platzieren oder klicken.");
        }
    }
}

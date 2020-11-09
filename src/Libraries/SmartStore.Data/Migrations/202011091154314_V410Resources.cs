namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Domain.Configuration;
    using SmartStore.Core.Domain.Media;
    using SmartStore.Core.Domain.Seo;
    using SmartStore.Data.Setup;
    using SmartStore.Utilities;

    public partial class V410Resources : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
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
            var settings = context.Set<Setting>();

            // Add .ico extension to MediaSettings.ImageTypes
            var name = TypeHelper.NameOf<MediaSettings>(y => y.ImageTypes, true);
            var setting = settings.FirstOrDefault(x => x.Name == name);
            if (setting != null)
            {
                var arr = setting.Value.EmptyNull()
                    .Replace(Environment.NewLine, " ")
                    .ToLower()
                    .Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (!arr.Contains("ico"))
                {
                    setting.Value += " ico";
                }
            }

            // Migrate old 'defaultSeoSettings' into new SeoSettings
            // Old settings.
            var defaultTitle = settings.FirstOrDefault(x => x.Name == "SeoSettings.DefaultTitle");
            var defaultMetaKeywords = settings.FirstOrDefault(x => x.Name == "SeoSettings.DefaultMetaKeywords");
            var defaultMetaDescription = settings.FirstOrDefault(x => x.Name == "SeoSettings.DefaultMetaDescription");

            // New settings.
            context.MigrateSettings(x =>
            {
                x.Add(TypeHelper.NameOf<SeoSettings>(y => y.MetaTitle, true), defaultTitle != null && defaultTitle.Value.HasValue() ? defaultTitle.Value : "");
                x.Add(TypeHelper.NameOf<SeoSettings>(y => y.MetaDescription, true), defaultMetaDescription != null && defaultMetaDescription.Value.HasValue() ? defaultMetaDescription.Value : "");
                x.Add(TypeHelper.NameOf<SeoSettings>(y => y.MetaKeywords, true), defaultMetaKeywords != null && defaultMetaKeywords.Value.HasValue() ? defaultMetaKeywords.Value : "");
            });

            if (defaultTitle != null) settings.Remove(defaultTitle);
            if (defaultMetaDescription != null) settings.Remove(defaultMetaDescription);
            if (defaultMetaKeywords != null) settings.Remove(defaultMetaKeywords);
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

            builder.AddOrUpdate("Admin.Customers.CustomerRoles.Fields.MinOrderTotal",
                "Minimum order total",
                "Mindestbestellwert",
                "Defines the minimum order total for customers in this group.",
                "Legt den Mindestbestellwert für Kunden in dieser Gruppe fest.");

            builder.AddOrUpdate("Admin.Customers.CustomerRoles.Fields.MaxOrderTotal",
                "Maximum order total",
                "Höchstbestellwert",
                "Defines the maximum order total for customers in this group.",
                "Legt den Höchstbestellwert für Kunden in dieser Gruppe fest.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Order.MinOrderTotal",
                "Minimum order total",
                "Mindestbestellwert",
                "Defines the default minimum order total for the shop. Overridden by customer group restrictions.",
                "Legt den standardmäßigen Mindestbestellwert für den Shop fest. Wird von Beschränkungen in Kundengruppen überschrieben.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Order.MaxOrderTotal",
                "Maximum order total",
                "Höchstbestellwert",
                "Defines the default maximum order total for the shop. Overridden by customer group restrictions.",
                "Legt den standardmäßigen Höchstbestellwert für den Shop fest. Wird von Beschränkungen in Kundengruppen überschrieben.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Order.ApplyToSubtotal",
                "Order total restriction in relation to subtotal",
                "Beschränkung bezogen auf Zwischensumme",
                "Determines whether the min/max order value refers to the order subtotal, otherwise it refers to the total amount.",
                "Bestimmt, ob sich der Mindest-/Höchstbetrag auf die Auftragszwischensumme bezieht, andernfalls bezieht er sich auf den Gesamtbetrag.");

            builder.Delete("Admin.Configuration.Settings.Order.MaxOrderSubtotalAmount",
                "Admin.Configuration.Settings.Order.MaxOrderSubtotalAmount.Hint",
                "Admin.Configuration.Settings.Order.MaxOrderTotalAmount",
                "Admin.Configuration.Settings.Order.MaxOrderTotalAmount.Hint",
                "Admin.Configuration.Settings.Order.MinOrderSubtotalAmount",
                "Admin.Configuration.Settings.Order.MinOrderSubtotalAmount.Hint",
                "Admin.Configuration.Settings.Order.MinOrderTotalAmount",
                "Admin.Configuration.Settings.Order.MinOrderTotalAmount.Hint");

            builder.AddOrUpdate("Admin.Configuration.Settings.Order.OrderTotalRestrictionType",
                "Restrictions in customer groups are cumulative",
                "Beschränkungen in Kundengruppen sind kumulativ",
                "Determines whether multiple order total restrictions are cumulated by customer group assignments.",
                "Bestimmt, ob mehrfache Bestellwertbeschränkungen durch Kundengruppenzuordnungen kumuliert werden.");

            builder.Delete("Products.AskQuestion.Question.Text");

            builder.AddOrUpdate("Products.AskQuestion.Sent",
                "Thank you. Your inquiry has been sent successfully.",
                "Vielen Dank. Ihre Anfrage wurde erfolgreich gesendet.");

            builder.AddOrUpdate("Products.AskQuestion.Question.GeneralInquiry",
                "I have following questions concerning the product {0}:",
                "Ich habe folgende Fragen zum Artikel {0}:");

            builder.AddOrUpdate("Products.AskQuestion.Question.QuoteRequest",
                "I would like to request price information on the following article: {0}",
                "Ich bitte um Preisinformationen zu folgendem Artikel: {0}");

            builder.AddOrUpdate("Products.AskQuestion.CallHotline", "or call directly", "oder rufen Sie direkt an");

            builder.AddOrUpdate("Products.AskQuestion.TitleQuoteRequest", "Request for quotation", "Angebotsanfrage");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CustomerFormFields.Description",
                "Manage form fields that are displayed during registration.<br>" +
                "In order to ensure the address transfer from the registration form, " +
                "it is necessary that the following fields are activated and filled in by the customer: " +
                "<ul><li>First name</li><li>Last name</li><li>E-mail</li><li>and all fields that are selected as required in the tab 'Addresses'</li></ul>",
                "Verwalten Sie Formularfelder, die während der Registrierung angezeigt werden.<br>" +
                "Um die Adressübergabe aus dem Registrierungsformular zu gewährleisten, ist es notwending, " +
                "dass folgende Felder aktiviert und vom Kunden ausgefüllt sind:" +
                "<ul><li>Vorname</li><li>Nachname</li><li>E-Mail</li><li>und alle Felder die im Tab \"Adressen\" als erforderlich ausgewählt sind</li></ul>");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.Cookies.AddCookieInfo.Title",
                "Add new cookie information",
                "Neue Cookie-Informationen hinzufügen");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.Cookies.EditCookieInfo.Title",
                "Edit new cookie information",
                "Cookie-Informationen bearbeiten");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.CookieInfo.Name",
                "Name",
                "Name",
                "Specifies the display name of the cookie information.",
                "Bestimmt den Anzeigenamen der Cookie-Information.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.CookieInfo.Description",
                "Description",
                "Beschreibung",
                "Specifies the description of the cookie information.",
                "Bestimmt die Beschreibung der Cookie-Information.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.CookieInfo.CookieType",
                "Cookie type",
                "Art des Cookies",
                "Specifies the usage type of the cookie (Required, Analytics, ThirdParty).",
                "Bestimmt die Verwendungsart des Cookies (Required, Analytics, ThirdParty).");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.SameSiteMode",
                "'SameSite' mode",
                "'SameSite' Modus");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.SameSiteMode.AdminInstruction",
                @"<h5>SameSite Mode</h5>
                    <p>
                        The SameSite setting for cookies specifies the level of security for cookies when they are requested by other sites (e.g. in a cross-site request or in an iframe).
                        The recommended setting is <b>Lax</b>, as cookies are sent with most secure actions.
                        <br />
                        If the store should also be embedded in an iframe, it is necessary to select the setting <b>None</b>. Additionally SSL must be set up in the store.
                    </p>
                    <ul>
                        <li><b>Unspecified:</b> No SameSite header is transmitted to requests.</li>
                        <li><b>None:</b> No mode specified. This setting must be used if the store should also be embedded in an iframe (SSL must be set up).</li>
                        <li><b>Lax:</b> Recommended setting. Cookies are sent at the highest level for requests on the same website and for cross-site requests.</li>
                        <li><b>Strict:</b> Cookies are only sent to requests from the same website.</li>
                    </ul>",
                @"<h5>SameSite Modus</h5>
                    <p>
                        Die SameSite-Einstellung für Cookies bestimmt, das Sicherheitslevel für Cookies, wenn diese von anderen Seiten (z.B. in einem Cross-Site-Request oder in einem IFrame) aufgerufen werden.
                        Die empfohlene Einstellung ist <b>Lax</b>, da hier Cookies bei den meisten gesicherten Aktionen mitgesendet werden.
                        <br />
                        Soll der Shop auch in ein IFrame eingebettet werden, ist es notwendig die Einstellung <b>None</b> zu wählen. Zusätzlich muss SSL im Shop eingerichtet sein.
                    </p>
                    <ul>
                        <li><b>Unspecified:</b> Es wird kein SameSite-Header an Requests übermittelt.</li>
                        <li><b>None:</b> Kein Modus angegeben. Diese Einstellung muss verwendet werden, wenn der Shop auch in ein IFrame eingebettet werden soll (SSL muss eingerichtet sein).</li>
                        <li><b>Lax:</b> Empfohlene Einstellung. Cookies werden bei Anfragen auf derselben Website und bei Cross-Site-Requests auf höchster Ebene gesendet.</li>
                        <li><b>Strict:</b> Cookies werden nur an Anfragen derselben Website gesendet.</li>
                    </ul>");


            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.ModalCookieConsent",
                "Modal Cookie Manager",
                "Cookie-Manager modal anzeigen",
                "Specifies whether the Cookie Manager is displayed in a modal dialog in the frontend. The modal mode requires a customer interaction with the dialog before the customer can navigate in the store.",
                "Bestimmt, ob der Cookie-Manager im Frontend in einem modalen Dialog angezeigt wird. Der modale Modus macht eine Kundeninteraktion mit dem Dialog erfoderlich, bevor der Kunde im Shop navigieren kann.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.EnableCookieConsent",
                "Enable Cookie Manager",
                "Cookie-Manager aktivieren",
                "Specifies whether the Cookie Manager will be displayed in the frontend.",
                "Legt fest, ob ein Dialog für die Zustimmung zur Nutzung von Cookies im Frontend angezeigt wird.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.CookieInfo.IsPluginInfo",
                "System cookie",
                "System-Cookie");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.Cookies.CookieInfoNotFound",
                "Cookie information wasn't found.",
                "Cookie-Information wurde nicht gefunden.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.Cookies.RegisterCookieInfo.Title",
                "Register your own cookie information",
                "Eigene Cookie-Informationen registrieren");

            builder.AddOrUpdate("CookieManager.Dialog.Hide",
                "Hide",
                "Ausblenden");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Privacy.Cookies.AdminInstruction",
                @"<p>
					If you have included third-party scripts in your shop that use cookies and are not integrated into Smartstore via a plugin,
					you can provide your own information about these cookies for the Cookie Manager in the following table.
					The added information is displayed in the Cookie Manager of the front end, corresponding to the type.
					Cookie information provided to the Cookie Manager by active plug-ins is displayed in this table,
					but cannot be edited. The texts of this information can be edited using the language resources
					<b>Admin > Configuration > Regional Settings > Languages</b>.
				</p>",
                @"<p>
					Wenn Sie Skripte von Drittanbietern in Ihren Shop eingebunden haben, die Cookies nutzen und die nicht über ein Plugin in Smartstore integriert sind,
					können Sie in folgender Tabelle eigene Informationen über diese Cookies für den Cookie-Manager bereitstellen.
					Die zugefügten Informationen werden, dem Typ entsprechend, im Cookie-Manager des Frontends angezeigt.
					Cookie-Informationen, die dem Cookie-Manager von aktiven Plugins zur Verfügung gestellt werden, werden in dieser Tabelle angezeigt,
					können aber nicht bearbeitet werden. Die Texte dieser Informationen können über die Sprachresourcen bearbeitet werden
					<b>Admin > Konfiguration > Regionale Einstellungen > Sprachen</b>.
				</p>");

            builder.AddOrUpdate("Admin.Catalog.Products.StockQuantityNotChanged",
                "The stock quantity has not been updated because the value has changed since the page was loaded (e.g. due to a placed order). Current value is {0}.",
                "Der Lagerbestand wurde nicht aktualisiert, weil sich der Wert seit Laden der Seite geändert hat (z.B. durch einen Auftragseingang). Aktueller Wert ist {0}.");

            builder.AddOrUpdate("Admin.Configuration.SeoTab.Title",
                "SEO settings",
                "SEO-Einstellungen");

            builder.AddOrUpdate("Admin.Configuration.Seo.MetaTitle",
                "Title tag",
                "Title-Tag",
                "Defines the title of the page, which is displayed in search engine results. If possible, enter a unique, concise title here.",
                "Bestimmt den Titel der Seite, der unter anderem in den Ergebnissen von Suchmaschinen angezeigt wird. Geben Sie hier nach Möglichkeit einen einzigartigen, prägnanten Titel an.");

            builder.AddOrUpdate("Admin.Configuration.Seo.MetaDescription",
                "Meta description",
                "Meta Description",
                "Defines the meta description of the site. An extract of this description is displayed in search engine results below the title.",
                "Bestimmt die Meta Desciption der Seite. Ein Auszug dieser Beschreibung wird in den Ergebnissen von Suchmaschinen unterhalb des Titels dargestellt.");

            builder.AddOrUpdate("Admin.Configuration.Seo.MetaKeywords",
                "Meta keywords",
                "Meta Keywords",
                "Defines the meta keywords of the site. Enter the keywords as a comma separated list.",
                "Bestimmt die Meta Keywords der Seite. Geben Sie die Keywords als kommagetrennte Liste an.");

            builder.AddOrUpdate("Admin.System.MetaInfos",
                "Meta tags",
                "Meta-Elemente");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.HomepageTitle",
                "Title tag (Homepage)",
                "Title-Tag (Startseite)",
                "Defines the title tag for the homepage, which is displayed in search engine results.",
                "Legt das Title-Tag für die Startseite fest, das unter anderem in den Ergebnissen von Suchmaschinen angezeigt wird.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.HomepageMetaDescription",
                "Meta description (Homepage)",
                "Meta Description (Startseite)",
                "Defines the meta description for the homepage. An extract of this description is displayed in search engine results below the title.",
                "Legt die Meta Description für die Startseite fest. Ein Auszug dieser Beschreibung wird in den Ergebnissen von Suchmaschinen unterhalb des Titels dargestellt.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.HomepageMetaKeywords",
                "Meta keywords (Homepage)",
                "Meta Keywords (Startseite)",
                "Defines the meta keywords for the homepage. Enter the keywords as a comma separated list.",
                "Legt die Meta Keywords für die Startseite fest. Geben Sie die Keywords als kommagetrennte Liste an.");

            builder.AddOrUpdate("Admin.Configuration.DeliveryTimes.Fields.MinDays",
                "Delivery not before (in days)",
                "Lieferung frühestens (in Tagen)",
                "Specifies the minimum number of days for the delivery.",
                "Legt den frühesten Zeitpunkt der Lieferung in Tagen fest.");

            builder.AddOrUpdate("Admin.Configuration.DeliveryTimes.Fields.MaxDays",
                "Delivery not later than (in days)",
                "Lieferung spätestens (in Tagen)",
                "Specifies the maximum number of days for the delivery.",
                "Legt den spätesten Zeitpunkt der Lieferung in Tagen fest.");

            builder.AddOrUpdate("Products.DeliveryDate", "Arrives:", "Lieferung:");

            builder.AddOrUpdate("DeliveryTimes.Dates.OnTomorrow", "Tomorrow", "Morgen");    // exactly tomorrow
            builder.AddOrUpdate("DeliveryTimes.Dates.Tomorrow", "tomorrow", "Morgen");      // <prefix> tomorrow

            builder.AddOrUpdate("DeliveryTimes.Dates.DeliveryOn", "<b>{0}</b>", "<b>{0}</b>");   // allow "on {0}" etc.
            builder.AddOrUpdate("DeliveryTimes.Dates.NotBefore", "not before <b>{0}</b>", "frühestens <b>{0}</b>");
            builder.AddOrUpdate("DeliveryTimes.Dates.Until", "until <b>{0}</b>", "bis <b>{0}</b>");
            builder.AddOrUpdate("DeliveryTimes.Dates.Between", "<b>{0}</b> - <b>{1}</b>", "<b>{0}</b> - <b>{1}</b>");   // allow "{0} till {1}" etc.

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.DeliveryTimesPresentation.None", "Do not display", "Nicht anzeigen");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.DeliveryTimesPresentation.DateOnly", "Date only (if possible)", "Nur Datum (sofern möglich)");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.DeliveryTimesPresentation.LabelOnly", "Label only", "Nur Label");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Directory.DeliveryTimesPresentation.LabelAndDate", "Label and date", "Label und Datum");

            builder.Delete(
                "Admin.Configuration.Settings.Catalog.ShowDeliveryTimesInProductLists",
                "Admin.Configuration.Settings.Catalog.ShowDeliveryTimesInProductLists.Hint",
                "Admin.Configuration.Settings.Catalog.ShowDeliveryTimesInProductDetail",
                "Admin.Configuration.Settings.Catalog.ShowDeliveryTimesInProductDetail.Hint",
                "Admin.Configuration.Settings.ShoppingCart.ShowDeliveryTimes",
                "Admin.Configuration.Settings.ShoppingCart.ShowDeliveryTimes.Hint");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.DeliveryTimesInProductDetail",
                "Presentation of delivery times",
                "Darstellung von Lieferzeiten",
                "Specifies the way delivery times are displayed on product detail pages.",
                "Legt die Darstellungsart von Lieferzeiten auf Produktdetailseiten fest.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.DeliveryTimesInLists",
                "Presentation of delivery times",
                "Darstellung von Lieferzeiten",
                "Specifies the way delivery times are displayed in product lists. Due to lack of space, the grid view does not show a date for the delivery time.",
                "Legt die Darstellungsart von Lieferzeiten in Produktlisten fest. Aus Platzgründen wird in der Rasteransicht kein Datum zur Lieferzeit angezeigt.");

            builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.DeliveryTimesInShoppingCart",
                "Presentation of delivery times",
                "Darstellung von Lieferzeiten",
                "Specifies the way delivery times are displayed in shopping cart.",
                "Legt die Darstellungsart von Lieferzeiten im Warenkorb fest.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Shipping.TodayShipmentHour",
                "Order before x o'clock for same day shipment",
                "Bis x Uhr bestellt, heute verschickt",
                "Specifies the hour by which the order will be shipped the same day.",
                "Legt die Stunde fest, bis zu der die Bestellung noch am selben Tag verschickt wird.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Shipping.DeliveryOnWorkweekDaysOnly",
                "Delivery on workweek days only",
                "Lieferungen nur an Werktagen",
                "Specifies whether delivery takes place only on workweek days.",
                "Legt fest, ob Lieferungen nur an Werktagen stattfinden.");

            builder.AddOrUpdate("Admin.Configuration.Seo.SeName",
                "URL alias",
                "URL-Alias",
                "Set a search engine friendly page name e.g. 'my-landing-page' to make the page URL 'http://www.yourStore.com/my-landing-page'. Leave empty to generate it automatically based on the name of the entity.",
                "Legen Sie einen suchmaschinenfreundlichen Seitennamen fest, z.B. 'meine-landing-page', um die URL zu 'http://www.yourStore.com/meine-landing-page' zu machen. Lassen Sie das Feld leer, um den Pfad automatisch auf Basis des Namens der Entität zu generieren.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.PageTitleSeparator",
                "Page title separator",
                "Titel-Trennzeichen",
                "Specify page title tag separator.",
                "Legt das Trennzeichen für das Titel-Tag fest.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.PageTitleSeoAdjustment",
                "Title tag order",
                "Titel-Tag Reihenfolge",
                "Select the page title order. The generated title tag can be (PAGE NAME | YOURSTORE.COM) or (YOURSTORE.COM | PAGE NAME).",
                "Legen Sie hier die Seitentitel-Reihenfolge fest. Das erzeugte Title-Tag könnte z.B. (MEINSHOP.DE | SEITE NAME) oder (SEITE NAME | MEINSHOP.DE) lauten.");

            builder.Delete("Admin.Catalog.Categories.Fields.SeName",
                "Admin.Catalog.Categories.Fields.SeName.Hint",
                "Admin.Catalog.Manufacturers.Fields.SeName",
                "Admin.Catalog.Manufacturers.Fields.SeName.Hint",
                "Admin.Catalog.Products.Fields.SeName",
                "Admin.Catalog.Products.Fields.SeName.Hint",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.SeName",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.SeName.Hint",
                "Admin.ContentManagement.Forums.Forum.Fields.SeName",
                "Admin.ContentManagement.Forums.Forum.Fields.SeName.Hint",
                "Admin.ContentManagement.Forums.ForumGroup.Fields.SeName",
                "Admin.ContentManagement.Forums.ForumGroup.Fields.SeName.Hint",
                "Admin.ContentManagement.News.NewsItems.Fields.SeName",
                "Admin.ContentManagement.News.NewsItems.Fields.SeName.Hint",
                "Admin.ContentManagement.Topics.Fields.SeName",
                "Admin.ContentManagement.Topics.Fields.SeName.Hint");

            builder.Delete("Admin.Catalog.Categories.Fields.MetaTitle",
                "Admin.Catalog.Categories.Fields.MetaTitle.Hint",
                "Admin.Catalog.Manufacturers.Fields.MetaTitle",
                "Admin.Catalog.Manufacturers.Fields.MetaTitle.Hint",
                "Admin.Catalog.Products.Fields.MetaTitle",
                "Admin.Catalog.Products.Fields.MetaTitle.Hint",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.MetaTitle",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.MetaTitle.Hint",
                "Admin.ContentManagement.News.NewsItems.Fields.MetaTitle",
                "Admin.ContentManagement.News.NewsItems.Fields.MetaTitle.Hint",
                "Admin.ContentManagement.Topics.Fields.MetaTitle",
                "Admin.ContentManagement.Topics.Fields.MetaTitle.Hint");

            builder.Delete("Admin.Catalog.Categories.Fields.MetaDescription",
                "Admin.Catalog.Categories.Fields.MetaDescription.Hint",
                "Admin.Catalog.Manufacturers.Fields.MetaDescription",
                "Admin.Catalog.Manufacturers.Fields.MetaDescription.Hint",
                "Admin.Catalog.Products.Fields.MetaDescription",
                "Admin.Catalog.Products.Fields.MetaDescription.Hint",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.MetaDescription",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.MetaDescription.Hint",
                "Admin.ContentManagement.News.NewsItems.Fields.MetaDescription",
                "Admin.ContentManagement.News.NewsItems.Fields.MetaDescription.Hint",
                "Admin.ContentManagement.Topics.Fields.MetaDescription",
                "Admin.ContentManagement.Topics.Fields.MetaDescription.Hint");

            builder.Delete("Admin.Catalog.Categories.Fields.MetaKeywords",
                "Admin.Catalog.Categories.Fields.MetaKeywords.Hint",
                "Admin.Catalog.Manufacturers.Fields.MetaKeywords",
                "Admin.Catalog.Manufacturers.Fields.MetaKeywords.Hint",
                "Admin.Catalog.Products.Fields.MetaKeywords",
                "Admin.Catalog.Products.Fields.MetaKeywords.Hint",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.MetaKeywords",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.MetaKeywords.Hint",
                "Admin.ContentManagement.News.NewsItems.Fields.MetaKeywords",
                "Admin.ContentManagement.News.NewsItems.Fields.MetaKeywords.Hint",
                "Admin.ContentManagement.Topics.Fields.MetaKeywords",
                "Admin.ContentManagement.Topics.Fields.MetaKeywords.Hint",
                "Admin.ContentManagement.Blog.BlogPosts.Fields.Language",
                "Admin.ContentManagement.News.NewsItems.Fields.Language");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.DefaultTitle",
                "Default title tag",
                "Standard-Titel-Tag",
                "Defines the default title for pages in your store. You can override this for individual categories, products, manufacturer and topic pages.",
                "Legt das Standard-Titel-Tag für Seiten im Shop fest. Es kann für Warengruppen, Produkte, Hersteller und Seiten individuell angegeben werden.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.DefaultMetaDescription",
                "Default meta description",
                "Standard Meta Beschreibung",
                "Defines the default meta description for pages in your store. You can override this for individual categories, products, manufacturer and topic pages.",
                "Legt die Meta Beschreibung für Seiten im Shop fest. Diese kann für Warengruppen, Produkte, Hersteller und Seiten individuell angegeben werden.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.DefaultMetaKeywords",
                "Default meta keywords",
                "Standard Meta Keywords",
                "Defines the default meta keywords for pages in your store. You can override these for individual categories, products, manufacturer and topic pages.",
                "Legt die Meta Keywords für Seiten im Shop fest. Für Warengruppen, Produkte, Hersteller und Seiten können diese individuell angegeben werden.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowDiscountSign",
                "Show discount sign",
                "Rabattzeichen anzeigen",
                "Specifies whether a discount sign should be displayed on product pictures when discounts were applied.",
                "Legt fest, ob ein Rabattzeichen auf dem Produktbild angezeigt werden soll, wenn Rabatte angewendet wurden.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.StateProvinceRequired",
                "'State/province' required",
                "'Bundesland' erferderlich",
                "Check the box if 'State/province' is required.",
                "Legt fest, ob Angaben im Eingabefeld 'Adresszusatz' erforderlich sind.");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.AttributeChoiceBehaviour.GrayOutUnavailable",
                "Gray out unavailable attributes",
                "Nicht verfügbare Attribute ausgrauen");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.AttributeChoiceBehaviour.None",
                "None",
                "Keines");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.AttributeChoiceBehaviour",
                "Attribute choice behaviour",
                "Attribut-Auswahlverhalten",
                "Specifies the behaviour when selecting attributes.",
                "Legt das Verhalten bei der Auswahl von Attributen fest.");

            builder.AddOrUpdate("Admin.Catalog.Products.ProductVariantAttributes.Attributes.Fields.CustomData",
                "Custom data",
                "Benutzerdefinierte Daten",
                "Specifies user-defined data. For free usage, e.g. for individual shop extensions.",
                "Legt benutzerdefinierte Daten fest. Zur freien Verwendung, z.B. bei individuellen Shop-Erweiterungen.");

            #region Media

            var mediaRoot = "Admin.Configuration.Settings.Media.";

            builder.AddOrUpdate(mediaRoot + "ImageSizes", "Image/thumbnail sizes", "Bild-/Thumbnailgrößen");

            builder.AddOrUpdate(mediaRoot + "DefaultPictureZoomEnabled",
                "Enable image zoom",
                "Bildzoom aktivieren",
                "Adds 'zoom on hover' functionality to product detail images.",
                "Aktiviert 'Zoom bei Hover'-Funktion für Produktdetail-Bilder.");

            builder.AddOrUpdate(mediaRoot + "AssociatedProductPictureSize", "Associated (grouped) product", "Verknüpftes (Gruppen)-Produkt");
            builder.AddOrUpdate(mediaRoot + "AvatarPictureSize", "Customer avatar", "Kundenavatar");
            builder.AddOrUpdate(mediaRoot + "BundledProductPictureSize", "Bundle items", "Bundle Stückliste");
            builder.AddOrUpdate(mediaRoot + "CartThumbBundleItemPictureSize", "Bundle items in cart", "Bundle Stückliste im Warenkorb");
            builder.AddOrUpdate(mediaRoot + "CartThumbPictureSize", "Cart", "Warenkorb");
            builder.AddOrUpdate(mediaRoot + "CategoryThumbPictureSize", "Categories", "Warengruppen");
            builder.AddOrUpdate(mediaRoot + "ManufacturerThumbPictureSize", "Brands", "Marken");
            builder.AddOrUpdate(mediaRoot + "MessageProductThumbPictureSize", "Products in e-mails", "Produkte in E-Mails");
            builder.AddOrUpdate(mediaRoot + "MiniCartThumbPictureSize", "Mini cart", "Mini-Warenkorb");
            builder.AddOrUpdate(mediaRoot + "ProductDetailsPictureSize", "Product detail", "Produktdetail");
            builder.AddOrUpdate(mediaRoot + "ProductThumbPictureSize", "Product list", "Produktliste");
            builder.AddOrUpdate(mediaRoot + "ProductThumbPictureSizeOnProductDetailsPage", "Gallery thumbnail", "Gallerie-Thumbnail");

            builder.Delete(
                mediaRoot + "AssociatedProductPictureSize.Hint",
                mediaRoot + "AvatarPictureSize.Hint",
                mediaRoot + "BundledProductPictureSize.Hint",
                mediaRoot + "CartThumbBundleItemPictureSize.Hint",
                mediaRoot + "CartThumbPictureSize.Hint",
                mediaRoot + "CategoryThumbPictureSize.Hint",
                mediaRoot + "ManufacturerThumbPictureSize.Hint",
                mediaRoot + "MessageProductThumbPictureSize.Hint",
                mediaRoot + "MiniCartThumbPictureSize.Hint",
                mediaRoot + "ProductDetailsPictureSize.Hint",
                mediaRoot + "ProductThumbPictureSize.Hint",
                mediaRoot + "ProductThumbPictureSizeOnProductDetailsPage.Hint");

            #endregion

            #region Customer settings

            builder.Delete("Admin.Configuration.Settings.CustomerUser.AddressFormFields.TitleEnabled",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.TitleEnabled.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.CompanyEnabled",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.CompanyEnabled.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.CompanyRequired",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.CompanyRequired.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddressEnabled",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddressEnabled.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddressRequired",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddressRequired.Hint",
                "Admin.Configuration.Settings.CustomerUser.StreetAddress2Enabled",
                "Admin.Configuration.Settings.CustomerUser.StreetAddress2Enabled.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Enabled",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Enabled.Hint",
                "Admin.Configuration.Settings.CustomerUser.StreetAddress2Required",
                "Admin.Configuration.Settings.CustomerUser.StreetAddress2Required.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Required",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Required.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.ZipPostalCodeEnabled",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.ZipPostalCodeEnabled.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.ZipPostalCodeRequired",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.ZipPostalCodeRequired.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.CityEnabled",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.CityEnabled.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.CityRequired",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.CityRequired.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountryEnabled",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountryEnabled.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountryRequired",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountryRequired.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceEnabled",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceEnabled.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceRequired",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceRequired.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.PhoneEnabled",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.PhoneEnabled.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.PhoneRequired",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.PhoneRequired.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.FaxEnabled",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.FaxEnabled.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.FaxRequired",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.FaxRequired.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.SalutationEnabled",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.SalutationEnabled.Hint",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.Salutations",
                "Admin.Configuration.Settings.CustomerUser.AddressFormFields.Salutations.Hint");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.GenderEnabled",
                "'Gender' enabled",
                "\"Geschlecht\" aktiv",
                "Defines whether the input of 'Gender' is enabled.",
                "Legt fest, ob das Eingabefeld \"Geschlecht\" aktiviert ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.TitleEnabled",
                "'Title' enabled",
                "\"Titel\" aktiv",
                "Defines whether the input of 'Title' is enabled.",
                "Legt fest, ob das Eingabefeld \"Titel\" aktiviert ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CompanyEnabled",
                "'Company' enabled",
                "\"Firma\" aktiv",
                "Defines whether the input of 'Company' is enabled.",
                "Legt fest, ob das Eingabefeld \"Firma\" aktiviert ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CompanyRequired",
                "'Company' required",
                "\"Firma\" ist erforderlich",
                "Defines whether the input of 'Company' is required.",
                "Legt fest, ob die Eingabe von \"Firma\" erforderlich ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.StreetAddressEnabled",
                "'Street address' enabled",
                "\"Straße\" aktiv",
                "Defines whether the input of 'Street adress' is enabled.",
                "Legt fest, ob das Eingabefeld \"Straße\" aktiviert ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.StreetAddressRequired",
                "'Street address' required",
                "\"Straße\" ist erforderlich",
                "Defines whether the input of 'Street adress' is required.",
                "Legt fest, ob die Eingabe von \"Straße\" erforderlich ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.StreetAddress2Enabled",
                "'Street address addition' enabled",
                "\"Adressszusatz\" aktiv",
                "Defines whether the input of 'street address addition' is enabled.",
                "Legt fest, ob das Eingabefeld \"Adressszusatz\" aktiviert ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.StreetAddress2Required",
                "'Street address addition' required",
                "\"Adressszusatz\" ist erforderlich",
                "Defines whether the input of 'street address addition' is required.",
                "Legt fest, ob die Eingabe von \"Adressszusatz\" erforderlich ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.ZipPostalCodeEnabled",
                "'Zip / postal code' enabled",
                "\"Postleitzahl\" aktiv",
                "Defines whether the input of 'Zip / postal code' is enabled.",
                "Legt fest, ob das Eingabefeld \"Postleitzahl\" aktiviert ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.ZipPostalCodeRequired",
                "'Zip / postal code' required",
                "\"Postleitzahl\" ist erforderlich",
                "Defines whether the input of 'Zip / postal code' is required.",
                "Legt fest, ob die Eingabe von \"Postleitzahl\" erforderlich ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CityEnabled",
                "'City' enabled",
                "\"Stadt\" aktiv",
                "Defines whether the input of 'City' is enabled.",
                "Legt fest, ob das Eingabefeld \"Stadt\" aktiviert ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CityRequired",
                "'City' required",
                "\"Stadt\" ist erforderlich",
                "Defines whether the input of 'City' is required.",
                "Legt fest, ob die Eingabe von \"Stadt\" erforderlich ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CountryEnabled",
                "'Country' enabled",
                "\"Land\" aktiv",
                "Defines whether the input of 'Country' is enabled.",
                "Legt fest, ob das Eingabefeld \"Land\" aktiviert ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CountryRequired",
                "'Country' required",
                "\"Land\" ist erforderlich",
                "Defines whether the input of 'Country' is required.",
                "Legt fest, ob die Eingabe von \"Land\" erforderlich ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.StateProvinceEnabled",
                "'State / province' enabled",
                "\"Bundesland\" aktiv",
                "Defines whether the input of 'State / province' is enabled.",
                "Legt fest, ob das Eingabefeld \"Bundesland\" aktiviert ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.StateProvinceRequired",
                "'State / province' required",
                "\"Bundesland\" ist erforderlich",
                "Defines whether the input of 'State / province' is required.",
                "Legt fest, ob die Eingabe von \"Bundesland\" erforderlich ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.PhoneEnabled",
                "'Phone number' enabled",
                "\"Telefon\" aktiv",
                "Defines whether the input of 'Phone number' is enabled.",
                "Legt fest, ob das Eingabefeld \"Telefon\" aktiviert ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.PhoneRequired",
                "'Phone number' required",
                "\"Telefon\" ist erforderlich",
                "Defines whether the input of 'Phone number' is required.",
                "Legt fest, ob die Eingabe von \"Telefon\" erforderlich ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.FaxEnabled",
                "'Fax number' enabled",
                "\"Fax\" aktiv",
                "Defines whether the input of 'Fax number' is enabled.",
                "Legt fest, ob das Eingabefeld \"Fax\" aktiviert ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.FaxRequired",
                "'Fax number' required",
                "\"Fax\" ist erforderlich",
                "Defines whether the input of 'Fax number' is required.",
                "Legt fest, ob die Eingabe von \"Fax\" erforderlich ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.NewsletterEnabled",
                "'Newsletter' enabled",
                "\"Newsletter\" aktiv",
                "Displays the newsletter registration form.",
                "Zeigt das Newsletter Anmeldeformular an.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.SalutationEnabled",
               "'Salutation' enabled",
                "\"Anrede\" aktiv",
                "Defines whether the input of 'Salutation' is enabled.",
                "Legt fest, ob das Eingabefeld \"Anrede\" aktiviert ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.Salutations",
                "Salutations",
                "Anreden",
                "Comma separated list of salutations (e.g. Mr., Mrs). Define the entries which will populate the dropdown list for salutation when entering addresses.",
                "Komma getrennte Liste (z.B. Herr, Frau). Bestimmen Sie die Einträge für die Auswahl der Anrede, bei der Adresserfassung.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.DateOfBirthEnabled",
                "'Date of birth' enabled",
                "\"Geburtsdatum\" aktiv",
                "Defines whether the input of 'Date of birth' is enabled.",
                "Legt fest, ob das Eingabefeld \"Geburtsdatum\" aktiviert ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.LastNameRequired",
                "'Last name' required",
                "\"Nachname\" ist erforderlich",
                "Defines whether the input of 'Last name' is required.",
                "Legt fest, ob die Eingabe von \"Nachname\" erforderlich ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.FirstNameRequired",
                "'First name' required",
                "\"Vorname\" ist erforderlich",
                "Defines whether the input of 'First name' is required.",
                "Legt fest, ob die Eingabe von \"Vorname\" erforderlich ist.");

            #endregion Customer settings

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.SocialSettings.FacebookAppId",
                "Facebook App ID",
                "Facebook App ID",
                "A Facebook App ID is a unique number that identifies your application and connects it to your Facebook account. It is used when sharing products, category and manufacturer pages on Facebook.",
                "Eine Facebook App ID ist eine eindeutige Nummer, die Ihre Anwendung identifiziert und mit Ihrem Facebook-Account verknüpft. Wird beim Teilen von Produkten, Warengruppen- und Herstellerseiten auf Facebook genutzt.");

            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.SocialSettings.TwitterSite",
                "Twitter Username",
                "Twitter-Benutzername",
                "Twitter Username that gets displayed on Twitter cards when a product, category and manufacturer page is shared on Twitter. Starts with a '@'.",
                "Twitter-Benutzername, der auf Twitter-Karten angezeigt wird, wenn ein Produkt, eine Kategorie oder eine Herstellerseite auf Twitter geteilt wird. Beginnt mit einem '@'.");

            builder.AddOrUpdate("Admin.ContentManagement.Topics.Validation.NoEmptyPassword",
                "Password must have a value.",
                "Passwort muss einen Wert haben.");

            builder.AddOrUpdate("Admin.ContentManagement.Topics.Validation.NoPasswordAllowed",
                "HTML widgets cannot have a password protection.",
                "HTML Widgets können keinen Passwortschutz haben.");

            builder.AddOrUpdate("Order.CompletePayment.Hint",
                "Click 'Complete payment' if you have not yet paid and wish to start the payment process again.",
                "Klicken Sie 'Zahlung veranlassen', falls Sie noch nicht gezahlt haben und den Zahlungsvorgang erneut starten möchten.");

            builder.AddOrUpdate("Admin.ContentManagement.Forums.ForumGroup.Updated")
                .Value("de", "Die Forengruppe wurde erfolgreich aktualisiert.");

            builder.AddOrUpdate("Account.Login.NotRegisteredYet",
                "Not registered yet?",
                "Noch nicht registriert?");
        }
    }
}

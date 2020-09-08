namespace SmartStore.Data.Migrations
{
	using System;
    using System.Linq;
	using System.Data.Entity.Migrations;
	using Setup;
    using SmartStore.Core.Data;
    using SmartStore.Utilities;
    using SmartStore.Core.Domain.Media;
    using SmartStore.Core.Domain.Configuration;

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
			// Add .ico extension to MediaSettings.ImageTypes
			var name = TypeHelper.NameOf<MediaSettings>(y => y.ImageTypes, true);
			var setting = context.Set<Setting>().FirstOrDefault(x => x.Name == name);
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
				"Legt den standardmäßigen Mindestbestellwert für den Shop fest. Wird von Kundengruppenbeschränkungen überschrieben.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Order.MaxOrderTotal",
				"Maximum order total",
				"Höchstbestellwert",
				"Defines the default maximum order total for the shop. Overridden by customer group restrictions.",
				"Legt den standardmäßigen Höchstbestellwert für den Shop fest. Wird von Kundengruppenbeschränkungen überschrieben.");

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
					Wenn Sie Scripte von Drittanbietern in Ihren Shop eingebunden haben, die Cookies nutzen und die nicht über ein Plugin in Smartstore integriert sind,
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
				"Meta title",
				"Meta Titel");

			builder.AddOrUpdate("Admin.Configuration.Seo.MetaDescription",
				"Meta description",
				"Meta Beschreibung");

			builder.AddOrUpdate("Admin.Configuration.Seo.MetaKeywords",
				"Meta keywords",
				"Meta Keywords");
		}
	}
}

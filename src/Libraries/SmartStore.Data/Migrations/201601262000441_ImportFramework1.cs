namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using Setup;

	public partial class ImportFramework1 : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            AddColumn("dbo.ImportProfile", "UpdateOnly", c => c.Boolean(nullable: false));
            AddColumn("dbo.ImportProfile", "KeyFieldNames", c => c.String(maxLength: 1000));
            AddColumn("dbo.ImportProfile", "ResultInfo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ImportProfile", "ResultInfo");
            DropColumn("dbo.ImportProfile", "KeyFieldNames");
            DropColumn("dbo.ImportProfile", "UpdateOnly");
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
			builder.AddOrUpdate("Admin.DataExchange.Import.MultipleFilesSameFileTypeNote",
				"For multiple import files please make sure that they are of the same file type and that the content follows the same pattern (e.g. same column headings).",
				"Bei mehreren Importdateien ist darauf zu achten, dass diese vom selben Dateityp sind und deren Inhalt demselben Schema folgt (z.B. gleiche Spaltenüberschriften).");

			builder.AddOrUpdate("Admin.DataExchange.Import.ProfileEntitySelectNote",
				"Please select an object that you want to import.",
				"Wählen Sie bitte ein Objekt aus, das Sie importieren möchten.");

			builder.AddOrUpdate("Admin.DataExchange.Import.ProfileCreationNote",
				"Please upload an import file, enter a meaningful name for the import profile and save.",
				"Laden Sie bitte eine Importdatei hoch, legen Sie einen aussagekräftigen Namen für das Importprofil fest und speichern Sie.");

			builder.AddOrUpdate("Admin.DataExchange.Import.AddAnotherFile",
				"Add import file...",
				"Importdatei hinzufügen...");

			builder.AddOrUpdate("Admin.System.ScheduleTasks.RunNow.Progress.DataImportTask",
				"The task is now running in the background. You will receive an email as soon as it is completed. The progress can be tracked in the import profile list.",
				"Die Aufgabe wird jetzt im Hintergrund ausgeführt. Sie erhalten eine E-Mail, sobald sie abgeschlossen ist. Den Fortschritt können Sie in der Importprofilliste verfolgen.");

			builder.AddOrUpdate("Admin.System.ScheduleTasks.RunNow.Progress.DataExportTask",
				"The task is now running in the background. You will receive an email as soon as it is completed. The progress can be tracked in the export profile list.",
				"Die Aufgabe wird jetzt im Hintergrund ausgeführt. Sie erhalten eine E-Mail, sobald sie abgeschlossen ist. Den Fortschritt können Sie in der Exportprofilliste verfolgen.");

			builder.AddOrUpdate("Admin.DataExchange.Import.DefaultProfileNames",
				"My product import;My category import;My customer import;My newsletter subscription import",
				"Mein Produktimport;Mein Warengruppenimport;Mein Kundenimport;Mein Newsletter-Abonnement-Import");

			builder.AddOrUpdate("Admin.DataExchange.Import.LastImportResult",
				"Last import result",
				"Letztes Importergebnis");

			builder.AddOrUpdate("Admin.Common.TotalRows", "Total rows", "Zeilen insgesamt");
			builder.AddOrUpdate("Admin.Common.Skipped", "Skipped", "Ausgelassen");
			builder.AddOrUpdate("Admin.Common.NewRecords", "New records", "Neue Datensätze");
			builder.AddOrUpdate("Admin.Common.Updated", "Updated", "Aktualisiert");
			builder.AddOrUpdate("Admin.Common.Warnings", "Warnings", "Warnungen");
			builder.AddOrUpdate("Admin.Common.Errors", "Errors", "Fehler");
			builder.AddOrUpdate("Admin.Common.UnsupportedEntityType", "Unsupported entity type '{0}'", "Nicht unterstützter Entitätstyp '{0}'");
			builder.AddOrUpdate("Admin.Common.DataExchange", "Data exchange", "Datenaustausch");

			builder.AddOrUpdate("Admin.DataExchange.Import.CompletedEmail.Body",
				"This is an automatic notification of store \"{0}\" about a recent data import. Summary:",
				"Dies ist eine automatische Benachrichtung von Shop \"{0}\" über einen erfolgten Datenimport. Zusammenfassung:");

			builder.AddOrUpdate("Admin.DataExchange.Import.CompletedEmail.Subject",
				"Import of \"{0}\" has been finished",
				"Import von \"{0}\" ist abgeschlossen");

			builder.AddOrUpdate("Admin.DataExchange.Import.ColumnMapping",
				"Assignment of import fields",
				"Zuordnung der Importfelder");

			builder.AddOrUpdate("Admin.DataExchange.Import.SelectTargetProperty",
				"Create new assignment here",
				"Hier neue Zuordnung vornehmen");

			builder.AddOrUpdate("Admin.DataExchange.Import.UpdateOnly",
				"Only update",
				"Nur aktualisieren",
				"If this option is enabled, only existing data is updated but no new records are added.",
				"Ist diese Option aktiviert, werden nur vorhandene Daten aktualisiert, aber keine neue Datensätze hinzugefügt.");

			builder.AddOrUpdate("Admin.DataExchange.Import.KeyFieldNames",
				"Key fields",
				"Schlüsselfelder",
				"Existing records can be identified for updates on the basis of key fields. The key fields are processed in the order how they are defined here.",
				"Anhand von Schlüsselfeldern können vorhandene Datensätze zwecks Aktualisierung identifiziert werden. Die Schlüsselfelder werden in der hier festgelegten Reihenfolge verarbeitet.");

			builder.AddOrUpdate("Admin.DataExchange.Import.Validate.OneKeyFieldRequired",
				"At least one key field is required.",
				"Es ist mindestens ein Schlüsselfeld erforderlich.");

			builder.AddOrUpdate("Admin.DataExchange.ColumnMapping.Validate.MultipleMappedIgnored",
				"The following object properties were multiple assigned and thus ignored: {0}",
				"Die folgenden Objekteigenschaft wurden mehrfach zugeodnet und deshalb ignoriert: {0}");

			builder.AddOrUpdate("Admin.DataExchange.ColumnMapping.Validate.MappingsReset",
				"The stored field assignments are invalid due to the change of the delimiter and were reset.",
				"Die gespeicherten Feldzuordnungen sind aufgrund der Änderung des Trennzeichens ungültig und wurden zurückgesetzt.");


			builder.AddOrUpdate("Common.Download.NoDataAvailable",
				"Download data is not available anymore.",
				"Es sind keine Daten zum Herunterladen mehr verfügbar.");

			builder.AddOrUpdate("Common.Download.NotAvailable",
				"Download is not available any more.",
				"Der Download ist nicht mehr verfügbar.");

			builder.AddOrUpdate("Common.Download.SampleNotAvailable",
				"Sample download is not available anymore.",
				"Der Download einer Beispieldatei ist nicht mehr verfügbar.");

			builder.AddOrUpdate("Common.Download.HasNoSample",
				"The product variant doesn't have a sample download.",
				"Für die Produktvariante ist der Download einer Beispieldatei nicht verfügbar.");

			builder.AddOrUpdate("Common.Download.NotAllowed",
				"Downloads are not allowed.",
				"Downloads sind nicht gestattet.");

			builder.AddOrUpdate("Common.Download.MaxNumberReached",
				"You have reached the maximum number of downloads {0}.",
				"Sie haben die maximale Anzahl an Downloads {0} erreicht.");

			builder.AddOrUpdate("Account.CustomerOrders.NotYourOrder",
				"This is not your order.",
				"Dieser Auftrag konnte Ihnen nicht zugeordnet werden.");

			builder.AddOrUpdate("Shipping.CouldNotLoadMethod",
				"The shipping rate computation method could not be loaded.",
				"Die Berechnungsmethode für Versandkosten konnte nicht geladen werden.");

			builder.AddOrUpdate("Shipping.OneActiveMethodProviderRequired",
				"At least one shipping rate computation method provider is required to be active.",
				"Mindestens ein Provider zur Berechnung von Versandkosten muss aktiviert sein.");

			builder.AddOrUpdate("Payment.CouldNotLoadMethod",
				"The payment method could not be loaded.",
				"Die Zahlungsart konnte nicht geladen werden.");

			builder.AddOrUpdate("Payment.MethodNotAvailable",
				"The payment method is not available.",
				"Die Zahlungsart steht nicht zur Verfügung.");

			builder.AddOrUpdate("Payment.OneActiveMethodProviderRequired",
				"At least one payment method provider is required to be active.",
				"Mindestens ein Zahlungsart-Provider muss aktiviert sein.");

			builder.AddOrUpdate("Payment.RecurringPaymentNotSupported",
				"Recurring payments are not supported by selected payment method.",
				"Wiederkehrende Zahlungen sind für die gewählte Zahlungsart nicht möglich.");

			builder.AddOrUpdate("Payment.RecurringPaymentNotActive",
				"Recurring payment is not active.",
				"Wiederkehrende Zahlung ist inaktiv.");

			builder.AddOrUpdate("Payment.RecurringPaymentTypeUnknown",
				"The recurring payment type is not supported.",
				"Der Typ von wiederkehrender Zahlung wird nicht unterstützt.");

			builder.AddOrUpdate("Payment.CannotCalculateNextPaymentDate",
				"The next payment date could not be calculated.",
				"Das Datum der nächsten Zahlung kann nicht ermittelt werden.");

			builder.AddOrUpdate("Payment.PayingFailed",
				"Unfortunately we can not handle this purchasing via your preferred payment method. Please select an alternate payment option to complete your order.",
				"Leider können wir diesen Einkauf nicht über die gewünschte Zahlungsart abwickeln. Bitte wählen Sie eine alternative Zahlungsoption aus, um Ihre Bestellung abzuschließen.");

			builder.AddOrUpdate("Order.InitialOrderDoesNotExistForRecurringPayment",
				"No initial order exists for the recurring payment.",
				"Für die wiederkehrende Zahlung existiert kein Ausgangsauftrag.");

			builder.AddOrUpdate("Order.CannotCalculateShippingTotal",
				"The shipping total could not be calculated.",
				"Die Versandkosten konnten nicht berechnet werden.");

			builder.AddOrUpdate("Order.CannotCalculateOrderTotal",
				"The order total could not be calculated.",
				"Die Auftragssumme konnte nicht berechnet werden.");

			builder.AddOrUpdate("Order.BillingAddressMissing",
				"The billing address is missing.",
				"Die Rechnungsanschrift fehlt.");

			builder.AddOrUpdate("Order.ShippingAddressMissing",
				"The shipping address is missing.",
				"Die Lieferanschrift fehlt.");

			builder.AddOrUpdate("Order.CountryNotAllowedForBilling",
				"The country '{0}' is not allowed for billing.",
				"Eine Rechnungslegung ist für das Land '{0}' unzulässig.");

			builder.AddOrUpdate("Order.CountryNotAllowedForShipping",
				"The country '{0}' is not allowed for shipping.",
				"Ein Versand ist für das Land '{0}' unzulässig.");

			builder.AddOrUpdate("Order.NoRecurringProducts",
				"There are no recurring products.",
                "Keine Abonnements.");

			builder.AddOrUpdate("Order.NotFound",
				"The order {0} was not found.",
				"Der Auftrag {0} wurde nicht gefunden.");

			builder.AddOrUpdate("Order.CannotCancel",
				"Cannot cancel order.",
				"Der Auftrag kann nicht storniert werden.");

			builder.AddOrUpdate("Order.CannotMarkCompleted",
				"Cannot mark order as completed.",
				"Der Auftrag kann nicht als abgeschlossen markiert werden.");

			builder.AddOrUpdate("Order.CannotCapture",
				"Cannot capture order.",
				"Der Auftrag kann nicht gebucht werden.");

			builder.AddOrUpdate("Order.CannotMarkPaid",
				"Cannot mark order as paid.",
				"Der Auftrag kann nicht als bezahlt markiert werden.");

			builder.AddOrUpdate("Order.CannotRefund",
				"Cannot do refund for order.",
				"Eine Rückerstattung ist für diesen Auftrag nicht möglich.");

			builder.AddOrUpdate("Order.CannotPartialRefund",
				"Cannot do partial refund for order.",
				"Eine Teilrückerstattung ist für diesen Auftrag nicht möglich.");

			builder.AddOrUpdate("Order.CannotVoid",
				"Cannot do void for order.",
				"Eine Stornierung dieses Auftrages ist nicht möglich.");

			builder.AddOrUpdate("Shipment.AlreadyShipped",
				"This shipment is already shipped.",
				"Diese Sendung wird bereits ausgeliefert.");

			builder.AddOrUpdate("Shipment.AlreadyDelivered",
				"This shipment is already delivered.",
				"Diese Sendung wird bereits zugestellt.");

			builder.AddOrUpdate("Customer.DoesNotExist",
				"The customer does not exist.",
				"Der Kunde existiert nicht.");

			builder.AddOrUpdate("Checkout.AnonymousNotAllowed",
				"An anonymous checkout is not allowed.",
				"Ein anonymer Checkout ist nicht zulässig.");

			builder.AddOrUpdate("Common.Error.InvalidEmail",
				"The email address is not valid.",
				"Die E-Mail-Adresse ist ungültig.");

			builder.AddOrUpdate("Common.Error.NoActiveLanguage",
				"No active language could be loaded.",
				"Es wurde keine aktive Sprache gefunden.");

			builder.AddOrUpdate("Common.Error.NoEmailAccount",
				"No email account could be loaded.",
				"Es wurde kein E-Mail-Konto gefunden.");

			builder.AddOrUpdate("Admin.OrderNotice.RecurringPaymentCancellationError",
				"Unable to cancel recurring payment for order {0}.",
				"Es ist ein Fehler bei der Stornierung einer wiederkehrenden Zahlung für Auftrag {0} aufgetreten.");

			builder.AddOrUpdate("Admin.OrderNotice.OrderRefundError",
				"Unable to refund order {0}.",
				"Es ist ein Fehler bei einer Rückerstattung zu Auftrag {0} aufgetreten.");

			builder.AddOrUpdate("Admin.OrderNotice.OrderPartiallyRefundError",
				"Unable to partially refund order {0}.",
				"Es ist ein Fehler bei einer Teilerstattung zu Auftrag {0} aufgetreten.");

			builder.AddOrUpdate("Admin.OrderNotice.OrderVoidError",
				"Unable to void payment transaction of order {0}.",
				"Es ist ein Fehler bei der Stornierung einer Zahlungstransaktion zu Auftrag {0} aufgetreten.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.SortFilterResultsByMatches",
				"Sort filter results by number of matches",
				"Filterergebnisse nach Trefferanzahl sortieren",
				"Specifies to sort filter results by number of matches in descending order. If this option is deactivated then the result is sorted by the display order of the values.",
				"Legt fest, das Filterergebnisse absteigend nach der Anzahl an Übereinstimmungen sortiert werden. Ist diese Option deaktiviert, so wird in der für die Werte festgelegten Reihenfolge sortiert.");

			builder.AddOrUpdate("Wishlist.IsDisabled",
				"The wishlist is disabled.",
				"Die Wunschliste ist deaktiviert.");

			builder.AddOrUpdate("ShoppingCart.IsDisabled",
				"The shoping cart is disabled.",
				"Der Warenkorb ist deaktiviert.");

			builder.AddOrUpdate("Products.NotFound",
				"The product {0} was not found.",
				"Das Produkt {0} wurde nicht gefunden.");

			builder.AddOrUpdate("Products.Variants.NotFound",
				"The product variant {0} was not found.",
				"Die Produktvariante {0} wurde nicht gefunden.");

			builder.AddOrUpdate("Reviews.NotFound",
				"The product review {0} was not found.",
				"Die Produktbewertung {0} wurde nicht gefunden.");

			builder.AddOrUpdate("Polls.AnswerNotFound",
				"The poll answer {0} was not found.",
				"Eine Umfrageantwort {0} wurde nicht gefunden.");

			builder.AddOrUpdate("Polls.NotAvailable",
				"The poll is not available.",
				"Die Umfrage ist nicht verfügbar.");

			builder.AddOrUpdate("Install.LanguageNotRegistered",
				"The install language '{0}' is not registered.",
				"Die Installationssprache '{0}' ist nicht registriert.");

			builder.AddOrUpdate("Admin.Catalog.Categories.DescriptionToggle",
				"Show other description",
				"Andere Beschreibung anzeigen");

			builder.AddOrUpdate("Admin.Catalog.Categories.Fields.Description",
				"Top description",
				"Obere Beschreibung",
				"Description of the category that is displayed above products on the category page.",
				"Beschreibung der Warengruppe, die auf der Warengruppenseite oberhalb der Produkte angezeigt wird.");

			builder.AddOrUpdate("Common.CaptchaUnableToVerify",
				"The API call to verify a CAPTCHA has failed.",
				"Der API-Aufruf zur Prüfung eines CAPTCHAs ist fehlgeschlagen.");

			builder.AddOrUpdate("Common.WrongCaptcha",
				"Please confirm that you are not a \"robot\".",
				"Bitte bestätigen Sie, dass Sie kein \"Roboter\" sind.");

			builder.AddOrUpdate("DownloadableProducts.UserAgreementConfirmation",
				"Yes, I agree to the <a href='javascript:void(0)' data-id='{0}' class='download-user-agreement'>user agreement</a> for this product.",
				"Ja, ich stimme der <a href='javascript:void(0)' data-id='{0}' class='download-user-agreement'>Nutzungsvereinbarung</a> für dieses Produkt zu.");

			builder.AddOrUpdate("DownloadableProducts.HasNoUserAgreement",
				"The product has no user agreement.",
				"Das Produkt besitzt keine Nutzungsvereinbarung.");

			builder.AddOrUpdate("Checkout.DownloadUserAgreement.PleaseAgree",
				"Please agree to the user agreement for downloadable products.",
				"Bitte stimmen Sie der Nutzungsvereinbarung für herunterladbare Produkte zu.");


			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.OrderConfirmationPage",
				"Order confirmation page",
				"Bestellabschlussseite");

			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ShowEsdRevocationWaiverBox",
				"Show revocation waiver box for electronic services",
				"Widerrufsverzichtbox für elektronische Leistungen anzeigen",
				"Specifies whether the customer must agree a revocation waiver for electronic services on the order confirmation page.",
				"Legt fest, ob der Kunde auf der Bestellabschlussseite einem Widerrufsverzicht für elektronische Leistungen zustimmen muss.");

			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ShowCommentBox",
				"Show comment box",
				"Kommentarbox anzeigen",
				"Specifies whether comment box is displayed on the order confirmation page.",
				"Legt fest, ob der Kunde auf der Bestellabschlussseite einen Kommentar zu seiner Bestellung hinterlegen kann.");

			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ShowConfirmOrderLegalHint",
				"Show legal hints in order summary",
				"Rechtliche Hinweise in der Warenkorbübersicht anzeigen",
				"Specifies whether to show hints in order summary on the confirm order page. This text can be altered in the language resources.",
				"Legt fest, ob rechtliche Hinweise in der Warenkorbübersicht auf der Bestellabschlußseite angezeigt werden. Dieser Text kann in den Sprachresourcen geändert werden.");


			builder.AddOrUpdate("Checkout.EsdRevocationWaiverConfirmation",
				"Yes, I want access to the digital content immediately and know that my right of revocation expires with the access.",
				"Ja, ich möchte sofort Zugang zu dem digitalen Inhalt und weiß, dass mein Widerrufsrecht mit dem Zugang erlischt.");

			builder.AddOrUpdate("Checkout.EsdRevocationWaiverConfirmation.PleaseAgree",
				"Please confirm that you would like access to the digital content immediately.",
				"Bitte bestätigen Sie, dass Sie sofort Zugang zu dem digitalen Inhalt wünschen.");


			builder.AddOrUpdate("Admin.Configuration.Settings.DataExchange.MaxFileNameLength",
				"Maximum length of file and folder names",
				"Maximale Länge von Datei- und Ordnernamen",
				"Specifies the maximum length of file and folder names created during an import or export.",
				"Legt die maximale Länge von Datei- und Ordnernamen fest, die im Rahmen eines Imports\\Exports erzeugt wurden.");

			builder.AddOrUpdate("Admin.Configuration.Settings.DataExchange.ImageImportFolder",
				"Image folder (relative path)",
				"Bilderordner (relativer Pfad)",
				"Specifies a relative path to a folder with images to be imported (e.g. Content\\Images).",
				"Legt einen relativen Pfad zu einem Ordner mit zu importierenden Bildern fest (z.B. Inhalt\\Bilder).");

			builder.AddOrUpdate("Admin.Configuration.Settings.DataExchange.ImageDownloadTimeout",
				"Timeout for image download (minutes)",
				"Zeitlimit für Bilder-Download (Minuten)",
				"Specifies the timeout for the image download in minutes.",
				"Legt das Zeitlimit für den Bilder-Download in Minuten fest.");

			builder.AddOrUpdate("Admin.System.Maintenance.SqlQuery.Succeeded",
				"The SQL command was executed successfully.",
				"Die SQL Anweisung wurde erfolgreich ausgeführt.");

			builder.AddOrUpdate("Admin.DataExchange.Import.KeyFieldNames.Note",
				"Please only use the ID field as a key field, if the data sourced from the same database to which it will be imported. Otherwise it is possible that the wrong records are updated.",
				"Benutzen Sie das ID-Feld bitte nur dann als Schlüsselfeld, wenn die Daten aus der derselben Datenbank stammen, in der sie importiert werden sollen. Ansonsten werden u.U. die falschen Datensätze aktualisiert.");

			builder.AddOrUpdate("Admin.Configuration.Settings.RewardPoints.RoundDownRewardPoints",
				"Round down points",
				"Punkte abrunden",
				"Specifies whether to round down calculated points. Otherwise the bonus points will be rounded up.",
				"Legt fest, ob bei der Punkteberechnung abgerundet werden soll. Ansonsten werden Bonuspunkte aufgerundet.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Order.GiftCards_Deactivated",
				"Gift card deactivation order status",
				"Geschenkgutschein wird deaktiviert, wenn Auftragsstatus...");

			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.NewsLetterSubscription",
				"Subscribe to newsletters",
				"Abonnieren von Newslettern",
				"Specifies if customers can subscribe to newsletters when ordering and if the checkbox is enabled by default.",
				"Legt fest, ob Kunden bei einer Bestellung Newsletter abonnieren können und ob die Checkbox standardmäßig aktiviert ist.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Orders.CheckoutNewsLetterSubscription.None", "Do not show", "Nicht anzeigen");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Orders.CheckoutNewsLetterSubscription.Deactivated", "Show deactivated", "Deaktiviert anzeigen");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Orders.CheckoutNewsLetterSubscription.Activated", "Show activated", "Aktiviert anzeigen");

			builder.AddOrUpdate("Common.Options", "Options", "Optionen");

			builder.AddOrUpdate("Checkout.SubscribeToNewsLetter",
				"Subscribed to newsletter",
				"Newsletter abonnieren");

			builder.AddOrUpdate("Admin.OrderNotice.NewsLetterSubscriptionAdded",
				"Subscribed to newsletter",
				"Newsletter wurde abonniert");

			builder.AddOrUpdate("Admin.OrderNotice.NewsLetterSubscriptionRemoved",
				"Newsletter subscriber has been removed",
				"Newsletter-Abonnent wurde entfernt");

			builder.AddOrUpdate("Admin.Orders.Fields.CaptureTransactionID",
				"Capture transaction ID",
				"Transaktions-ID für Buchung",
				"Capture transaction identifier received from your payment gateway.",
				"Vom Zahlungsanbieter erhaltene Transaktions-ID für die Buchung.");

			builder.AddOrUpdate("Admin.Orders.Fields.AuthorizationTransactionID",
				"Authorization transaction ID",
				"Transaktions-ID für Autorisierung",
				"Authorization transaction identifier received from your payment gateway.",
				"Vom Zahlungsanbieter erhaltene Transaktions-ID für die Autorisierung.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.SearchDescriptions",
				"Search product description",
				"Produktbeschreibung durchsuchen",
				"Specifies whether the product description should be included in the search.",
				"Legt fest, ob die Produktbeschreibung in der Suche einbezogen werden soll.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Projection.NumberOfPictures",
				"Number of pictures",
				"Anzahl der Bilder",
				"Specifies the number of images per object to be exported.",
				"Legt die Anzahl der zu exportierenden Bilder pro Objekt fest.");
		}
	}
}

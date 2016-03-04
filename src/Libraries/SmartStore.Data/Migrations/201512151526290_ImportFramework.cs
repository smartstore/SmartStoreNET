namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using Core.Domain.Customers;
	using Core.Domain.Security;
	using Core.Domain.Seo;
	using Setup;

	public partial class ImportFramework : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            CreateTable(
                "dbo.ImportProfile",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        FolderName = c.String(nullable: false, maxLength: 100),
                        FileTypeId = c.Int(nullable: false),
                        EntityTypeId = c.Int(nullable: false),
                        Enabled = c.Boolean(nullable: false),
                        Skip = c.Int(nullable: false),
                        Take = c.Int(nullable: false),
                        FileTypeConfiguration = c.String(),
                        ColumnMapping = c.String(),
                        SchedulingTaskId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ScheduleTask", t => t.SchedulingTaskId)
                .Index(t => t.SchedulingTaskId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ImportProfile", "SchedulingTaskId", "dbo.ScheduleTask");
            DropIndex("dbo.ImportProfile", new[] { "SchedulingTaskId" });
            DropTable("dbo.ImportProfile");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			var permissionMigrator = new PermissionMigrator(context);
			var activityLogMigrator = new ActivityLogTypeMigrator(context);

			permissionMigrator.AddPermission(new PermissionRecord
			{
				Name = "Admin area. Manage Imports",
				SystemName = "ManageImports",
				Category = "Configuration"
			}, new string[] { SystemCustomerRoleNames.Administrators });

			activityLogMigrator.AddActivityLogType("DeleteOrder", "Delete order", "Auftrag gelöscht");

			context.MigrateSettings(x =>
			{
				var seoSettings = new SeoSettings();
				x.Add("seosettings.seonamecharconversion", seoSettings.SeoNameCharConversion);
			});
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Common.RecordsSkip",
				"Skip",
				"Überspringen",
				"Specifies the number of records to be skipped.",
				"Legt die Anzahl der zu überspringenden Datensätze fest.");

			builder.AddOrUpdate("Common.Unknown", "Unknown", "Unbekannt");
			builder.AddOrUpdate("Common.Unavailable", "Unavailable", "Nicht verfügbar");
			builder.AddOrUpdate("Common.Language", "Language", "Sprache");
			builder.AddOrUpdate("Admin.Common.ImportFile", "Import file", "Importdatei");
			builder.AddOrUpdate("Admin.Common.ImportFiles", "Import files", "Importdateien");
			builder.AddOrUpdate("Admin.Common.CsvConfiguration", "CSV Configuration", "CSV Konfiguration");

			builder.AddOrUpdate("Admin.Common.RecordsTake",
				"Limit",
				"Begrenzen",
				"Specifies the maximum number of records to be processed.",
				"Legt die maximale Anzahl der zu verarbeitenden Datensätze fest.");

			builder.AddOrUpdate("Admin.Common.FileTypeMustEqual",
				"The file must be of the type {0}.",
				"Die Datei muss vom Typ {0} sein.");

			builder.AddOrUpdate("Admin.DataExchange.Import.NoProfiles",
				"There were no import profiles found.",
				"Es wurden keine Importprofile gefunden.");

			builder.AddOrUpdate("Admin.DataExchange.Import.Name",
				"Name of profile",
				"Name des Profils",
				"Specifies the name of the import profile.",
				"Legt den Namen des Importprofils fest.");

			builder.AddOrUpdate("Admin.DataExchange.Import.ProgressInfo",
				"{0} of {1} records processed",
				"{0} von {1} Datensätzen verarbeitet");


			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ImportEntityType.Product", "Product", "Produkt");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ImportEntityType.Customer", "Customer", "Kunde");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ImportEntityType.NewsLetterSubscription", "Newsletter Subscriber", "Newsletter Abonnent");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ImportEntityType.Category", "Category", "Warengruppe");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ImportFileType.CSV", "Delimiter separated values (.csv, .txt, .tab)", "Trennzeichen getrennte Werte (.csv, .txt, .tab)");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ImportFileType.XLSX", "Excel (.xlsx)", "Excel  (.xlsx)");

			builder.AddOrUpdate("Admin.DataExchange.Import.FileUpload",
				"Upload import file...",
				"Importdatei hochladen...");

			builder.AddOrUpdate("Admin.DataExchange.Import.MissingImportFile",
				"Please upload an import file.",
				"Bitte laden Sie eine Importdatei hoch.");

			builder.AddOrUpdate("Admin.DataExchange.Csv.QuoteAllFields",
				"Quote all fields",
				"Alle Felder in Anführungszeichen",
				"Specifies whether to set quotation marks around all field values.",
				"Legt fest, ob die Werte aller Felder in Anführungszeichen gestellt werden sollen.");

			builder.AddOrUpdate("Admin.DataExchange.Csv.TrimValues",
				"Trim values",
				"Überflüssige Leerzeichen entfernen",
				"Specifies whether to remove space characters at start and end of a field value.",
				"Legt fest, ob Leerzeichen am Anfang und am Ende eines Feldwertes entfernt werden sollen.");

			builder.AddOrUpdate("Admin.DataExchange.Csv.SupportsMultiline",
				"Supports multilines",
				"Mehrzeilen erlaubt",
				"Specifies whether field values with multilines are supported.",
				"Legt fest, ob mehrzeilige Feldwerte unterstützt werden.");

			builder.AddOrUpdate("Admin.DataExchange.Csv.Delimiter",
				"Delimiter",
				"Trennzeichen",
				"Specifies the field separator.",
				"Legt das zu verwendende Trennzeichen für die Felder fest.");

			builder.AddOrUpdate("Admin.DataExchange.Csv.Quote",
				"Quote character",
				"Anführungszeichen",
				"Specifies the quotation character.",
				"Legt das zu verwendende Anführungszeichen fest.");

			builder.AddOrUpdate("Admin.DataExchange.Csv.Escape",
				"Inner quote character",
				"Inneres Anführungszeichen",
				"Specifies the inner quote character used for escaping.",
				"Legt das innere Anführungszeichen (Escaping) fest.");

			builder.AddOrUpdate("Admin.DataExchange.Csv.Delimiter.Validation",
				"Please enter a valid delimiter.",
				"Geben Sie bitte ein gültiges Trennzeichen ein.");

			builder.AddOrUpdate("Admin.DataExchange.Csv.Quote.Validation",
				"Please enter a valid quote character.",
				"Geben Sie bitte ein gültiges Anführungszeichen ein.");

			builder.AddOrUpdate("Admin.DataExchange.Csv.Escape.Validation",
				"Please enter a valid inner quote character (escaping).",
				"Geben Sie bitte ein gültiges, inneres Anführungszeichen (Escaping) ein.");

			builder.AddOrUpdate("Admin.DataExchange.Csv.EscapeDelimiter.Validation",
				"Delimiter and inner quote character cannot be equal in CSV files.",
				"Trennzeichen und inneres Anführungszeichen können in CSV Dateien nicht gleich sein.");

			builder.AddOrUpdate("Admin.DataExchange.Csv.QuoteDelimiter.Validation",
				"Delimiter and quote character cannot be equal in CSV files.",
				"Trennzeichen und Anführungszeichen können in CSV Dateien nicht gleich sein.");


			builder.AddOrUpdate("Admin.Catalog.Products.Fields.BasePriceMeasureUnit", "Base price measure unit", "Grundpreis Maßeinheit");
			builder.AddOrUpdate("Admin.Catalog.Products.Fields.ApprovedRatingSum", "Approved rating sum", "Summe genehmigter Bewertungen");
			builder.AddOrUpdate("Admin.Catalog.Products.Fields.NotApprovedRatingSum", "Not approved rating sum", "Summe nicht genehmigter Bewertungen");
			builder.AddOrUpdate("Admin.Catalog.Products.Fields.ApprovedTotalReviews", "Approved total reviews", "Summe genehmigter Rezensionen");
			builder.AddOrUpdate("Admin.Catalog.Products.Fields.NotApprovedTotalReviews", "Not approved total reviews", "Summe nicht genehmigter Rezensionen");
			builder.AddOrUpdate("Admin.Catalog.Products.Fields.HasTierPrices", "Has tier prices", "Hat Staffelpreise");
			builder.AddOrUpdate("Admin.Catalog.Products.Fields.LowestAttributeCombinationPrice", "Lowest attribute combination price", "Niedrigster Attributkombinationspreis");
			builder.AddOrUpdate("Admin.Catalog.Products.Fields.HasDiscountsApplied", "Has discounts applied", "Hat angewendete Rabatte");

			builder.AddOrUpdate("Admin.Catalog.Categories.Fields.ParentCategory", "Parent category", "Übergeordnete Warengruppe");

			builder.AddOrUpdate("Admin.Customers.Customers.Fields.CustomerGuid", "Customer GUID", "Kunden GUID");
			builder.AddOrUpdate("Admin.Customers.Customers.Fields.PasswordSalt", "Password salt", "Passwort Salt");
			builder.AddOrUpdate("Admin.Customers.Customers.Fields.IsSystemAccount", "Is system account", "Ist Systemkonto");
			builder.AddOrUpdate("Admin.Customers.Customers.Fields.LastLoginDateUtc", "Last login date", "Letztes Login-Datum");

			builder.AddOrUpdate("Admin.Promotions.NewsLetterSubscriptions.Fields.NewsLetterSubscriptionGuid", "Subscription GUID", "Abonnement GUID");

			builder.AddOrUpdate("Admin.DataExchange.ColumnMapping.Note",
				"You can optionally set for each field of the import file whether and for which object property the data should be imported. Fields with equal names are always imported as long as they are not explicitly ignored. Not yet selected properties are highlighted in the selection list. It is also possible to define a default value which is applied when the import field is empty. Stored assignments becomes invalid and reset when the delimiter changes.",
				"Sie können optional für jedes Feld der Importdatei festlegen, ob und nach welcher Objekteigenschaft dessen Daten zu importieren sind. Gleichnamige Felder werden grundsätzlich immer importiert, sofern sie nicht explizit ignoriert werden sollen. Noch nicht ausgewählte Eigenschaften sind in der Auswahlliste hervorgehoben. Zudem ist die Angabe eines Standardwertes möglich, der angewendet wird, wenn das Importfeld leer ist. Durch Änderung des Trennzeichens werden gespeicherte Zuordnungen ungültig und zurückgesetzt.");

			builder.AddOrUpdate("Admin.DataExchange.ColumnMapping.ImportField", "Import Field", "Importfeld");
			builder.AddOrUpdate("Admin.DataExchange.ColumnMapping.EntityProperty", "Object property", "Eigenschaft des Objektes");
			builder.AddOrUpdate("Admin.DataExchange.ColumnMapping.DefaultValue", "Default Value", "Standardwert");


			builder.Delete(
				"Admin.DataExchange.Export.LastExecution",
				"Admin.DataExchange.Export.Offset",
				"Admin.DataExchange.Export.Limit",
				"Admin.Promotions.NewsLetterSubscriptions.ImportEmailsSuccess",
				"Admin.Common.ImportFromCsv",
				"Admin.Common.CsvFile",

				"Admin.Common.ImportFromExcel",
				"Admin.Common.ExcelFile",
				"Admin.Common.ImportFromExcel.InProgress",
				"Admin.Common.ImportFromExcel.LastResultTitle",
				"Admin.Common.ImportFromExcel.ProcessedCount",
				"Admin.Common.ImportFromExcel.QuickStats",
				"Admin.Common.ImportFromExcel.ActiveSince",
				"Admin.Common.ImportFromExcel.CancelPrompt",
				"Admin.Common.ImportFromExcel.Cancel",
				"Admin.Common.ImportFromExcel.Cancelled",
				"Admin.Common.ImportFromExcel.DownloadReport",
				"Admin.Common.ImportFromExcel.NoReportAvailable",

				"Admin.Configuration.ActivityLog.ActivityLog.Fields.ActivityLogTypeColumn",
				"Plugins.ExchangeRate.EcbExchange.SetCurrencyToEURO"
			);

			builder.AddOrUpdate("ActivityLog.DeleteOrder", "Deleted order {0}", "Auftrag {0} gelöscht");

			builder.AddOrUpdate("Admin.System.SystemCustomerNames.SearchEngine", "Search Engine", "Suchmaschine");
			builder.AddOrUpdate("Admin.System.SystemCustomerNames.BackgroundTask", "Background Task", "Geplante Aufgabe");
			builder.AddOrUpdate("Admin.System.SystemCustomerNames.PdfConverter", "PDF Converter", "PDF-Konvertierer");

			builder.AddOrUpdate("Admin.Configuration.ActivityLog.ActivityLog.Fields.CustomerSystemAccount",
				"Customer system account",
				"Kundensystemkonto",
				"Filters results by customer system accounts.",
				"Filtert Ergebnisse nach Kundenystemkonten.");

			builder.AddOrUpdate("Admin.Configuration.ActivityLog.ActivityLog.Fields.CustomerEmail",
				"Customer Email",
				"Kunden-E-Mail",
				"Filters results by customer email address.",
				"Filtert Ergebnisse nach E-Mail-Adresse der Kunden.");

			builder.AddOrUpdate("Admin.Configuration.Plugins.UnknownError",
				"An unknown error occurred when calling a plugin. Please refer to the following message for details.",
				"Beim Aufruf eines Plugins ist ein unbekannter Fehler aufgetreten. Details entnehmen Sie bitte der folgenden Meldung.");

			builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.AllowUnicodeCharsInUrls",
				"Allow unicode characters",
				"Unicode-Zeichen erlauben",
				"Check whether SEO names can contain letters that are classified as unicode characters.",
				"Legt fest, ob als Unicode-Zeichen eingestufte Buchstaben in SEO relevanten Namen erlaubt sind.");

			builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.SeoNameCharConversion",
				"Characters to be converted",
				"Zu konvertierende Zeichen",
				"Allows an individual conversion of characters for SEO name creation. Enter the old and the new character separated by a semicolon, e.g. ä;ae. Each entry has to be entered in a new line.",
				"Ermöglicht das individuelle Konvertieren von Zeichen bei der Erstellung SEO Namen. Geben Sie hier durch Semikolon getrennt das alte und das neue Zeichen ein, z.B. ä;ae. Jeder Eintrag muss in einer neuen Zeile erfolgen.");

			builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.TestSeoNameCreation",
				"Check string",
				"Zeichenkette prüfen",
				"Enter any string to check the SEO name creation. Changed settings must be saved before.",
				"Geben Sie eine beliebige Zeichenkette ein, um daraus den SEO Namen zu erstellen. Geänderte Einstellungen müssen zuvor gespeichert werden.");


			builder.AddOrUpdate("Admin.System.Warnings.NoPermissionsDefined",
				"There are no permissions defined.",
				"Es sind keine Zugriffsrechte festgelegt.");

			builder.AddOrUpdate("Admin.System.Warnings.NoCustomerRolesDefined",
				"There are no customer roles defined.",
				"Es sind keine Kundengruppen festgelegt.");

			builder.AddOrUpdate("Admin.System.Warnings.AccessDeniedToAnonymousRequest",
				"Access denied to anonymous request on {0}.",
				"Zugriffsverweigerung durch anonyme Anfrage bei {0}.");

			builder.AddOrUpdate("Admin.System.Warnings.AccessDeniedToUser",
				"Access denied to user #{0} '{1}' on {2}.",
				"Zugriffsverweigerung durch Kunde #{0} '{1}' bei {2}.");

			builder.AddOrUpdate("Admin.Configuration.Countries.CannotDeleteDueToAssociatedAddresses",
				"The country cannot be deleted because it has associated addresses.",
				"Das Land kann nicht gelöscht werden, weil ihm Adressen zugeordnet sind.");

			builder.AddOrUpdate("Admin.Configuration.Countries.States.CantDeleteWithAddresses",
				"The state\\province cannot be deleted because it has associated addresses.",
				"Das Bundesland\\Region kann nicht gelöscht werden, weil ihm Adressen zugeordnet sind.");

			builder.AddOrUpdate("Admin.Configuration.Shipping.Methods.NoMethodsLoaded",
				"No shipping methods could be loaded.",
				"Es konnten keine Versandarten geladen werden.");

			builder.AddOrUpdate("Admin.System.Warnings.NoShipmentItems",
				"No shipment items",
				"Keine Versand-Artikel");

			builder.AddOrUpdate("Admin.System.Warnings.DigitsOnly",
				"Please enter digits only.",
				"Bitte nur Ziffern eingeben.");

			builder.AddOrUpdate("Account.Register.Errors.CannotRegisterSearchEngine",
				"A search engine can't be registered.",
				"Eine Suchmaschine kann nicht registriert werden.");

			builder.AddOrUpdate("Account.Register.Errors.CannotRegisterTaskAccount",
				"A background task account can't be registered.",
				"Das Konto einer geplanten Aufgabe kann nicht registriert werden.");

			builder.AddOrUpdate("Account.Register.Errors.AlreadyRegistered",
				"The customer is already registered.",
				"Der Kunde ist bereits registriert.");

			builder.AddOrUpdate("Admin.Customers.CustomerRoles.CannotFoundRole",
				"The customer role \"{0}\" cannot be found.",
				"Die Kundengruppe \"{0}\" wurde nicht gefunden.");

			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.RegisterCustomerRole",
				"Customer role at registrations",
				"Kundengruppe bei Registrierungen",
				"Specifies a customer role that will be assigned to newly registered customers.",
				"Legt eine Kundengruppe fest, die neu registrierten Kunden zugeordnet wird.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Order.DisplayOrdersOfAllStores",
				"Display orders of all stores",
				"Aufträge aller Shops anzeigen",
				"Specifies whether to display the orders of all stores to the customer. If this option is disabled, only the orders of the current store are displayed.",
				"Legt fest, ob dem Kunden die Aufträge aller Shops angezeigt werden sollen. Ist diese Option deaktiviert, so werden nur die Aufträge des aktuellen Shops angezeigt.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Order.GiftCards_Deactivated")
				.Value("de", "Geschenkgutschein wird deaktiviert, wenn Auftragsstatus...");

			builder.AddOrUpdate("Admin.Configuration.Languages.Fields.UniqueSeoCode.Required",
				"Please select a SEO language code.",
				"Bitte legen Sie einen SEO Sprach-Code fest.");

			builder.AddOrUpdate("Admin.Configuration.Languages.Fields.FlagImageFileName",
				"Flag image",
				"Flaggenbild",
				"Specifies the flag image. The files for the flag images must be stored in /Content/Images/flags/.",
				"Legt das Flaggenbild fest. Die Dateien der Flaggenbilder müssen in /Content/Images/flags/ liegen.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.HideCategoryDefaultPictures",
				"Hide default picture for categories",
				"Standardbild bei Warengruppen ausblenden",
				"Specifies whether to hide the default image for categories. The default image is shown when no image is assigned to a category.",
				"Legt fest, ob das Standardbild bei Warengruppen ausgeblendet werden soll. Das Standardbild wird angezeigt, wenn der Warengruppe kein Bild zugeordnet ist.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.HideProductDefaultPictures",
				"Hide default picture for products",
				"Standardbild bei Produkten ausblenden",
				"Specifies whether to hide the default image for products. The default image is shown when no image is assigned to a product.",
				"Legt fest, ob das Standardbild bei Produkten ausgeblendet werden soll. Das Standardbild wird angezeigt, wenn dem Produkt kein Bild zugeordnet ist.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Media.MessageProductThumbPictureSize",
				"Thumbnail size of products in emails",
				"Thumbnail-Größe von Produkten in E-Mails",
				"Specifies the thumbnail image size (pixels) of products in emails. Enter 0 to not display thumbnails.",
				"Legt die Thumbnail-Bildgröße (in Pixel) von Produkten in E-Mails fest. Geben Sie 0 ein, um keine Thumbnails anzuzeigen.");

			builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.MetaRobotsContent",
				"Meta robots",
				"Meta Robots",
				"Specifies if and how search engines indexing the pages of your store.",
				"Legt fest, ob und wie Suchmaschinen die Seiten Ihres Shops indexieren.");

			builder.AddOrUpdate("Providers.ExchangeRate.EcbExchange.SetCurrencyToEURO",
				"You can use ECB (European central bank) exchange rate provider only when exchange rate currency code is set to EURO.",
				"Der EZB-Wechselkursdienst kann nur genutzt werden, wenn der Wechselkurs-Währungscode auf EUR gesetzt ist.");


			builder.AddOrUpdate("Common.Loading", "Loading", "Lade");
			builder.AddOrUpdate("Common.ShowMore", "Show more", "Mehr anzeigen");
			builder.AddOrUpdate("Common.Published", "Published", "Veröffentlicht");
			builder.AddOrUpdate("Common.Unpublished", "Unpublished", "Unveröffentlicht");
			builder.AddOrUpdate("Common.NotSelectable", "Not selectable", "Nicht auswählbar");

			builder.AddOrUpdate("Common.EntityPicker.SinglePickNote",
				"Click on an item to select it and OK to apply it.",
				"Klicken Sie auf ein Element, um es auszuwählen und OK, um es zu übernehmen.");

			builder.AddOrUpdate("Common.EntityPicker.MultiPickNote",
				"Click on an item to select or deselect it and OK to apply the selection.",
				"Klicken Sie auf ein Element, um es aus- bzw. abzuwählen und OK, um die Auswahl zu übernehmen.");

			builder.AddOrUpdate("Common.EntityPicker.NoMoreItemsFound",
				"There were no more items found.",
				"Es wurden keine weiteren Elemente gefunden.");

			builder.AddOrUpdate("Admin.Catalog.Products.BundleItems.NotesOnProductBundles",
				"Notes on product bundles",
				"Hinweise zu Produkt-Bundles");

			builder.AddOrUpdate("Admin.Catalog.Products.RelatedProducts.AddNew",
				"Add cross-selling product",
				"Cross-Selling-Produkt hinzufügen");

			builder.AddOrUpdate("Admin.Catalog.Products.RelatedProducts.SaveBeforeEdit",
				"You need to save the product before you can add cross-selling products for this product page.",
				"Sie müssen das Produkt speichern, bevor Sie Cross-Selling-Produkte hinzufügen können.");

			builder.AddOrUpdate("Admin.Catalog.Products.CrossSells.AddNew",
				"Add checkout-selling product",
				"Checkout-Selling-Produkt hinzufügen");

			builder.AddOrUpdate("Admin.Catalog.Products.CrossSells.SaveBeforeEdit",
				"You need to save the product before you can add checkout-selling products for this product page.",
				"Sie müssen das Produkt speichern, bevor Sie Checkout-Selling-Produkte hinzufügen können.");
		}
	}
}

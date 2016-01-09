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

			builder.AddOrUpdate("Admin.DataExchange.Import.FileType",
				"File type",
				"Dateityp",
				"The file type of the import file(s).",
				"Der Dateityp der Importdatei(en).");

			builder.AddOrUpdate("Admin.DataExchange.Import.ProgressInfo",
				"{0} of {1} records processed",
				"{0} von {1} Datensätzen verarbeitet");


			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ImportEntityType.Product", "Product", "Produkt");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ImportEntityType.Customer", "Customer", "Kunde");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ImportEntityType.NewsLetterSubscription", "Newsletter Subscriber", "Newsletter Abonnent");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ImportEntityType.Category", "Category", "Warengruppe");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ImportFileType.CSV", "Delimiter separated values (.csv)", "Trennzeichen getrennte Werte (.csv)");
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
				"For each field of the import file you can optionally set whether and to which entity property the data is to be imported. It is also possible to define a default value which is applied when the import field is empty. Through <b>Clear</b> all made assignments are reset to their original values.",
				"Sie können optional für jedes Feld der Importdatei festlegen, ob und nach welcher Entitätseigenschaft dessen Daten importiert werden sollen. Zudem ist die Angabe eines Standardwertes möglich, der angewendet wird, wenn das Importfeld leer ist. Über <b>Zurücksetzen</b> werden alle getätigten Zuordnungen auf ihre Ursprungswerte zurückgesetzt.");

			builder.AddOrUpdate("Admin.DataExchange.ColumnMapping.ImportField", "Import Field", "Importfeld");
			builder.AddOrUpdate("Admin.DataExchange.ColumnMapping.EntityProperty", "Entity property", "Eigenschaft der Entität");
			builder.AddOrUpdate("Admin.DataExchange.ColumnMapping.DefaultValue", "Default Value", "Standardwert");

			builder.AddOrUpdate("Admin.DataExchange.ColumnMapping.Validate.EntityMultipleMapped",
				"The entity property \"{0}\" was assigned several times. Please assign each property only once.",
				"Die Entitätseigenschaft \"{0}\" wurde mehrfach zugeodnet. Bitte ordnen Sie jede Eigenschaft nur einmal zu.");


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
				"Admin.Configuration.ActivityLog.ActivityLog.Fields.ActivityLogTypeColumn"
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
		}
	}
}

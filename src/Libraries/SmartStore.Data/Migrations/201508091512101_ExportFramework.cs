namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using SmartStore.Core.Domain.Customers;
	using SmartStore.Core.Domain.Security;
	using SmartStore.Data.Setup;

	public partial class ExportFramework : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExportDeployment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProfileId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        Enabled = c.Boolean(nullable: false),
                        IsPublic = c.Boolean(nullable: false),
                        DeploymentTypeId = c.Int(nullable: false),
                        Username = c.String(maxLength: 400),
                        Password = c.String(maxLength: 400),
                        Url = c.String(maxLength: 4000),
                        FileSystemPath = c.String(maxLength: 400),
                        EmailAddresses = c.String(maxLength: 4000),
                        EmailSubject = c.String(maxLength: 400),
                        EmailAccountId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ExportProfile", t => t.ProfileId, cascadeDelete: true)
                .Index(t => t.ProfileId);
            
            CreateTable(
                "dbo.ExportProfile",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        FolderName = c.String(nullable: false, maxLength: 100),
                        ProviderSystemName = c.String(nullable: false, maxLength: 4000),
                        Enabled = c.Boolean(nullable: false),
                        SchedulingTaskId = c.Int(nullable: false),
                        Filtering = c.String(),
                        Projection = c.String(),
                        ProviderConfigData = c.String(),
                        Offset = c.Int(nullable: false),
                        Limit = c.Int(nullable: false),
                        BatchSize = c.Int(nullable: false),
                        PerStore = c.Boolean(nullable: false),
                        CompletedEmailAddresses = c.String(maxLength: 400),
                        CreateZipArchive = c.Boolean(nullable: false),
                        Cleanup = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ScheduleTask", t => t.SchedulingTaskId)
                .Index(t => t.SchedulingTaskId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExportDeployment", "ProfileId", "dbo.ExportProfile");
            DropForeignKey("dbo.ExportProfile", "SchedulingTaskId", "dbo.ScheduleTask");
            DropIndex("dbo.ExportProfile", new[] { "SchedulingTaskId" });
            DropIndex("dbo.ExportDeployment", new[] { "ProfileId" });
            DropTable("dbo.ExportProfile");
            DropTable("dbo.ExportDeployment");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			var permissionMigrator = new PermissionMigrator(context);

			permissionMigrator.AddPermission(new PermissionRecord
			{
				Name = "Admin area. Manage Exports",
				SystemName = "ManageExports",
				Category = "Configuration"
			}, new string[] { SystemCustomerRoleNames.Administrators });
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Common.Enabled", "Enabled", "Aktiviert");
			builder.AddOrUpdate("Common.Provider", "Provider", "Provider");
			builder.AddOrUpdate("Common.Profile", "Profile", "Profil");
			builder.AddOrUpdate("Common.Partition", "Partition", "Aufteilung");
			builder.AddOrUpdate("Common.Image", "Image", "Bild");
			builder.AddOrUpdate("Common.Filter", "Filter", "Filter");
			builder.AddOrUpdate("Common.Projection", "Projection", "Projektion");
			builder.AddOrUpdate("Common.Deployment", "Deployment", "Bereitstellung");
			builder.AddOrUpdate("Common.Website", "Website", "Web-Seite");
			builder.AddOrUpdate("Common.DetailDescription", "Detail description", "Detailbeschreibung");

			builder.AddOrUpdate("Admin.Validation.UsernamePassword", "Please enter username and password", "Bitte geben Sie Benutzername und Passwort ein");
			builder.AddOrUpdate("Admin.Validation.Url", "Please enter a valid URL", "Bitte geben Sie eine gültige URL ein");
			builder.AddOrUpdate("Admin.Validation.Name", "Please enter a name", "Bitte geben Sie einen Namen ein");
			builder.AddOrUpdate("Admin.Validation.EmailAddress", "Please enter a valid email address", "Bitte geben Sie eine gültige E-Mail Adresse ein");


			builder.AddOrUpdate("Admin.Configuration.Export.ProviderSystemName.Validate",
				"There were no export provider found for system name \"{0}\". A provider is mandatory for an export profile.",
				"Es wurde kein Export-Provider mit dem Systemnamen \"{0}\" gefunden. Ein Provider ist für ein Exportprofil zwingend erforderlich.");

			builder.AddOrUpdate("Admin.Configuration.Export.NoProfiles",
				"There were no export profiles found.",
				"Es wurden keine Exportprofile gefunden.");

			builder.AddOrUpdate("Admin.Configuration.Export.NoProviderConfigurationRequired",
				"The export provider <b>{0}</b> requires no further configuration.",
				"Der Export-Provider <b>{0}</b> benötigt keine weitergehende Konfiguration.");



			builder.AddOrUpdate("Admin.Configuration.Export.ProviderSystemName",
				"Provider",
				"Provider",
				"Specifies the export provider. It is responsible for the individual formatting of the export data.",
				"Legt den Export-Provider fest. Er ist für die individuelle Formatierung der zu exportierenden Daten zuständig.");

			builder.AddOrUpdate("Admin.Configuration.Export.EntityType",
				"Entity",
				"Entität",
				"The entity type the provider processes.",
				"Der Entitätstyp, den der Provider verarbeitet.");


			builder.AddOrUpdate("Admin.Configuration.Export.Name",
				"Name of profile",
				"Name des Profils",
				"Specifies the name of the export profile.",
				"Legt den Namen des Exportprofils fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.FileExtension",
				"File type",
				"Dateityp",
				"The file type of the exported data.",
				"Der Dateityp der exportierten Daten.");

			builder.AddOrUpdate("Admin.Configuration.Export.SchedulingHours",
				"Hours (interval)",
				"Stunden (Intervall)",
				"Specifies the interval in hours to which the export should execute automatically.",
				"Legt das Intervall in Stunden fest, zu dem der Export automatisch erfolgen soll.");

			builder.AddOrUpdate("Admin.Configuration.Export.LastExecution",
				"Last execution",
				"Letzte Ausführung",
				"Information about the last execution of the export.",
				"Informationen zur letzten Ausführung des Exports.");


			builder.AddOrUpdate("Admin.Configuration.Export.Offset",
				"Offset",
				"Abstand",
				"Specifies the number of records to be skipped.",
				"Legt die Anzahl der zu überspringenden Datensätze fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.Limit",
				"Limit",
				"Begrenzung",
				"Specifies how many records to be loaded at maximum.",
				"Legt die Anzahl der Datensätze fest, die maximal geladen werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Export.LimitOffsetValidation",
				"The offset must be less or equal to limit.",
				"Der Abstand muss kleiner oder gleich der Begrenzung sein.");

			builder.AddOrUpdate("Admin.Configuration.Export.BatchSize",
				"Batch size",
				"Stapelgröße",
				"Specifies the maximum number of records per export file. 0 is the default and means that all the records are exported in one file.",
				"Legt die maximale Anzahl der Datensätze pro Exportdatei fest. 0 ist der Standard und bedeutet, dass alle Datensätze in eine Datei exportiert werden.");

			builder.AddOrUpdate("Admin.Configuration.Export.PerStore",
				"Per store",
				"Per Shop",
				"Specifies whether to start a separate run-through for each store. For each shop a new file will be created.",
				"Legt fest, ob für jeden Shop ein separater Verarbeitungsdurchlauf erfolgen soll. Für jeden Shop wird eine neue Datei erzeugt.");

			builder.AddOrUpdate("Admin.Configuration.Export.CreateZipArchive",
				"Create ZIP archive",
				"ZIP-Archiv erstellen",
				"Specifies whether to combine and compress the export files in a ZIP archive.",
				"Legt fest, ob die Exportdateien in einem ZIP-Archiv zusammengefasst und komprimiert werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Export.CompletedEmailAddresses",
				"Notification (email addresses)",
				"Benachrichtigung (E-Mail-Addressen)",
				"Specifies the email addresses (semicolon separated) where to send a notification message of the completion of the export.",
				"Legt die E-Mail Addressen (Semikolon getrennt) fest, an die eine Benachrichtigung über die Fertigstellung des Exports verschickt werden soll.");

			builder.AddOrUpdate("Admin.Configuration.Export.Cleanup",
				"Clean up at the end",
				"Zum Schluss aufräumen",
				"Specifies whether to delete unneeded files after deployment.",
				"Legt fest, ob nicht mehr benötigte Dateien nach der Bereitstellung gelöscht werden sollen.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportEntityType.Product", "Product", "Produkt");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportEntityType.Category", "Category", "Warengruppe");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportEntityType.Manufacturer", "Manufacturer", "Hersteller");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportEntityType.Customer", "Customer", "Kunde");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportEntityType.Order", "Order", "Auftrag");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.StoreId",
				"Store",
				"Shop",
				"Filter by store.",
				"Nach Shop filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.CreatedFrom",
				"Created from",
				"Erstellt von",
				"Filter by created date.",
				"Nach dem Erstellungsdatum filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.CreatedTo",
				"Created to",
				"Erstellt bis",
				"Filter by created date.",
				"Nach dem Erstellungsdatum filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.PriceMinimum",
				"Price from",
				"Preis von",
				"Filter by price.",
				"Nach dem Preis filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.PriceMaximum",
				"Price to",
				"Preis bis",
				"Filter by price.",
				"Nach dem Preis filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.AvailabilityMinimum",
				"Availability from",
				"Verfügbar von",
				"Filter by availability quantity.",
				"Nach der Verfügbarkeitsmenge filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.AvailabilityMaximum",
				"Availability to",
				"Verfügbar bis",
				"Filter by availability quantity.",
				"Nach der Verfügbarkeitsmenge filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.IsPublished",
				"Published",
				"Veröffentlicht",
				"Filter by publishing.",
				"Nach Veröffentlichung filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.CategoryIds",
				"Categories",
				"Warengruppen",
				"Filter by categtories.",
				"Nach Warengruppen filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.WithoutCategories",
				"Without category mapping",
				"Ohne Warengruppenzuordnung",
				"Filter by missing category mapping.",
				"Nach fehlender Warengruppenzuordnung filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.ManufacturerIds",
				"Manufacturers",
				"Hersteller",
				"Filter by manufacturers.",
				"Nach Hersteller filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.WithoutManufacturers",
				"Without manufacturer mapping",
				"Ohne Herstellerzuordnung",
				"Filter by missing manufacturer mapping.",
				"Nach fehlender Herstellerzuordnung filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.ProductTagIds",
				"Product tags",
				"Produkt-Tags",
				"Filter by product tags.",
				"Nach Produkt-Tags filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.FeaturedProducts",
				"Only featured products",
				"Nur empfohlene Produkte",
				"Filter by featured products. Is only applied when the filtering by categories and manufacturers.",
				"Nach empfohlenen Produkten filtern. Wird nur bei der Filterung nach Warengruppen und Hersteller angewendet.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.ProductType",
				"Product type",
				"Produkttyp",
				"Filter by product type.",
				"Nach Produkttyp filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.OrderStatus",
				"Order status",
				"Auftragsstatus",
				"Filter by order status.",
				"Nach Auftragsstaus filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.PaymentStatus",
				"Payment status",
				"Zahlungsstatus",
				"Filter by payment status.",
				"Nach Zahlungsstatus filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.ShippingStatus",
				"Shipping status",
				"Versandstatus",
				"Filter by shipping status.",
				"Nach Versandstatus filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.CustomerRoleIds",
				"Customer roles",
				"Kundengruppen",
				"Filter by customer roles.",
				"Nach Kundengruppen filtern.");


			builder.AddOrUpdate("Admin.Configuration.Export.Projection.LanguageId",
				"Language",
				"Sprache",
				"Specifies the language to be applied to the export.",
				"Legt die auf den Export anzuwendende Sprache fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.Projection.CurrencyId",
				"Currency",
				"Währung",
				"Specifies the currency to be applied to the export.",
				"Legt die auf den Export anzuwendende Währung fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.Projection.CustomerId",
				"Customer ID",
				"Kunden-ID",
				"Specifies the ID of the customer to be applied to the export. Is taken into account for price calculations for example.",
				"Legt die ID des Kunden fest, auf den sich der Export beziehen soll. Wird z.B. bei Preisberechnungen berücksichtigt.");

			builder.AddOrUpdate("Admin.Configuration.Export.Projection.DescriptionMerging",
				"Product description",
				"Artikelbeschreibung",
				"Specifies what information to use for the description of the product.",
				"Legt fest, welche Informationen zur Beschreibung des Artikel wie verwendet werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Export.Projection.DescriptionToPlainText",
				"Remove HTML from description",
				"HTML aus der Beschreibung entfernen",
				"Specifies whether to remove all HTML from the product description for the export.",
				"Legt fest, ob für den Export alle HTML-Auszeichnungen aus der Artikelbeschreibung entfernt werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Export.Projection.AppendDescriptionText",
				"Text to be appended",
				"Anzuhängender Text",
				"Specifies the text to be attached to the product description. You can enter multiple, semicolon delimited texts. Then one of it is selected randomly.",
				"Legt den an die Artikelbeschreibung anzuhängenden Text fest. Es können auch mehrere, durch Semikolon getrennte Texte eingegeben werden. Dann wird per Zufall einer ausgewählt.");

			builder.AddOrUpdate("Admin.Configuration.Export.Projection.RemoveCriticalCharacters",
				"Remove critical characters",
				"Kritische Zeichen entfernen",
				"Specifies whether to remove critical characters (like ½) from the detail description.",
				"Legt fest, ob kritische Zeichen (wie z.B. ½) aus der Detailsbeschreibung entfernt werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Export.Projection.CriticalCharacters",
				"Critical characters",
				"Kritische Zeichen",
				"List with semicolon delimited characters to be removed from the detail description.",
				"Liste mit Semikolon getrennten Zeichen, die aus der Detailbeschreibung entfernt werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Export.Projection.PriceType",
				"Product price",
				"Produktpreis",
				"Specifies the product price to be exported.",
				"Legt den zu exportierenden Produktpreis fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.Projection.ConvertNetToGrossPrices",
				"Convert net into gross prices",
				"Netto- in Bruttopreise umrechnen",
				"Specifies to convert net into gross prices.",
				"Legt fest, dass Netto- in Bruttopreise umgerechnet werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Export.Projection.Brand",
				"Manufacturer\\Brand",
				"Hersteller\\Marke",
				"Specifies the manufacturer or brand to be exported, if a product has no manufacturer assigned.",
				"Legt den zu exportierenden Hersteller bzw. die Marke fest, wenn für ein Produkt kein Hersteller zugeordnet ist.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDeploymentType.FileSystem", "File system", "Dateisystem");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDeploymentType.Email", "Email", "E-Mail");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDeploymentType.Http", "HTTP", "HTTP");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDeploymentType.Ftp", "FTP", "FTP");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDescriptionMergingType.None",
				"None", "Keine");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDescriptionMergingType.ShortDescriptionOrNameIfEmpty",
				"Short description or name if empty", "Kurzbeschreibung oder Name falls leer");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDescriptionMergingType.ShortDescription",
				"Short description", "Kurzbeschreibung");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDescriptionMergingType.Description",
				"Description", "Detailbeschreibung");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDescriptionMergingType.NameAndShortDescription",
				"Product name + short description", "Produktname + Kurzbeschreibung");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDescriptionMergingType.NameAndDescription",
				"Product name + long description", "Produktname + Detailbeschreibung");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDescriptionMergingType.ManufacturerAndNameAndShortDescription",
				"Manufacturer + Product name + short description", "Hersteller + Produktname + Kurzbeschreibung");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDescriptionMergingType.ManufacturerAndNameAndDescription",
				"Manufacturer + Product name + long description", "Hersteller + Produktname + Detailbeschreibung");


			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.Name",
				"Name",
				"Name",
				"Specifies the name of the deployment.",
				"Legt den Namen der Bereitstellung fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.IsPublic",
				"Make public",
				"Öffentlich machen",
				"Specifies whether to publish the exported data (e.g. whether they are accessible on the internet).",
				"Legt fest, ob die exportierten Daten öffentlich gemacht werden sollen (z.B. ob sie über das Internet erreichbar sind).");

			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.DeploymentType",
				"Type of deployment",
				"Art der Bereitstellung",
				"Specifies the deployment type.",
				"Legt die Art Bereitstellung fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.Username",
				"User name",
				"Benutzername",
				"Specifies the user name.",
				"Legt den Benutzernamen fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.Password",
				"Password",
				"Passwort",
				"Specifies the password.",
				"Legt das Passwort fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.Url",
				"URL",
				"URL",
				"Specifies the URL on which the data should be deployed.",
				"Legt die URL fest, unter der die Daten bereitgestellt werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.FileSystemPath",
				"Relative path",
				"Relativer Pfad",
				"Specifies the relative path for the deployed data.",
				"Legt den relativen Pfad für die bereitgestellten Daten fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.EmailAddresses",
				"Email addresses",
				"E-Mail-Addressen",
				"Specifies the email addresses (semicolon separated) where to send the data.",
				"Legt die E-Mail Addressen (Semikolon getrennt) fest, an die die Daten verschickt werden soll.");

			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.EmailSubject",
				"Email subject",
				"E-Mail Betreff",
				"Specifies the subject of the data should be sent.",
				"Legt den Betreff der verschickten Daten fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.EmailAccountId",
				"Email account",
				"E-Mail Konto",
				"Specifies the email account through which the data should be sent.",
				"Legt das E-Mail Konto fest, über welches die Daten verschickt werden sollen.");
		}
    }
}

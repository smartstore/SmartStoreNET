namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;
	using SmartStore.Core.Domain.Media;
	using System.Linq;

	public partial class ThirdPartyEmailHandOver : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            AddColumn("dbo.Order", "AcceptThirdPartyEmailHandOver", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Order", "AcceptThirdPartyEmailHandOver");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			// Some users have disabled the "TransientMediaClearTask" due to a bug.
			// When the task is enabled again, it would delete media files, which are marked as transient
			// but are permanent actually. To avoid this, we have to set IsTransient to false.
			var transientPictures = context.Set<Picture>().Where(x => x.IsTransient == true).ToList();
			transientPictures.Each(x => x.IsTransient = false);

			var transientDownloads = context.Set<Download>().Where(x => x.IsTransient == true).ToList();
			transientDownloads.Each(x => x.IsTransient = false);

			context.SaveChanges();
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Common.Ignore", "Ignore", "Ignorieren");
			builder.AddOrUpdate("Common.My", "My", "Mein");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Orders.CheckoutThirdPartyEmailHandOver.None", "Do not show", "Nicht anzeigen");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Orders.CheckoutThirdPartyEmailHandOver.Deactivated", "Show deactivated", "Deaktiviert anzeigen");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Orders.CheckoutThirdPartyEmailHandOver.Activated", "Show activated", "Aktiviert anzeigen");

			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ThirdPartyEmailHandOver",
				"Third party email hand over",
				"E-Mail Weitergabe an Dritte",
				"Specifies if customers can accept to hand over their email address to third party when ordering and if the checkbox is enabled by default.",
				"Legt fest, ob Kunden bei einer Bestellung der Weitergabe Ihrer E-Mail Adresse an Dritte zustimmen können und ob die Checkbox standardmäßig aktiviert ist.");

			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ThirdPartyEmailHandOverLabel",
				"Text for email hand over",
				"Text für E-Mail Weitergabe",
				"Specifies the text to accept to hand over the email address to third party. Please choose a certain reason for the hand over, e.g. 'I agree with the transmission and storage of my email address for the trusted shops buyer protection.'",
				"Legt den Text für die Zustimmung zur Weitergabe der E-Mail Adresse an Dritte fest. Wählen Sie bitte einen konkreten Grund für die Weitergabe, z.B. 'Mit der Übermittlung und Speicherung meiner E-Mail-Adresse zur Abwicklung des Käuferschutzes durch Trusted Shops bin ich einverstanden.'");

			builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.ThirdPartyEmailHandOverLabel.Default",
				"I agree with the transmission and storage of my email address by third parties.",
				"Mit der Übermittlung und Speicherung meiner E-Mail-Adresse durch dritte Parteien bin ich einverstanden.");

			builder.AddOrUpdate("Admin.Orders.Fields.AcceptThirdPartyEmailHandOver",
				"Accepts hand over of email",
				"Akzeptiert Weitergabe der E-Mail",
				"Indicates whether the customer has accepted to hand over his email address to third party.",
				"Gibt an, ob der Kunde bei der Bestellung einer Weitergabe seiner E-Mail Adresse an Dritte zugestimmt hat oder nicht.");

			builder.AddOrUpdate("Admin.OrderNotice.OrderCaptureError",
				"Unable to capture payment for order {0}.",
				"Es ist ein Fehler bei der Zahlungsbuchung zu Auftrag {0} aufgetreten.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportDescriptionMerging.None",
				"None", "Keine");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowManufacturerPicturesInProductDetail",
				"Show manufacturer pictures",
				"Bilder von Herstellern anzeigen",
				"Specifies whether to show manufacturer pictures on product detail page.",
				"Legt fest, ob Herstellerbilder auf der Produktdetailseite angezeigt werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.HideManufacturerDefaultPictures",
				"Hide default picture for manufacturers",
				"Standardbild bei Herstellern ausblenden",
				"Specifies whether to hide the default image for manufacturers. The default image is shown when no image is assigned to a manufacturer.",
				"Legt fest, ob das Standardbild bei Herstellern ausgeblendet werden soll. Das Standardbild wird angezeigt, wenn dem Hersteller kein Bild zugeordnet ist.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Partition.Note",
				"You can partition the data to be exported with the following settings. This includes<ul><li>Skipping the first n records</li><li>The maximum number of records to be exported</li><li>The number of records per export file</li><li>Export data for each shop in a separate file</li></ul>By default, all data of a store will be exported into one file.",
				"Mit den folgenden Einstellungen lassen sich die zu exportierenden Daten aufteilen. Dazu zählt<ul><li>Das Überspringen der ersten n Datensätze</li><li>Die maximale Anzahl zu exportierender Datensätze</li><li>Die Anzahl der Datensätze pro Exportdatei</li><li>Daten von jedem Shop in eine separate Datei exportieren</li></ul>Standardmäßig werden alle Daten eines Shops in eine Datei exportiert.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Partition.Validate",
				"Partitioning settings must be greater than or equal to 0.",
				"Einstellungen zur Aufteilung müssen größer oder gleich 0 sein.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Filter.IsActiveSubscriber",
				"Only active subscriber",
				"Nur aktive Abonnenten",
				"Filter by active or inactive newsletter subscriber.",
				"Nach aktiven bzw. inaktiven Newsletter Abonnenten filtern.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Filter.IsActiveCustomer",
				"Only active customers",
				"Nur aktive Kunden",
				"Filter by active or inactive customers.",
				"Nach aktiven bzw. inaktiven Kunden filtern.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Filter.IsTaxExempt",
				"Only tax exempt customers",
				"Nur MwSt. befreite Kunden",
				"Filter by tax exempt customers.",
				"Nach MwSt. befreiten Kunden filtern.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Filter.BillingCountryIds",
				"Billing countries",
				"Rechnungsländer",
				"Filter by billing countries.",
				"Nach Rechnungsländern filtern.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Filter.ShippingCountryIds",
				"Shipping countries",
				"Versandländer",
				"Filter by shipping countries.",
				"Nach Versandländern filtern.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Filter.LastActivityFrom",
				"Last activity from",
				"Zuletzt aktiv von",
				"Filter by date of last store activity.",
				"Nach dem Datum der letzten Shop Aktivität filtern.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Filter.LastActivityTo",
				"Last activity to",
				"Zuletzt aktiv bis",
				"Filter by date of last store activity.",
				"Nach dem Datum der letzten Shop Aktivität filtern.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Filter.HasSpentAtLeastAmount",
				"Has spent x amount",
				"Hat Betrag x ausgegeben",
				"Filter by spent amount.",
				"Nach dem insgesamt ausgegebenen Betrag filtern.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Filter.HasPlacedAtLeastOrders",
				"Has x placed orders",
				"Hat x Bestellungen",
				"Filter by number of placed orders.",
				"Nach der Anzahl der getätigten Bestellungen filtern.");

			builder.AddOrUpdate("Admin.DataExchange.Export.SystemProfileNote",
				"The following list contains system profiles, which are provided by plugins such as the <a href='http://community.smartstore.com/index.php?/files/file/85-smartstorenet-common-export-providers/' target='_blank'>Data Export Plugin</a>. You can customize system profiles as desired, but cannot create new ones. These profiles also add additional action buttons. You will find these above data lists, such as the product or order list.",
				"Die folgende Liste enthält Systemprofile, die von Plugins wie bspw. dem <a href='http://community.smartstore.com/index.php?/files/file/85-smartstorenet-common-export-providers/' target='_blank'>Datenexporte Plugin</a> bereitgestellt werden. Sie können Systemprofile nach Belieben anpassen, aber keine Neuen erstellen. Für diese Profile stehen außerdem zusätzliche Aktions-Buttons zur Verfügung. Sie finden diese über den entsprechenden Listen, wie z.B. der Produkt- oder Auftragsliste.");

			builder.AddOrUpdate("Admin.DataExchange.AddNewProfile",
				"New profile",
				"Neues Profil");

			builder.AddOrUpdate("Admin.DataExchange.Import.ProfileCreationNote",
				"Please select the import object and upload an import file.",
				"Wählen Sie bitte das zu importierende Objekt und laden Sie eine Importdatei hoch.");

			builder.Delete("Admin.DataExchange.Import.ProfileEntitySelectNote");

			builder.AddOrUpdate("Admin.Configuration.Restriction.SaveBeforeEdit",
				"You need to save the data before you can specify restrictions.",
				"Sie müssen zunächst speichern, bevor Sie Einschränkungen festlegen können.");

			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CustomerNumberMethod",
				"Customer numbers",
				"Kundennummern",
				"Specifies whether to assign customer numbers and whether these should be created automatically.",
				"Legt fest, ob Kundennummern vergeben werden und ob diese automatisch vergeben werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CustomerNumberVisibility",
				"Customer number presentation",
				"Darstellung der Kundennummer",
				"Specifies the presentation and handling of the customer number to the customer.",
				"Legt die Darstellung und Handhabung der Kundennummer gegenüber dem Kunden fest.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.CustomerNumberMethod.Disabled", "Disabled", "Deaktiviert");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.CustomerNumberMethod.Enabled", "Enabled", "Aktiviert");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.CustomerNumberMethod.AutomaticallySet", "Automatically assigned", "Automatisch vergeben");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.CustomerNumberVisibility.None",	"Do not display", "Nicht anzeigen");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.CustomerNumberVisibility.Display", "Display", "Anzeigen");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.CustomerNumberVisibility.EditableIfEmpty", "Editable if empty", "Editierbar falls leer");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.CustomerNumberVisibility.Editable", "Always editable", "Stets editierbar");

			builder.AddOrUpdate("Admin.Common.FileInUse",
				"The file is in use and cannot be opened.",
				"Die Datei ist in Benutzung und kann nicht geöffnet werden.");


			builder.AddOrUpdate("Admin.DataExchange.Export.UserProfilesTitle",
				"User profiles",
				"Benutzerprofile");
			builder.AddOrUpdate("Admin.DataExchange.Export.SystemProfilesTitle",
				"System profiles",
				"Systemprofile");
		}
	}
}

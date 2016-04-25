namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class ExportFramework1 : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.ExportDeployment", "CreateZip", c => c.Boolean(nullable: false));
            AddColumn("dbo.ExportDeployment", "HttpTransmissionTypeId", c => c.Int(nullable: false));
            AddColumn("dbo.ExportDeployment", "HttpTransmissionType", c => c.Int(nullable: false));
            AddColumn("dbo.ExportDeployment", "PassiveMode", c => c.Boolean(nullable: false));
            AddColumn("dbo.ExportDeployment", "UseSsl", c => c.Boolean(nullable: false));
            AddColumn("dbo.ExportProfile", "FileNamePattern", c => c.String(maxLength: 400));
            AddColumn("dbo.ExportProfile", "EmailAccountId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExportProfile", "EmailAccountId");
            DropColumn("dbo.ExportProfile", "FileNamePattern");
            DropColumn("dbo.ExportDeployment", "UseSsl");
            DropColumn("dbo.ExportDeployment", "PassiveMode");
            DropColumn("dbo.ExportDeployment", "HttpTransmissionType");
            DropColumn("dbo.ExportDeployment", "HttpTransmissionTypeId");
            DropColumn("dbo.ExportDeployment", "CreateZip");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			context.Execute("DELETE FROM [dbo].[ScheduleTask] WHERE [Type] = 'SmartStore.Billiger.StaticFileGenerationTask, SmartStore.Billiger'");
			context.Execute("DELETE FROM [dbo].[ScheduleTask] WHERE [Type] = 'SmartStore.ElmarShopinfo.StaticFileGenerationTask, SmartStore.ElmarShopinfo'");
			context.Execute("DELETE FROM [dbo].[ScheduleTask] WHERE [Type] = 'SmartStore.Guenstiger.StaticFileGenerationTask, SmartStore.Guenstiger'");
			context.Execute("DELETE FROM [dbo].[ScheduleTask] WHERE [Type] = 'SmartStore.Shopwahl.StaticFileGenerationTask, SmartStore.Shopwahl'");

			context.MigrateSettings(x =>
			{
				x.DeleteGroup("BilligerSettings");
				x.DeleteGroup("ElmarShopinfoSettings");
				x.DeleteGroup("GuenstigerSettings");
				x.DeleteGroup("ShopwahlSettings");
			});
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Common.ExportSelected", "Export selected", "Ausgewählte exportieren");
			builder.AddOrUpdate("Admin.Common.ExportAll", "Export all", "Alle exportieren");
			builder.AddOrUpdate("Common.Public", "public", "öffentlich");

			builder.AddOrUpdate("Admin.System.ScheduleTask", "Scheduled task", "Geplante Aufgabe");

			builder.AddOrUpdate("Admin.DataExchange.Export.NoExportProvider",
				"There were no export provider found.",
				"Es wurden keine Export-Provider gefunden.");

			builder.AddOrUpdate("Admin.DataExchange.Export.ProgressInfo",
				"{0} of {1} records exported",
				"{0} von {1} Datensätzen exportiert");

			builder.AddOrUpdate("Admin.DataExchange.Export.FileNamePattern",
				"Pattern for file names",
				"Muster für Dateinamen",
				"Specifies the pattern for creating file names.",
				"Legt das Muster fest, nach dem Dateinamen erzeugt werden.");

			builder.AddOrUpdate("Admin.DataExchange.Export.EmailAccountId",
				"Email notification",
				"E-Mail Benachrichtigung",
				"Specifies the email account used to send a notification message of the completion of the export.",
				"Legt das E-Mail Konto fest, über welches eine Benachrichtigung über die Fertigstellung des Exports verschickt werden soll.");

			builder.AddOrUpdate("Admin.DataExchange.Export.CompletedEmailAddresses",
				"Email addresses to",
				"E-Mail Adressen an",
				"Specifies the email addresses where to send the notification message.",
				"Legt die E-Mail Adressen fest, an die die Benachrichtigung geschickt werden soll.");

			builder.AddOrUpdate("Admin.DataExchange.Export.CompletedEmail.Subject",
				"Export of profile \"{0}\" has been finished",
				"Export von Profil \"{0}\" ist abgeschlossen");

			builder.AddOrUpdate("Admin.DataExchange.Export.CompletedEmail.Body",
				"This is an automatic notification of store \"{0}\" about a recent data export.",
				"Dies ist eine automatische Benachrichtung von Shop \"{0}\" über einen erfolgten Datenexport.");

			builder.AddOrUpdate("Admin.DataExchange.Export.FolderName",
				"Folder name",
				"Ordnername",
				"Specifies the name of the folder where to export the data.",
				"Legt den Namen des Ordners fest, in den die Daten exportiert werden.");

			builder.AddOrUpdate("Admin.DataExchange.Export.FolderAndFileName.Validate",
				"Please enter a valid folder and file name. Example for file names: %File.Index%-%Profile.Id%-gmc-%Store.Name%",
				"Bitte einen gültigen Ordner- und Dateinamen eingeben. Beispiel für Dateinamen: %File.Index%-%Profile.Id%-gmc-%Store.Name%");


			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.CreateZip",
				"Create ZIP archive",
				"ZIP-Archiv erstellen",
				"Specifies whether to combine the export files in a ZIP archive and only to deploy the archive.",
				"Legt fest, ob die Exportdateien in einem ZIP-Archiv zusammengefasst und nur das Archiv bereitgestellt werden soll.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Projection.AttributeCombinationAsProduct",
				"Export attribute combinations",
				"Attributkombinationen exportieren",
				"Specifies whether to export a standalone product for each active attribute combination.",
				"Legt fest, ob für jede aktive Attributkombination ein eigenständiges Produkt exportiert werden soll.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Projection.AttributeCombinationValueMerging",
				"Attribute values",
				"Attributwerte",
				"Specifies if and how to further process the attribute values.",
				"Legt fest, ob und wie die Werte der Attribute weiter verarbeitet werden sollen.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportAttributeValueMerging.None",
				"Not specified", "Nicht spezifiziert");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportAttributeValueMerging.AppendAllValuesToName",
				"Append all values to the product name", "Alle Werte an den Produktnamen anhängen");

			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.HttpTransmissionType",
				"HTTP transmission type",
				"HTTP Übertragungsart",
				"Specifies how to transmit the export files via HTTP.",
				"Legt fest, aus welcher Art die Exportdateien per HTTP übertragen werden sollen.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportHttpTransmissionType.SimplePost", "Simple POST", "Einfacher POST");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportHttpTransmissionType.MultipartFormDataPost", "Multipart form data POST", "Multipart-Form-Data POST");

			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.PassiveMode",
				"Passive mode",
				"Passiver Modus",
				"Specifies whether to exchange data in active or passive mode.",
				"Legt fest, ob Daten im aktiven oder passiven Modus ausgetauscht werden sollen.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.UseSsl",
				"Use SSL",
				"SSL verwenden",
				"Specifies whether to use a SSL (Secure Sockets Layer) connection.",
				"Legt fest, ob einen SSL (Secure Sockets Layer) Verbindung genutzt werden soll.");


			builder.AddOrUpdate("Admin.DataExchange.Export.Filter.Note",
				"Specify individual filters to limit the exported data.",
				"Legen Sie individuelle Filter fest, um die zu exportierenden Daten einzugrenzen.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Projection.Note",
				"The following information will be taken into account during the export and integrated in the process.",
				"Die folgenden Angaben werden beim Export berücksichtigt und an entsprechenden Stellen in den Vorgang eingebunden.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Configuration.Note",
				"The following specific information will be taken into account by the provider during the export.",
				"Die folgenden spezifischen Angaben werden durch den Provider beim Export berücksichtigt.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Configuration.NotRequired",
				"The export provider <b>{0}</b> requires no further configuration.",
				"Der Export-Provider <b>{0}</b> benötigt keine weitergehende Konfiguration.");

			builder.AddOrUpdate("Admin.DataExchange.Export.Deployment.Note",
				"Click <b>Insert</b> to add one or multiple publishing profiles to specify how to further proceed with the export files.",
				"Legen Sie über <b>Hinzufügen</b> ein oder mehrere Veröffentlichungsprofile an, um festzulegen wie mit den Exportdateien weiter zu verfahren ist.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportOrderStatusChange.None", "None", "Keine");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportOrderStatusChange.Processing", "Processing", "Wird bearbeitet");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportOrderStatusChange.Complete", "Complete", "Komplett");

			builder.AddOrUpdate("Admin.DataExchange.Export.Projection.OrderStatusChange",
				"Change order status to",
				"Auftragsstatus ändern in",
				"Specifies if and how to change the status of the exported orders.",
				"Legt fest, ob und wie der Status der exportierten Aufträge geändert werden soll.");

			builder.AddOrUpdate("Admin.DataExchange.Export.EnableProfileForPreview",
				"The export profile is disabled. It must be enabled to preview the export data.",
				"Das Exportprofil ist deaktiviert. Für eine Exportvorschau muss das Exportprofil aktiviert sein.");

			builder.AddOrUpdate("Admin.DataExchange.Export.NoProfilesForProvider",
				"There was no export profile of type <b>{0}</b> found. Create now a <a href=\"{1}\">new export profile</a>.",
				"Es wurde kein Exportprofil vom Typ <b>{0}</b> gefunden. Jetzt ein <a href=\"{1}\">neues Exportprofil anlegen</a>.");

			builder.AddOrUpdate("Admin.DataExchange.Export.ProfileForProvider",
				"Export profile",
				"Exportprofil",
				"The export profile for this export provider.",
				"Das Exportprofil für diesen Export-Provider.");


			RemoveObsoleteResources(builder);
		}

		private void RemoveObsoleteResources(LocaleResourcesBuilder builder)
		{
			builder.Delete(
				"Plugins.Feed.FreeShippingThreshold"
			);

			builder.Delete(
				"Plugins.Feed.Billiger.ProductPictureSize",
				"Plugins.Feed.Billiger.ProductPictureSize.Hint",
				"Plugins.Feed.Billiger.TaskEnabled",
				"Plugins.Feed.Billiger.TaskEnabled.Hint",
				"Plugins.Feed.Billiger.StaticFileUrl",
				"Plugins.Feed.Billiger.StaticFileUrl.Hint",
				"Plugins.Feed.Billiger.GenerateStaticFileEachMinutes",
				"Plugins.Feed.Billiger.GenerateStaticFileEachMinutes.Hint",
				"Plugins.Feed.Billiger.BuildDescription",
				"Plugins.Feed.Billiger.BuildDescription.Hint",
				"Plugins.Feed.Billiger.Automatic",
				"Plugins.Feed.Billiger.DescShort",
				"Plugins.Feed.Billiger.DescLong",
				"Plugins.Feed.Billiger.DescTitleAndShort",
				"Plugins.Feed.Billiger.DescTitleAndLong",
				"Plugins.Feed.Billiger.DescManuAndTitleAndShort",
				"Plugins.Feed.Billiger.DescManuAndTitleAndLong",
				"Plugins.Feed.Billiger.DescriptionToPlainText",
				"Plugins.Feed.Billiger.DescriptionToPlainText.Hint",
				"Plugins.Feed.Billiger.ShippingCost",
				"Plugins.Feed.Billiger.ShippingCost.Hint",
				"Plugins.Feed.Billiger.ShippingTime",
				"Plugins.Feed.Billiger.ShippingTime.Hint",
				"Plugins.Feed.Billiger.Brand",
				"Plugins.Feed.Billiger.Brand.Hint",
				"Plugins.Feed.Billiger.UseOwnProductNo",
				"Plugins.Feed.Billiger.UseOwnProductNo.Hint",
				"Plugins.Feed.Billiger.Store",
				"Plugins.Feed.Billiger.Store.Hint",
				"Plugins.Feed.Billiger.ConvertNetToGrossPrices",
				"Plugins.Feed.Billiger.ConvertNetToGrossPrices.Hint",
				"Plugins.Feed.Billiger.LanguageId",
				"Plugins.Feed.Billiger.LanguageId.Hint",
				"Plugins.Feed.Billiger.ConfigSaveNote",
				"Plugins.Feed.Billiger.GeneratingNow",
				"Plugins.Feed.Billiger.SuccessResult"
			);

			builder.Delete(
				"Plugins.Feed.ElmarShopinfo.TaskEnabled",
				"Plugins.Feed.ElmarShopinfo.TaskEnabled.Hint",
				"Plugins.Feed.ElmarShopinfo.StaticFileUrl",
				"Plugins.Feed.ElmarShopinfo.StaticFileUrl.Hint",
				"Plugins.Feed.ElmarShopinfo.GenerateStaticFileEachMinutes",
				"Plugins.Feed.ElmarShopinfo.GenerateStaticFileEachMinutes.Hint",
				"Plugins.Feed.ElmarShopinfo.BuildDescription",
				"Plugins.Feed.ElmarShopinfo.BuildDescription.Hint",
				"Plugins.Feed.ElmarShopinfo.Automatic",
				"Plugins.Feed.ElmarShopinfo.DescShort",
				"Plugins.Feed.ElmarShopinfo.DescLong",
				"Plugins.Feed.ElmarShopinfo.DescTitleAndShort",
				"Plugins.Feed.ElmarShopinfo.DescTitleAndLong",
				"Plugins.Feed.ElmarShopinfo.DescManuAndTitleAndShort",
				"Plugins.Feed.ElmarShopinfo.DescManuAndTitleAndLong",
				"Plugins.Feed.ElmarShopinfo.DescriptionToPlainText",
				"Plugins.Feed.ElmarShopinfo.DescriptionToPlainText.Hint",
				"Plugins.Feed.ElmarShopinfo.ProductPictureSize",
				"Plugins.Feed.ElmarShopinfo.ProductPictureSize.Hint",
				"Plugins.Feed.ElmarShopinfo.Currency",
				"Plugins.Feed.ElmarShopinfo.Currency.Hint",
				"Plugins.Feed.ElmarShopinfo.ShippingTime",
				"Plugins.Feed.ElmarShopinfo.ShippingTime.Hint",
				"Plugins.Feed.ElmarShopinfo.Brand",
				"Plugins.Feed.ElmarShopinfo.Brand.Hint",
				"Plugins.Feed.ElmarShopinfo.Store",
				"Plugins.Feed.ElmarShopinfo.Store.Hint",
				"Plugins.Feed.ElmarShopinfo.ConvertNetToGrossPrices",
				"Plugins.Feed.ElmarShopinfo.ConvertNetToGrossPrices.Hint",
				"Plugins.Feed.ElmarShopInfo.LanguageId",
				"Plugins.Feed.ElmarShopInfo.LanguageId.Hint",
				"Plugins.Feed.ElmarShopInfo.General",
				"Plugins.Feed.ElmarShopInfo.General.Hint",
				"Plugins.Feed.ElmarShopInfo.Automation",
				"Plugins.Feed.ElmarShopInfo.Automation.Hint",
				"Plugins.Feed.ElmarShopInfo.Address",
				"Plugins.Feed.ElmarShopInfo.Address.Hint",
				"Plugins.Feed.ElmarShopInfo.Contact",
				"Plugins.Feed.ElmarShopInfo.Contact.Hint",
				"Plugins.Feed.ElmarShopInfo.Generate",
				"Plugins.Feed.ElmarShopInfo.ConfigSaveNote"
			);

			builder.Delete(
				"Plugins.Feed.Guenstiger.TaskEnabled",
				"Plugins.Feed.Guenstiger.TaskEnabled.Hint",
				"Plugins.Feed.Guenstiger.StaticFileUrl",
				"Plugins.Feed.Guenstiger.StaticFileUrl.Hint",
				"Plugins.Feed.Guenstiger.GenerateStaticFileEachMinutes",
				"Plugins.Feed.Guenstiger.GenerateStaticFileEachMinutes.Hint",
				"Plugins.Feed.Guenstiger.BuildDescription",
				"Plugins.Feed.Guenstiger.BuildDescription.Hint",
				"Plugins.Feed.Guenstiger.Automatic",
				"Plugins.Feed.Guenstiger.DescShort",
				"Plugins.Feed.Guenstiger.DescLong",
				"Plugins.Feed.Guenstiger.DescTitleAndShort",
				"Plugins.Feed.Guenstiger.DescTitleAndLong",
				"Plugins.Feed.Guenstiger.DescManuAndTitleAndShort",
				"Plugins.Feed.Guenstiger.DescManuAndTitleAndLong",
				"Plugins.Feed.Guenstiger.ProductPictureSize",
				"Plugins.Feed.Guenstiger.ProductPictureSize.Hint",
				"Plugins.Feed.Guenstiger.Brand",
				"Plugins.Feed.Guenstiger.Brand.Hint",
				"Plugins.Feed.Guenstiger.ShippingTime",
				"Plugins.Feed.Guenstiger.ShippingTime.Hint",
				"Plugins.Feed.Guenstiger.Store",
				"Plugins.Feed.Guenstiger.Store.Hint",
				"Plugins.Feed.Guenstiger.ConvertNetToGrossPrices",
				"Plugins.Feed.Guenstiger.ConvertNetToGrossPrices.Hint",
				"Plugins.Feed.Guenstiger.LanguageId",
				"Plugins.Feed.Guenstiger.LanguageId.Hint",
				"Plugins.Feed.Guenstiger.NoSpec",
				"Plugins.Feed.Guenstiger.NoSpec.Hint",
				"Plugins.Feed.Guenstiger.DescriptionToPlainText",
				"Plugins.Feed.Guenstiger.DescriptionToPlainText.Hint",
				"Plugins.Feed.Guenstiger.Generate",
				"Plugins.Feed.Guenstiger.ConfigSaveNote"
			);

			builder.Delete(
				"Plugins.Feed.Shopwahl.TaskEnabled",
				"Plugins.Feed.Shopwahl.TaskEnabled.Hint",
				"Plugins.Feed.Shopwahl.StaticFileUrl",
				"Plugins.Feed.Shopwahl.StaticFileUrl.Hint",
				"Plugins.Feed.Shopwahl.GenerateStaticFileEachMinutes",
				"Plugins.Feed.Shopwahl.GenerateStaticFileEachMinutes.Hint",
				"Plugins.Feed.Shopwahl.ShippingCost",
				"Plugins.Feed.Shopwahl.ShippingCost.Hint",
				"Plugins.Feed.Shopwahl.ShippingTime",
				"Plugins.Feed.Shopwahl.ShippingTime.Hint",
				"Plugins.Feed.Shopwahl.Currency",
				"Plugins.Feed.Shopwahl.Currency.Hint",
				"Plugins.Feed.Shopwahl.ProductPictureSize",
				"Plugins.Feed.Shopwahl.ProductPictureSize.Hint",
				"Plugins.Feed.Shopwahl.BuildDescription",
				"Plugins.Feed.Shopwahl.BuildDescription.Hint",
				"Plugins.Feed.Shopwahl.Automatic",
				"Plugins.Feed.Shopwahl.DescShort",
				"Plugins.Feed.Shopwahl.DescLong",
				"Plugins.Feed.Shopwahl.DescTitleAndShort",
				"Plugins.Feed.Shopwahl.DescTitleAndLong",
				"Plugins.Feed.Shopwahl.DescManuAndTitleAndShort",
				"Plugins.Feed.Shopwahl.DescManuAndTitleAndLong",
				"Plugins.Feed.Shopwahl.NoSpec",
				"Plugins.Feed.Shopwahl.UseOwnProductNo",
				"Plugins.Feed.Shopwahl.UseOwnProductNo.Hint",
				"Plugins.Feed.Shopwahl.DescriptionToPlainText",
				"Plugins.Feed.Shopwahl.DescriptionToPlainText.Hint",
				"Plugins.Feed.Shopwahl.Brand",
				"Plugins.Feed.Shopwahl.Brand.Hint",
				"Plugins.Feed.Shopwahl.ExportFormat",
				"Plugins.Feed.Shopwahl.ExportFormat.Hint",
				"Plugins.Feed.Shopwahl.Store",
				"Plugins.Feed.Shopwahl.Store.Hint",
				"Plugins.Feed.Shopwahl.ConvertNetToGrossPrices",
				"Plugins.Feed.Shopwahl.ConvertNetToGrossPrices.Hint",
				"Plugins.Feed.Shopwahl.LanguageId",
				"Plugins.Feed.Shopwahl.LanguageId.Hint",
				"Plugins.Feed.Shopwahl.Generate",
				"Plugins.Feed.Shopwahl.ConfigSaveNote"
			);
		}
    }
}

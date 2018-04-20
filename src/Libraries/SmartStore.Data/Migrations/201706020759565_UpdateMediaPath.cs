namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using System.Web.Hosting;
	using Core.Data;
	using Setup;

	public partial class UpdateMediaPath : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
			if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
			{
				var tenantName = DataSettings.Current.TenantName.NullEmpty() ?? "Default";
				var uploadedPath = $"/Media/{tenantName}/Uploaded/";
				var thumbsPath = $"/Media/{tenantName}/Thumbs/";

				Sql($"UPDATE [dbo].[Category] SET [Description] = REPLACE([Description],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[Category] SET [BottomDescription] = REPLACE([BottomDescription],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[Manufacturer] SET [Description] = REPLACE([Description],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[ShippingMethod] SET [Description] = REPLACE([Description],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[PaymentMethod] SET [FullDescription] = REPLACE([FullDescription],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[BlogPost] SET [Body] = REPLACE([Body],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[News] SET [Full] = REPLACE([Full],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[Campaign] SET [Body] = REPLACE([Body],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[QueuedEmail] SET [Body] = REPLACE([Body],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[QueuedEmail] SET [Body] = REPLACE([Body],'/Media/Thumbs/','{thumbsPath}')");
				Sql($"UPDATE [dbo].[Topic] SET [Body] = REPLACE([Body],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[MessageTemplate] SET [Body] = REPLACE([Body],'/Media/Uploaded/','{uploadedPath}')");
				Sql($"UPDATE [dbo].[Product] SET [FullDescription] = REPLACE([FullDescription],'/Media/Uploaded/','{uploadedPath}')");

				// LocalizedProperty
				Sql($"UPDATE [dbo].[LocalizedProperty] SET [LocaleValue] = REPLACE([LocaleValue],'/Media/Thumbs/','{thumbsPath}') WHERE [LocaleKey] = 'Body' AND [LocaleKeyGroup] = 'QueuedEmail'");
				Sql($@"
UPDATE [dbo].[LocalizedProperty] SET [LocaleValue] = REPLACE([LocaleValue],'/Media/Uploaded/','{uploadedPath}') 
WHERE 
	([LocaleKey] = 'BottomDescription' AND [LocaleKeyGroup] = 'Category') OR
	([LocaleKey] = 'Full' AND [LocaleKeyGroup] = 'News') OR
	([LocaleKey] = 'Description' AND [LocaleKeyGroup] IN ('Category', 'Manufacturer', 'ShippingMethod')) OR
	([LocaleKey] = 'FullDescription' AND [LocaleKeyGroup] IN ('PaymentMethod', 'Product')) OR
	([LocaleKey] = 'Body' AND [LocaleKeyGroup] IN ('BlogPost', 'Campaign', 'QueuedEmail', 'MessageTemplate', 'Topic'))");
			}
		}
        
        public override void Down()
        {
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			context.SaveChanges();
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Common.For", "For: {0}", "Für: {0}");
			builder.AddOrUpdate("Products.Sorting.Featured", "Featured", "Empfehlung");

			builder.AddOrUpdate("Common.AdditionalShippingSurcharge",
				"Plus <b>{0}</b> shipping surcharge",
				"zzgl. <b>{0}</b> zusätzlicher Versandgebühr");

            builder.AddOrUpdate("Address.Fields.Salutation", "Salutation", "Anrede");
            builder.AddOrUpdate("Address.Fields.Title", "Title", "Titel");
            builder.AddOrUpdate("Account.Fields.Title", "Title", "Titel");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.AddressFormFields.Salutations",
                "Salutations",
                "Anreden",
                "Comma separated list of salutations (e.g. Mr., Mrs). Define the entries which will populate the dropdown list for salutation when entering addresses.",
                "Komma getrennte Liste (z.B. Herr, Frau). Bestimmen Sie die Einträge für die Auswahl der Anrede, bei der Adresserfassung.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.AddressFormFields.SalutationEnabled",
                "'Salutation' enabled",
                "'Anrede' aktiv",
                "Set if 'Salutation' is enabled.",
                "Legt fest, ob das Feld 'Anrede' aktiv ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.AddressFormFields.TitleEnabled",
                "'Title' enabled",
                "'Titel' aktiv",
                "Set if 'Title' is enabled.",
                "Legt fest, ob das Feld 'Titel' aktiv ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.TitleEnabled",
                "'Title' enabled",
                "'Titel' aktiv",
                "Set if 'Title' is enabled.",
                "Legt fest, ob das Feld 'Titel' aktiv ist.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Shipping.SkipShippingIfSingleOption",
                "Display shipping options during checkout process only if more then one option is available",
                "Versandartauswahl nur anzeigen, wenn mehr als eine Versandart zur Verfügung steht",
                "Display shipping options during the checkout process only if more then one shipping option is available.",
                "Legt fest, ob die Versandartauswahl nur im Checkout-Prozess angezeigt wird, wenn mehr als eine Versandart zur Verfügung steht");

            builder.AddOrUpdate("Admin.DataExchange.Export.Projection.OnlyIndividuallyVisibleAssociated",
                "Only individually visible products",
                "Nur individuell sichtbare Produkte",
                "Specifies whether to only export individually visible associated products.",
                "Legt fest, ob nur individuell sichtbare, verknüpfte Produkte exportiert werden sollen.");

			builder.AddOrUpdate("Product.ThumbTitle", "{0}, Picture {1} large", "{0}, Bild {1} groß");
			builder.AddOrUpdate("Product.ThumbAlternateText", "{0}, Picture {1}", "{0}, Bild {1}");
		}
	}
}

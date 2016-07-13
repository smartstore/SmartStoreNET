namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;
    
    public partial class AddressEnhancement : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Address", "Salutation", c => c.String());
            AddColumn("dbo.Address", "Title", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Address", "Title");
            DropColumn("dbo.Address", "Salutation");
        }

        public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

            context.SaveChanges();
		}

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Address.Fields.Salutation", "Salutation", "Anrede");
            builder.AddOrUpdate("Address.Fields.Title", "Title", "Titel");
            builder.AddOrUpdate("Account.Fields.Title", "Title", "Titel");
            
            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.AddressFormFields.Salutations",
                "Salutations",
                "Anreden",
                "Comma separated list of salutations (e.g. Mr., Mrs). Define the entries which will populate the dropdown list for salutation when entering addresses.",
                "Komma getrennte Liste (z.B. Herr, Frau). Bestimmen Sie die Eintr�ge f�r die Auswahl der Anrede, bei der Adresserfassung.");

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
                "Versandartauswahl nur anzeigen, wenn mehr als eine Versandart zur Verf�gung steht",
                "Display shipping options during the checkout process only if more then one shipping option is available.",
                "Legt fest, ob die Versandartauswahl nur im Checkout-Prozess angezeigt wird, wenn mehr als eine Versandart zur Verf�gung steht");


			builder.AddOrUpdate("Admin.DataExchange.Export.Projection.OnlyIndividuallyVisibleAssociated",
				"Only individually visible products",
				"Nur individuell sichtbare Produkte",
				"Specifies whether to only export individually visible associated products.",
				"Legt fest, ob nur individuell sichtbare, verkn�pfte Produkte exportiert werden sollen.");


			builder.Delete("Providers.ExchangeRate.EcbExchange.SetCurrencyToEURO");
		}
    }
}

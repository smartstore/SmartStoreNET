namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using Setup;

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
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Common.Ignore", "Ignore", "Ignorieren");

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
		}
	}
}

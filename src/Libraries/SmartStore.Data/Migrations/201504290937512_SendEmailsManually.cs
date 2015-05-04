namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class SendEmailsManually : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Order", "HasNewPaymentNotification", c => c.Boolean(nullable: false));
            AddColumn("dbo.MessageTemplate", "SendManually", c => c.Boolean(nullable: false));
            AddColumn("dbo.QueuedEmail", "SendManually", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QueuedEmail", "SendManually");
            DropColumn("dbo.MessageTemplate", "SendManually");
            DropColumn("dbo.Order", "HasNewPaymentNotification");
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
			builder.AddOrUpdate("Admin.Common.ErrorSendingEmail",
				"Error sending an email",
				"Fehler beim Senden einer E-Mail");

			builder.AddOrUpdate("Admin.System.QueuedEmails.Fields.SendManually",
				"Only send manually",
				"Nur manuell senden",
				"Indicates whether the email should only be send manually.",
				"Legt fest, ob die E-Mail ausschlieﬂlich manuell gesendet werden soll.");

			builder.AddOrUpdate("Admin.System.QueuedEmails.List.SendManually",
				"Load emails manually send only",
				"Lade nur manuell zu sendende E-Mails",
				"Load emails manually send only.",
				"Lade nur manuell zu sendende E-Mails.");

			builder.AddOrUpdate("Admin.System.QueuedEmails.List.LoadNotSent",
				"Load not sent emails only",
				"Lade nur noch nicht gesendete E-Mails",
				"Load not sent emails only.",
				"Lade nur noch nicht gesendete E-Mails.");

			builder.AddOrUpdate("Admin.System.QueuedEmails.Requeued",
				"The queued email has been requeued successfully.",
				"Die Nachricht wurde erfolgreich neu eingereiht.");

			builder.AddOrUpdate("Admin.Common.SendNow",
				"Send now",
				"Jetzt senden");

			builder.AddOrUpdate("Admin.ContentManagement.MessageTemplates.Fields.SendManually",
				"Only send manually",
				"Nur manuell senden",
				"Indicates whether emails derived from this message template should only be send manually.",
				"Legt fest, ob E-Mails, die von dieser Nachrichtenvorlage abgeleitet sind, ausschlieﬂlich manuell gesendet werden sollen.");


			builder.AddOrUpdate("Admin.Orders.Payments.NewIpn",
				"New IPN",
				"Neue IPN");

			builder.AddOrUpdate("Admin.Orders.Payments.NewIpn.Hint",
				"A new notification from the payment provider has arrived in the order notes.",
				"In den Auftragsnotizen ist eine neue Benachrichtigung vom Zahlungsanbieter eingetroffen.");


			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.SubCategoryDisplayType",
				"Show subcategories",
				"Unterwarengruppen anzeigen",
				"Indicates whether and where to show subcategories on a category page.",
				"Legt fest, ob und wo Unterwarengruppen auf einer Warengruppenseite angezeigt werden sollen.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.SubCategoryDisplayType.Hide",
				"Do not display",
				"Nicht anzeigen");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.SubCategoryDisplayType.AboveProductList",
				"Above product list",
				"‹ber der Produktliste");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.SubCategoryDisplayType.Bottom",
				"At bottom of page",
				"Am Seitenende");
		}
    }
}

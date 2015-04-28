namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class SendEmailsManually : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.MessageTemplate", "SendManually", c => c.Boolean(nullable: false));
            AddColumn("dbo.QueuedEmail", "SendManually", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QueuedEmail", "SendManually");
            DropColumn("dbo.MessageTemplate", "SendManually");
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
		}
    }
}

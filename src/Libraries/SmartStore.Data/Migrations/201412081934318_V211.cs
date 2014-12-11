namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class V211 : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
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
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.CanonicalHostNameRule",
				"Canonical host name rule",
				"Regel für kanonischen Domänennamen");
			builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.CanonicalHostNameRule.Hint",
				"Enforces permanent redirection to a single domain name for a better page rank (e.g. mystore.com > www.mystore.com or vice versa)",
				"Erzwingt die permanente Umleitung zu einem einzelnen Domännennamen für ein besseres Seitenranking (z.B. meinshop.de > www.meinshop.de oder umgekehrt)");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Seo.CanonicalHostNameRule.NoRule",
				"Don't apply",
				"Nicht anwenden");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Seo.CanonicalHostNameRule.RequireWww",
				"Require www prefix",
				"www-Präfix erzwingen");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Seo.CanonicalHostNameRule.OmitWww",
				"Omit www prefix",
				"www-Präfix weglassen");

			builder.AddOrUpdate("Admin.Orders.Fields.PartialRefundOffline",
				"Partial refund (Offline)",
				"Teilerstattung (Offline)");
			builder.AddOrUpdate("Admin.Orders.Fields.Void",
				"Cancel",
				"Stornieren");
			builder.AddOrUpdate("Admin.Orders.Fields.VoidOffline",
				"Cancel (Offline)",
				"Stornieren (Offline)");

			builder.AddOrUpdate("Admin.Orders.Fields.MarkAsPaid.Hint",
				"Sets the payment status to 'paid' without contacting the payment provider.",
				"Setzt den Zahlungsstatus auf 'Bezahlt' ohne dabei den Zahlungsanbieter zu kontaktieren.");
			builder.AddOrUpdate("Admin.Orders.Fields.Capture.Hint",
				"Books a previously authorised payment through the payment provider.",
				"Zieht eine zuvor reservierte Zahlung über den Zahlungsanbieter ein.");
			builder.AddOrUpdate("Admin.Orders.Fields.Refund.Hint",
				"Initiates a refund of the total order value at the payment provider.",
				"Leitet eine Rückerstattung des gesamten Auftragswertes beim Zahlungsanbieter ein.");
			builder.AddOrUpdate("Admin.Orders.Fields.RefundOffline.Hint",
				"Setzt the payment status to 'refunded' without contacting the payment provider.",
				"Setzt den Zahlungsstatus auf 'Erstattet', ohne dabei den Zahlungsanbieter zu kontaktieren.");
			builder.AddOrUpdate("Admin.Orders.Fields.PartialRefund.Hint",
				"Initiates the refund of a partial amount of the total order value at the payment provider.",
				"Leitet die Rückerstattung eines Teilbetrages des Auftragswertes beim Zahlungsanbieter ein.");
			builder.AddOrUpdate("Admin.Orders.Fields.PartialRefundOffline.Hint",
				"Setzt the payment status to 'partially refunded' including the refunded amount without contacting the payment provider.",
				"Setzt den Zahlungsstatus auf 'Teilweise erstattet' samt Erstattungsbetrag, ohne dabei den Zahlungsanbieter zu kontaktieren.");
			builder.AddOrUpdate("Admin.Orders.Fields.Void.Hint",
				"Initiates the cancellation of the payment transaction at the payment provider.",
				"Leitet die Stornierung der Zahlungstransaktion beim Zahlungsanbieter ein.");
			builder.AddOrUpdate("Admin.Orders.Fields.VoidOffline.Hint",
				"Setzt the payment status to 'canceled' without contacting the payment provider.",
				"Setzt den Zahlungsstatus auf 'Storniert', ohne dabei den Zahlungsanbieter zu kontaktieren.");
		}
    }
}

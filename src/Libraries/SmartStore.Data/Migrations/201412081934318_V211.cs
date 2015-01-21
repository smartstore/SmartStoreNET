namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using SmartStore.Core.Domain.Topics;
	using SmartStore.Data.Setup;
	using System.Linq;

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

			var topic = context.Set<Topic>().FirstOrDefault(x => x.SystemName == "PageNotFound");
			if (topic != null)
			{
				context.Set<Topic>().Remove(topic);
			}
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

			// order payment row
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

			// Error Handling (404/500)
			builder.Delete("PageTitle.PageNotFound");
			builder.AddOrUpdate("ErrorPage.Title",
				"Oops!",
				"Oops!");
			builder.AddOrUpdate("ErrorPage.Body",
				@"We apologize, a server error ocurred while handling your request, this is not a problem with your computer or internet connection.
			The details have been sent to our support team and we will investigate the issue very soon.<br /><br />
			In the meantime, please retry your request as it may have been temporary.",
				@"Leider ist ein Serverfehler aufgetreten, und das hat nichts mit Ihrem Computer oder Ihrem Internetanschluss zu tun.
			Unser Support Team wurde bereits benachrichtigt und wird sich sehr bald um die Behebung kümmern.");
			builder.AddOrUpdate("NotFoundPage.Title",
				"404",
				"404");
			builder.AddOrUpdate("NotFoundPage.Body",
				"Sorry! The page you were looking for could not be found.",
				"Tut uns leid! Diese Adresse gibt es auf unserer Website nicht.");

			// amazon payment review
			builder.AddOrUpdate("Checkout.ConfirmYourOrder",
				"Please confirm your order.",
				"Bitte bestätigen Sie Ihren Auftrag.");

			builder.AddOrUpdate("Checkout.ConfirmHint",
				"Please verify the order total and the specifics regarding the billing address and, if required, the shipping address. You can make corrections to your entry anytime by clicking on <strong>back</strong>. If everything is as it should be, deliver your order to us by clicking <strong>confirm</strong>.",
				"Bitte prüfen Sie die Gesamtsumme und die Rechnungsadresse. Bei abweichender Lieferanschrift prüfen Sie bitte auch diese. Änderungen können Sie jederzeit mit einem Klick auf <strong>zurück</strong> vornehmen. Sind alle Daten richtig, bestätigen Sie bitte mit einem Klick auf <strong>kaufen</strong> Ihre Bestellung.");

			// new setting
			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.DeliveryTimeIdForEmptyStock",
				"Delivery time dislayed when stock is empty",
				"Lieferzeit, die bei einem leerem Lagerbestand angezeigt wird",
				"Delivery time to be dislayed when the stock quantity of a product is equal or less 0.",
				"Lieferzeit, die angezeigt wird, wenn der Warenbestand des Produktes kleiner gleich 0 ist.");

			// old setting now with ui
			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.IncludeFeaturedProductsInNormalLists",
				"Show featured products in lists",
				"Top-Produkte in Listen anzeigen",
				"Specifies whether to display featured products in product and filter lists. Otherwise they only appear in the top featured product list.",
				"Legt fest, ob Top-Produkte sowohl in den Produkt- als auch in den Filterlisten angezeigt werden sollen. Ansonsten erscheinen sie nur oberhalb in der Top-Produktliste.");

			// new notification
			builder.AddOrUpdate("Admin.Order.NotFound",
				"An order with this number was not found.",
				"Ein Auftrag mit dieser Nummer wurde nicht gefunden.");

			// PDF
			builder.AddOrUpdate("Common.Date",
				"Date",
				"Datum");

			builder.Delete(
				"PDFInvoice.Address", 
				"PDFInvoice.Address2",
				"PDFInvoice.BillingInformation",
				"PDFInvoice.Company",
				"PDFInvoice.CreatedOn",
				"PDFInvoice.Discount",
				"PDFInvoice.Fax",
				"PDFInvoice.GiftCardInfo",
				"PDFInvoice.Name",
				"PDFInvoice.Note",
				"PDFInvoice.OrderDate",
				"PDFInvoice.OrderTotal",
				"PDFInvoice.PaymentMethod",
				"PDFInvoice.PaymentMethodAdditionalFee",
				"PDFInvoice.Phone",
				"PDFInvoice.PurchaseOrderNumber",
				"PDFInvoice.RewardPoints",
				"PDFInvoice.ShippingInformation",
				"PDFInvoice.ShippingMethod",
				"PDFInvoice.Sub-Total",
				"PDFInvoice.Shipping",
				"PDFInvoice.Tax",
				"PDFInvoice.TaxRate",
				"PDFInvoice.VATNumber");

			builder.AddOrUpdate("PDFInvoice.Footer.Bankcode").Value("de", "BLZ: {0}");
			builder.AddOrUpdate("PDFInvoice.TaxNumber").Value("en", "Tax Number: {0}");
			builder.AddOrUpdate("PDFInvoice.Order#",
				"Order#",
				"Auftrag#");
			builder.AddOrUpdate("PDFInvoice.SKU",
				"SKU",
				"Artikelnr.");

            builder.AddOrUpdate("PDFPackagingSlip.DeliveryDate",
                "Delivery date",
				"Lieferdatum");
            builder.AddOrUpdate("PDFPackagingSlip.Weight",
                "Weight",
				"Gewicht");
            builder.AddOrUpdate("PDFPackagingSlip.DeliveryDate",
                "Delivery date",
                "Lieferdatum");
            builder.AddOrUpdate("PDFPackagingSlip.TrackingNumber",
                "Tracking number",
                "Trackingnummer");
            builder.AddOrUpdate("PDFPackagingSlip.ShippingMethod",
                "Shipping method",
                "Versandart");
            builder.AddOrUpdate("PDFPackagingSlip.ProductListHeadline",
                "Shipped products",
                "Gelieferte Produkte");

			builder.AddOrUpdate("Admin.Common.ExportToPdf.TooManyItems",
				"Too many items! The PDF conversion is limited to 500 items. Please reduce the amount of selected records.",
				"Zu viele Objekte! Mehr als 500 Objekte können nicht konvertiert werden. Bitte reduzieren Sie die Anzahl ausgewählter Datensätze.");
			builder.AddOrUpdate("Admin.Common.ExportToPdf.TocTitle",
				"Table of contents",
				"Inhaltsverzeichnis");

            builder.AddOrUpdate("PDFProductCatalog.Manufacturer",
                "Manufacturer",
                "Hersteller");
            builder.AddOrUpdate("PDFProductCatalog.Length",
				"Legth",
				"Länge");
            builder.AddOrUpdate("PDFProductCatalog.Width",
                "Width",
				"Breite");
            builder.AddOrUpdate("PDFProductCatalog.Height",
                "Height",
				"Höhe");

            builder.AddOrUpdate("PDFProductCatalog.SpecificationAttributes",
                "Specification attributes",
                "Spezifikation");
            builder.AddOrUpdate("PDFProductCatalog.BundledItems",
                "Bundled items",
                "Produktset besteht aus");
            builder.AddOrUpdate("PDFProductCatalog.AssociatedProducts",
                "Associated products",
                "Gruppierte Produkte");


            builder.AddOrUpdate("PDFProductCatalog.CompanyEmailAddress",
                "Mail",
                "E-Mail");
            builder.AddOrUpdate("PDFProductCatalog.CompanyTelephoneNumber",
                "Phone",
                "Telefon");
            builder.AddOrUpdate("PDFProductCatalog.CompanyFaxNumber",
                "Fax",
                "Fax");
            builder.AddOrUpdate("PDFProductCatalog.Cover.Address",
                "Address",
                "Anschrift");
            builder.AddOrUpdate("PDFProductCatalog.Cover.ContactData",
                "Contact data",
                "Kontaktdaten");

            builder.AddOrUpdate("Admin.Configuration.Settings.Order.MinOrderSubtotalAmount").Value("de", "Mindestbestellwert Zwischensumme");
            builder.AddOrUpdate("Admin.Configuration.Settings.Order.MinOrderSubtotalAmount.Hint").Value("de", "Legt den Mindestbestellwert der Zwischensumme fest.");
            builder.AddOrUpdate("Admin.Configuration.Settings.Order.MinOrderTotalAmount").Value("de", "Mindestbestellwert Gesamtsumme");
            builder.AddOrUpdate("Admin.Configuration.Settings.Order.MinOrderTotalAmount.Hint").Value("de", "Legt den Mindestbestellwert der Gesamtsumme fest.");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowProductReviewsInProductDetail",
                "Show product reviews in product detail view",
                "Zeige Produktbewertungen in der Produktdetailansicht");
            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowProductReviewsInProductDetail.Hint",
                "Determines whether product reviews are shown in product detail view.",
                "Legt fest, ob Produktbewertungen in der Produktdetailansicht angezeigt werden.");

		}
    }
}

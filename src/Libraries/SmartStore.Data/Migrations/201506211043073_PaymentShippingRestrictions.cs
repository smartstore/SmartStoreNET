namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class PaymentShippingRestrictions : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PaymentMethod",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PaymentMethodSystemName = c.String(nullable: false, maxLength: 4000),
                        ExcludedCustomerRoleIds = c.String(maxLength: 500),
                        ExcludedCountryIds = c.String(maxLength: 2000),
                        ExcludedShippingMethodIds = c.String(maxLength: 500),
                        CountryExclusionContextId = c.Int(nullable: false),
                        MinimumOrderAmount = c.Decimal(precision: 18, scale: 4),
                        MaximumOrderAmount = c.Decimal(precision: 18, scale: 4),
                        AmountRestrictionContextId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
				.Index(t => t.PaymentMethodSystemName);
            
            AddColumn("dbo.ShippingMethod", "ExcludedCustomerRoleIds", c => c.String(maxLength: 500));
            AddColumn("dbo.ShippingMethod", "CountryExclusionContextId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ShippingMethod", "CountryExclusionContextId");
            DropColumn("dbo.ShippingMethod", "ExcludedCustomerRoleIds");
            DropTable("dbo.PaymentMethod");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			context.MigrateSettings(x =>
			{
				x.Add("localizationsettings.loadalllocalizedpropertiesonstartup", true);
				x.Add("seosettings.loadallurlaliasesonstartup", true);
			});
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Common.Restrictions",
				"Restrictions",
				"Einschränkungen");

			builder.AddOrUpdate("Admin.Common.DeleteAll",
				"Delete all",
				"Alle löschen");

			builder.AddOrUpdate("Admin.Common.RecordsDeleted",
				"{0} records were deleted.",
				"Es wurden {0} Datensätze gelöscht.");

			builder.AddOrUpdate("Common.RequestProcessingFailed",
				"The request could not be processed.<br />Controller: {0}, Action: {1}, Reason: {2}.",
				"Die Anfrage konnte nicht ausgeführt werden.<br />Controller: {0}, Action: {1}, Grund: {2}.");

			builder.AddOrUpdate("Admin.System.Warnings.SitemapReachable.OK",
				"The sitemap for the store is reachable.",
				"Die Sitemap für den Shop ist erreichbar.");

			builder.AddOrUpdate("Admin.System.Warnings.SitemapReachable.Wrong",
				"The sitemap for the store is not reachable.",
				"Die Sitemap für den Shop ist nicht erreichbar.");


			builder.AddOrUpdate("Admin.Configuration.Payment.Methods.RestrictionNote",
				"Select customer roles, shipping methods, countries and order amounts for which you do <b>not</b> want to offer this payment method.",
				"Wählen Sie Kundengruppen, Versandarten, Länder und Bestellwerte, bei denen Sie diese Zahlungsmethode <b>nicht</b> anbieten möchten.");

			builder.AddOrUpdate("Admin.Configuration.Shipping.Methods.RestrictionNote",
				"Select customer roles and countries for which you do <b>not</b> want to offer this shipping method.",
				"Wählen Sie Kundengruppen und Länder, bei denen Sie diese Versandart <b>nicht</b> anbieten möchten.");


			builder.AddOrUpdate("Admin.Configuration.Payment.Methods.ExcludedCustomerRole",
				"Customer roles",
				"Kundengruppen",
				"Specifies customer roles for which the payment method should not be offered.",
				"Legt Kundengruppen fest, für die die Zahlungsmethode nicht angeboten werden soll.");

			builder.AddOrUpdate("Admin.Configuration.Payment.Methods.ExcludedShippingMethod",
				"Shipping method",
				"Versandarten",
				"Specifies shipping methods for which the payment method should not be offered.",
				"Legt Versandarten fest, für die die Zahlungsmethode nicht angeboten werden soll.");

			builder.AddOrUpdate("Admin.Configuration.Payment.Methods.ExcludedCountry",
				"Countries",
				"Länder",
				"Specifies countries for which the payment method should not be offered.",
				"Legt Länder fest, für die die Zahlungsmethode nicht angeboten werden soll.");

			builder.AddOrUpdate("Admin.Configuration.Payment.Methods.MinimumOrderAmount",
				"Minimum order amount",
				"Mindestbestellwert",
				"Specifies the minimum order amount from which on the payment method should be offered.",
				"Legt den Mindestbestellwert fest, ab dem die Zahlungsmethode angeboten werden soll.");

			builder.AddOrUpdate("Admin.Configuration.Payment.Methods.MaximumOrderAmount",
				"Maximum amount",
				"Maximalbestellwert",
				"Specifies the maximum order amount up to which the payment methods should be offered.",
				"Legt den maximalen Bestellwert fest, bis zu dem die Zahlungsmethode angeboten werden soll.");


			builder.AddOrUpdate("Admin.Configuration.Shipping.Methods.ExcludedCustomerRole",
				"Customer roles",
				"Kundengruppen",
				"Specifies customer roles for which the shipping method should not be offered.",
				"Legt Kundengruppen fest, für die die Versandart nicht angeboten werden soll.");

			builder.AddOrUpdate("Admin.Configuration.Shipping.Methods.ExcludedCountry",
				"Countries",
				"Länder",
				"Specifies countries for which the shipping method should not be offered.",
				"Legt Länder fest, für die die Versandart nicht angeboten werden soll.");


			builder.AddOrUpdate("Admin.Configuration.Restrictions.AmountRestrictionContext",
				"Amount related to",
				"Betrag bezieht sich auf",
				"Specifies the amount to which the minimum and maximum order amounts are related to.",
				"Legt den Betrag fest, auf den sich die Mindest- und Maximalbestellwerte beziehen.");

			builder.AddOrUpdate("Admin.Configuration.Restrictions.CountryExclusionContext",
				"Countries related to",
				"Länder beziehen sich auf",
				"Specifies the address to which the selected countries are related to.",
				"Legt die Adresse fest, auf den sich die gewählten Länder beziehen.");


			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Common.CountryRestrictionContextType.BillingAddress",
				"Billing address",
				"Rechnungsadresse");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Common.CountryRestrictionContextType.ShippingAddress",
				"Shipping address",
				"Versandadresse");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Common.AmountRestrictionContextType.SubtotalAmount",
				"Subtotal",
				"Zwischensumme");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Common.AmountRestrictionContextType.TotalAmount",
				"Order total",
				"Gesamtsumme");

			builder.Delete("Admin.Configuration.Shipping.Restrictions.Updated");
			builder.Delete("Admin.Configuration.Shipping.Restrictions.Description");
			builder.Delete("Admin.Configuration.Shipping.Restrictions.Country");
			builder.Delete("Admin.Configuration.Shipping.Restrictions");
		}
    }
}

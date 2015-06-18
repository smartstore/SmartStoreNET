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
                        ExcludedCustomerRoleIds = c.String(),
                        ExcludedCountryIds = c.String(),
                        ExcludedShippingMethodIds = c.String(),
                        CountryExclusionContextId = c.Int(nullable: false),
                        MinimumOrderAmount = c.Decimal(precision: 18, scale: 4),
                        MaximumOrderAmount = c.Decimal(precision: 18, scale: 4),
                        AmountRestrictionContextId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
				.Index(t => t.PaymentMethodSystemName);
            
            AddColumn("dbo.ShippingMethod", "ExcludedCustomerRoleIds", c => c.String());
        }
        
        public override void Down()
        {
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
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Common.Restrictions",
				"Restrictions",
				"Einschränkungen");

			builder.AddOrUpdate("Admin.Common.RelatedTo",
				"related to",
				"bezogen auf");

			builder.AddOrUpdate("Admin.Configuration.Payment.Methods.RestrictionNote",
				"Select customer roles, shipping methods and countries for which you do <u>not</u> want to offer this payment method.",
				"Wählen Sie Kundengruppen, Versandarten und Länder, bei denen Sie diese Zahlungsmethode <u>nicht</u> anbieten möchten.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Payments.CountryExclusionContextType.BillingAddress",
				"Billing address",
				"Rechnungsadresse");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Payments.CountryExclusionContextType.ShippingAddress",
				"Shipping address",
				"Versandadresse");

			builder.AddOrUpdate("Admin.Configuration.Shipping.Methods.RestrictionNote",
				"Select customer roles for which you do <u>not</u> want to offer this shipping method.",
				"Wählen Sie Kundengruppen, bei denen Sie diese Versandart <u>nicht</u> anbieten möchten.");

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

			builder.AddOrUpdate("Admin.Configuration.Payment.Methods.AmountRestrictionContext",
				"Amount related to",
				"Betrag bezieht sich auf",
				"Specifies the amount to which the minimum and maximum order amounts are related to.",
				"Legt den Betrag fest, auf den sich die Minimal- und Maximalbestellwerte beziehen.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Payments.AmountRestrictionContextType.SubtotalAmount",
				"Subtotal",
				"Zwischensumme");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Payments.AmountRestrictionContextType.TotalAmount",
				"Order total",
				"Gesamtsumme");
		}
    }
}

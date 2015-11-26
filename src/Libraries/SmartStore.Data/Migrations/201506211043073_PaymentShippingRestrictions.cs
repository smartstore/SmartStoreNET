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


			builder.Delete("Admin.Configuration.Shipping.Restrictions.Updated");
			builder.Delete("Admin.Configuration.Shipping.Restrictions.Description");
			builder.Delete("Admin.Configuration.Shipping.Restrictions.Country");
			builder.Delete("Admin.Configuration.Shipping.Restrictions");
		}
    }
}

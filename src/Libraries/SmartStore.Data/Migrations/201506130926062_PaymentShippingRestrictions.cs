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
                    })
                .PrimaryKey(t => t.Id)
				.Index(t => t.PaymentMethodSystemName);            
        }
        
        public override void Down()
        {
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

			builder.AddOrUpdate("Admin.Configuration.Payment.Methods.RestrictionNote",
				"Select customer roles, shipping methods and countries for which you do <u>not</u> want to offer this payment method.",
				"Wählen Sie Kundengruppen, Versandarten und Länder, bei denen sie diese Zahlungsmethode <u>nicht</u> anbieten möchten.");
		}
    }
}

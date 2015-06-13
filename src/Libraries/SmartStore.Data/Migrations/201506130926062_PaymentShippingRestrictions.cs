namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PaymentShippingRestrictions : DbMigration
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
    }
}

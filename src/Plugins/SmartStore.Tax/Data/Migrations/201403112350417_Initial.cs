namespace SmartStore.Tax.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
			if (DbMigrationContext.Current.SuppressInitialCreate<TaxRateObjectContext>())
				return;
			
			CreateTable(
                "dbo.TaxRate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TaxCategoryId = c.Int(nullable: false),
                        CountryId = c.Int(nullable: false),
                        StateProvinceId = c.Int(nullable: false),
                        Zip = c.String(maxLength: 4000),
                        Percentage = c.Decimal(nullable: false, precision: 18, scale: 4),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TaxRate");
        }
    }
}

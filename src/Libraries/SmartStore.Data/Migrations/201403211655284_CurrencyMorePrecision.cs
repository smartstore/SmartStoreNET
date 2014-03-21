namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CurrencyMorePrecision : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Currency", "Rate", c => c.Decimal(nullable: false, precision: 18, scale: 8));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Currency", "Rate", c => c.Decimal(nullable: false, precision: 18, scale: 4));
        }
    }
}

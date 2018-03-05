namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Wallet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WalletHistory",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StoreId = c.Int(nullable: false),
                        CustomerId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 4),
                        AmountBalance = c.Decimal(nullable: false, precision: 18, scale: 4),
                        CreatedOnUtc = c.DateTime(nullable: false),
                        Message = c.String(maxLength: 1000),
                        AdminComment = c.String(maxLength: 4000),
                        UsedWithOrder_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Order", t => t.UsedWithOrder_Id)
                .Index(t => new { t.StoreId, t.CreatedOnUtc }, name: "IX_StoreId_CreatedOn")
                .Index(t => t.CustomerId)
                .Index(t => t.UsedWithOrder_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WalletHistory", "UsedWithOrder_Id", "dbo.Order");
            DropForeignKey("dbo.WalletHistory", "CustomerId", "dbo.Customer");
            DropIndex("dbo.WalletHistory", new[] { "UsedWithOrder_Id" });
            DropIndex("dbo.WalletHistory", new[] { "CustomerId" });
            DropIndex("dbo.WalletHistory", "IX_StoreId_CreatedOn");
            DropTable("dbo.WalletHistory");
        }
    }
}

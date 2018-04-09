namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Web.Hosting;
    using Core.Data;
    using Setup;

    public partial class Wallet : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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
                        OrderId = c.Int(),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 4),
                        AmountBalance = c.Decimal(nullable: false, precision: 18, scale: 4),
                        AmountBalancePerStore = c.Decimal(nullable: false, precision: 18, scale: 4),
                        CreatedOnUtc = c.DateTime(nullable: false),
                        Reason = c.Int(),
                        Message = c.String(maxLength: 1000),
                        AdminComment = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Order", t => t.OrderId)
                .Index(t => new { t.StoreId, t.CreatedOnUtc }, name: "IX_StoreId_CreatedOn")
                .Index(t => t.CustomerId)
                .Index(t => t.OrderId);
            
            AddColumn("dbo.Product", "IsSystemProduct", c => c.Boolean(nullable: false));
            AddColumn("dbo.Product", "SystemName", c => c.String(maxLength: 500));
            AddColumn("dbo.Order", "CreditBalance", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AddColumn("dbo.Order", "RefundedCreditBalance", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            CreateIndex("dbo.Product", new[] { "SystemName", "IsSystemProduct" }, name: "Product_SystemName_IsSystemProduct");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WalletHistory", "OrderId", "dbo.Order");
            DropForeignKey("dbo.WalletHistory", "CustomerId", "dbo.Customer");
            DropIndex("dbo.WalletHistory", new[] { "OrderId" });
            DropIndex("dbo.WalletHistory", new[] { "CustomerId" });
            DropIndex("dbo.WalletHistory", "IX_StoreId_CreatedOn");
            DropIndex("dbo.Product", "Product_SystemName_IsSystemProduct");
            DropColumn("dbo.Order", "RefundedCreditBalance");
            DropColumn("dbo.Order", "CreditBalance");
            DropColumn("dbo.Product", "SystemName");
            DropColumn("dbo.Product", "IsSystemProduct");
            DropTable("dbo.WalletHistory");
        }

        public bool RollbackOnFailure
        {
            get { return false; }
        }

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);

            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.WalletPostingReason.Admin",
                "Administration",
                "Administration");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.WalletPostingReason.Purchase",
                "Purchase",
                "Einkauf");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.WalletPostingReason.Refill",
                "Refilling",
                "Auffüllung");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.WalletPostingReason.Refund",
                "Refund",
                "Rückerstattung");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.WalletPostingReason.PartialRefund",
                "Partial refund",
                "Teilerstattung");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.WalletPostingReason.Debit",
                "Debit",
                "Kontobelastung");

            builder.AddOrUpdate("ShoppingCart.Totals.CreditBalance",
                "Credit balance",
                "Guthaben");

            builder.AddOrUpdate("Admin.Orders.Fields.CreditBalance",
                "Credit balance",
                "Guthaben",
                "The used credit balance.",
                "Das verwendete Guthaben.");

            builder.AddOrUpdate("Admin.Validation.ValueLessThan",
                "The value must be less than {0}.",
                "Der Wert muss kleiner {0} sein.");
        }
    }
}

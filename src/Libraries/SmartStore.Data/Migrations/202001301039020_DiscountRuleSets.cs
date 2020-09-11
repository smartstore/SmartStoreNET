namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Core.Data;
    using SmartStore.Data.Setup;
    using SmartStore.Data.Utilities;

    public partial class DiscountRuleSets : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RuleSet_PaymentMethod_Mapping",
                c => new
                {
                    PaymentMethod_Id = c.Int(nullable: false),
                    RuleSetEntity_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.PaymentMethod_Id, t.RuleSetEntity_Id })
                .ForeignKey("dbo.PaymentMethod", t => t.PaymentMethod_Id, cascadeDelete: true)
                .ForeignKey("dbo.RuleSet", t => t.RuleSetEntity_Id, cascadeDelete: true)
                .Index(t => t.PaymentMethod_Id)
                .Index(t => t.RuleSetEntity_Id);

            CreateTable(
                "dbo.RuleSet_ShippingMethod_Mapping",
                c => new
                {
                    ShippingMethod_Id = c.Int(nullable: false),
                    RuleSetEntity_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.ShippingMethod_Id, t.RuleSetEntity_Id })
                .ForeignKey("dbo.ShippingMethod", t => t.ShippingMethod_Id, cascadeDelete: true)
                .ForeignKey("dbo.RuleSet", t => t.RuleSetEntity_Id, cascadeDelete: true)
                .Index(t => t.ShippingMethod_Id)
                .Index(t => t.RuleSetEntity_Id);

            CreateTable(
                "dbo.RuleSet_Discount_Mapping",
                c => new
                {
                    Discount_Id = c.Int(nullable: false),
                    RuleSetEntity_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.Discount_Id, t.RuleSetEntity_Id })
                .ForeignKey("dbo.Discount", t => t.Discount_Id, cascadeDelete: true)
                .ForeignKey("dbo.RuleSet", t => t.RuleSetEntity_Id, cascadeDelete: true)
                .Index(t => t.Discount_Id)
                .Index(t => t.RuleSetEntity_Id);

            CreateIndex("dbo.RuleSet", "IsSubGroup");
        }

        public override void Down()
        {
            DropForeignKey("dbo.RuleSet_Discount_Mapping", "RuleSetEntity_Id", "dbo.RuleSet");
            DropForeignKey("dbo.RuleSet_Discount_Mapping", "Discount_Id", "dbo.Discount");
            DropForeignKey("dbo.RuleSet_ShippingMethod_Mapping", "RuleSetEntity_Id", "dbo.RuleSet");
            DropForeignKey("dbo.RuleSet_ShippingMethod_Mapping", "ShippingMethod_Id", "dbo.ShippingMethod");
            DropForeignKey("dbo.RuleSet_PaymentMethod_Mapping", "RuleSetEntity_Id", "dbo.RuleSet");
            DropForeignKey("dbo.RuleSet_PaymentMethod_Mapping", "PaymentMethod_Id", "dbo.PaymentMethod");
            DropIndex("dbo.RuleSet_Discount_Mapping", new[] { "RuleSetEntity_Id" });
            DropIndex("dbo.RuleSet_Discount_Mapping", new[] { "Discount_Id" });
            DropIndex("dbo.RuleSet_ShippingMethod_Mapping", new[] { "RuleSetEntity_Id" });
            DropIndex("dbo.RuleSet_ShippingMethod_Mapping", new[] { "ShippingMethod_Id" });
            DropIndex("dbo.RuleSet_PaymentMethod_Mapping", new[] { "RuleSetEntity_Id" });
            DropIndex("dbo.RuleSet_PaymentMethod_Mapping", new[] { "PaymentMethod_Id" });
            DropIndex("dbo.RuleSet", new[] { "IsSubGroup" });
            DropTable("dbo.RuleSet_Discount_Mapping");
            DropTable("dbo.RuleSet_ShippingMethod_Mapping");
            DropTable("dbo.RuleSet_PaymentMethod_Mapping");
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            if (DataSettings.DatabaseIsInstalled())
            {
                DataMigrator.AddRuleSets(context);
            }
        }
    }
}

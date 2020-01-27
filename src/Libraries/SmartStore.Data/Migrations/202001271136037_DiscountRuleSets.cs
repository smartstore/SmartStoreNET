namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using SmartStore.Core.Data;
    using SmartStore.Data.Setup;
    using SmartStore.Data.Utilities;

    public partial class DiscountRuleSets : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Discount_RuleSet_Mapping",
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Discount_RuleSet_Mapping", "RuleSetEntity_Id", "dbo.RuleSet");
            DropForeignKey("dbo.Discount_RuleSet_Mapping", "Discount_Id", "dbo.Discount");
            DropIndex("dbo.Discount_RuleSet_Mapping", new[] { "RuleSetEntity_Id" });
            DropIndex("dbo.Discount_RuleSet_Mapping", new[] { "Discount_Id" });
            DropTable("dbo.Discount_RuleSet_Mapping");
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            if (DataSettings.DatabaseIsInstalled())
            {
                DataMigrator.AddRuleSets(context);
            }
        }
    }
}

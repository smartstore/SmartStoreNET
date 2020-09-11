namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class RuleSystem : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RuleSet",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(maxLength: 200),
                    Description = c.String(maxLength: 400),
                    IsActive = c.Boolean(nullable: false),
                    Scope = c.Int(nullable: false),
                    IsSubGroup = c.Boolean(nullable: false),
                    LogicalOperator = c.Int(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                    UpdatedOnUtc = c.DateTime(nullable: false),
                    LastProcessedOnUtc = c.DateTime(),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.IsActive, t.Scope }, name: "IX_RuleSetEntity_Scope");

            CreateTable(
                "dbo.Rule",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    RuleSetId = c.Int(nullable: false),
                    RuleType = c.String(nullable: false, maxLength: 100),
                    Operator = c.String(nullable: false, maxLength: 10),
                    Value = c.String(),
                    DisplayOrder = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RuleSet", t => t.RuleSetId, cascadeDelete: true)
                .Index(t => t.RuleSetId)
                .Index(t => t.RuleType, name: "IX_PageBuilder_RuleType")
                .Index(t => t.DisplayOrder, name: "IX_PageBuilder_DisplayOrder");

        }

        public override void Down()
        {
            DropForeignKey("dbo.Rule", "RuleSetId", "dbo.RuleSet");
            DropIndex("dbo.Rule", "IX_PageBuilder_DisplayOrder");
            DropIndex("dbo.Rule", "IX_PageBuilder_RuleType");
            DropIndex("dbo.Rule", new[] { "RuleSetId" });
            DropIndex("dbo.RuleSet", "IX_RuleSetEntity_Scope");
            DropTable("dbo.Rule");
            DropTable("dbo.RuleSet");
        }
    }
}

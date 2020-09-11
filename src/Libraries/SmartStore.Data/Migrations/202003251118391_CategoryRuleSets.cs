namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Domain.Localization;
    using SmartStore.Core.Domain.Tasks;
    using SmartStore.Data.Setup;

    public partial class CategoryRuleSets : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.RuleSet_Category_Mapping",
                c => new
                {
                    Category_Id = c.Int(nullable: false),
                    RuleSetEntity_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.Category_Id, t.RuleSetEntity_Id })
                .ForeignKey("dbo.Category", t => t.Category_Id, cascadeDelete: true)
                .ForeignKey("dbo.RuleSet", t => t.RuleSetEntity_Id, cascadeDelete: true)
                .Index(t => t.Category_Id)
                .Index(t => t.RuleSetEntity_Id);

            AddColumn("dbo.Product_Category_Mapping", "IsSystemMapping", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Product_Category_Mapping", "IsSystemMapping");
        }

        public override void Down()
        {
            DropForeignKey("dbo.RuleSet_Category_Mapping", "RuleSetEntity_Id", "dbo.RuleSet");
            DropForeignKey("dbo.RuleSet_Category_Mapping", "Category_Id", "dbo.Category");
            DropIndex("dbo.RuleSet_Category_Mapping", new[] { "RuleSetEntity_Id" });
            DropIndex("dbo.RuleSet_Category_Mapping", new[] { "Category_Id" });
            DropIndex("dbo.Product_Category_Mapping", new[] { "IsSystemMapping" });
            DropColumn("dbo.Product_Category_Mapping", "IsSystemMapping");
            DropTable("dbo.RuleSet_Category_Mapping");
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            var defaultLang = context.Set<Language>().AsNoTracking().OrderBy(x => x.DisplayOrder).First();
            var isGerman = defaultLang.UniqueSeoCode.IsCaseInsensitiveEqual("de");

            // Note, core scheduled tasks must always be added to the installation as well!
            context.Set<ScheduleTask>().AddOrUpdate(x => x.Type,
                new ScheduleTask
                {
                    Name = isGerman ? "Zuordnungen von Produkten zu Warengruppen aktualisieren" : "Update assignments of products to categories",
                    CronExpression = "20 2 * * *", // At 02:20
                    Type = "SmartStore.Services.Catalog.ProductRuleEvaluatorTask, SmartStore.Services",
                    Enabled = true,
                    StopOnError = false
                }
            );
            context.SaveChanges();
        }
    }
}

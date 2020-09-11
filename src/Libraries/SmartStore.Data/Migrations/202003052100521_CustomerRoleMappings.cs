namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web.Hosting;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Localization;
    using SmartStore.Core.Domain.Tasks;
    using SmartStore.Data.Setup;

    public partial class CustomerRoleMappings : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomerRoleMapping",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    CustomerId = c.Int(nullable: false),
                    CustomerRoleId = c.Int(nullable: false),
                    IsSystemMapping = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customer", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.CustomerRole", t => t.CustomerRoleId, cascadeDelete: true)
                .Index(t => t.CustomerId)
                .Index(t => t.CustomerRoleId)
                .Index(t => t.IsSystemMapping);

            CreateTable(
                "dbo.RuleSet_CustomerRole_Mapping",
                c => new
                {
                    CustomerRole_Id = c.Int(nullable: false),
                    RuleSetEntity_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.CustomerRole_Id, t.RuleSetEntity_Id })
                .ForeignKey("dbo.CustomerRole", t => t.CustomerRole_Id, cascadeDelete: true)
                .ForeignKey("dbo.RuleSet", t => t.RuleSetEntity_Id, cascadeDelete: true)
                .Index(t => t.CustomerRole_Id)
                .Index(t => t.RuleSetEntity_Id);

            if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
            {
                // Copy customer role mappings.
                Sql("IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Customer_CustomerRole_Mapping')) BEGIN Insert Into [dbo].[CustomerRoleMapping] (CustomerId, CustomerRoleId, IsSystemMapping) Select Customer_Id As Customer_Id, CustomerRole_Id As CustomerRole_Id, 0 As IsSystemMapping From [dbo].[Customer_CustomerRole_Mapping] END");
            }
        }

        public override void Down()
        {
            DropForeignKey("dbo.RuleSet_CustomerRole_Mapping", "RuleSetEntity_Id", "dbo.RuleSet");
            DropForeignKey("dbo.RuleSet_CustomerRole_Mapping", "CustomerRole_Id", "dbo.CustomerRole");
            DropForeignKey("dbo.CustomerRoleMapping", "CustomerRoleId", "dbo.CustomerRole");
            DropForeignKey("dbo.CustomerRoleMapping", "CustomerId", "dbo.Customer");
            DropIndex("dbo.RuleSet_CustomerRole_Mapping", new[] { "RuleSetEntity_Id" });
            DropIndex("dbo.RuleSet_CustomerRole_Mapping", new[] { "CustomerRole_Id" });
            DropIndex("dbo.CustomerRoleMapping", new[] { "IsSystemMapping" });
            DropIndex("dbo.CustomerRoleMapping", new[] { "CustomerRoleId" });
            DropIndex("dbo.CustomerRoleMapping", new[] { "CustomerId" });
            DropTable("dbo.RuleSet_CustomerRole_Mapping");
            DropTable("dbo.CustomerRoleMapping");
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            if (!DataSettings.DatabaseIsInstalled())
            {
                return;
            }

            var defaultLang = context.Set<Language>().AsNoTracking().OrderBy(x => x.DisplayOrder).First();
            var isGerman = defaultLang.UniqueSeoCode.IsCaseInsensitiveEqual("de");

            // Note, core scheduled tasks must always be added to the installation as well!
            context.Set<ScheduleTask>().AddOrUpdate(x => x.Type,
                new ScheduleTask
                {
                    Name = isGerman ? "Zuordnungen von Kunden zu Kundengruppen aktualisieren" : "Update assignments of customers to customer roles",
                    CronExpression = "15 2 * * *", // At 02:15
                    Type = "SmartStore.Services.Customers.TargetGroupEvaluatorTask, SmartStore.Services",
                    Enabled = true,
                    StopOnError = false
                }
            );
            context.SaveChanges();
        }
    }
}

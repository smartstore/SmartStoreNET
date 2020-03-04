namespace SmartStore.Data.Migrations
{
    using System;
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
            DropForeignKey("dbo.Customer_CustomerRole_Mapping", "Customer_Id", "dbo.Customer");
            DropForeignKey("dbo.Customer_CustomerRole_Mapping", "CustomerRole_Id", "dbo.CustomerRole");
            DropIndex("dbo.Customer_CustomerRole_Mapping", new[] { "Customer_Id" });
            DropIndex("dbo.Customer_CustomerRole_Mapping", new[] { "CustomerRole_Id" });
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

            // Copy mapping data.
            if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
            {
                Sql("Insert Into [dbo].[CustomerRoleMapping] (CustomerId, CustomerRoleId, IsSystemMapping) Select Customer_Id As Customer_Id, CustomerRole_Id As CustomerRole_Id, 0 As IsSystemMapping From [dbo].[Customer_CustomerRole_Mapping]");
            }

            DropTable("dbo.Customer_CustomerRole_Mapping");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Customer_CustomerRole_Mapping",
                c => new
                    {
                        Customer_Id = c.Int(nullable: false),
                        CustomerRole_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Customer_Id, t.CustomerRole_Id });

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

            // Copy mapping data.
            if (DataSettings.Current.IsSqlServer)
            {
                Sql("Insert Into [dbo].[Customer_CustomerRole_Mapping] (Customer_Id, CustomerRole_Id) Select CustomerId As CustomerId, CustomerRoleId As CustomerRoleId From [dbo].[CustomerRoleMapping]");
            }

            DropTable("dbo.CustomerRoleMapping");
            CreateIndex("dbo.Customer_CustomerRole_Mapping", "CustomerRole_Id");
            CreateIndex("dbo.Customer_CustomerRole_Mapping", "Customer_Id");
            AddForeignKey("dbo.Customer_CustomerRole_Mapping", "CustomerRole_Id", "dbo.CustomerRole", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Customer_CustomerRole_Mapping", "Customer_Id", "dbo.Customer", "Id", cascadeDelete: true);
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            if (DataSettings.DatabaseIsInstalled())
            {
                var defaultLang = context.Set<Language>().AsNoTracking().OrderBy(x => x.DisplayOrder).First();
                var isGerman = defaultLang.UniqueSeoCode.IsCaseInsensitiveEqual("de");

                context.Set<ScheduleTask>().AddOrUpdate(x => x.Type,
                    new ScheduleTask
                    {
                        Name = isGerman ? "Zuordnungen zu Kundengruppen für Regeln aktualisieren" : "Update assignments to customer roles for rules",
                        CronExpression = "15 2 * * *", // At 02:15
                        Type = "SmartStore.Services.Customers.CustomerRolesAssignmentsTask, SmartStore.Services",
                        Enabled = true,
                        StopOnError = false
                    }
                );
                context.SaveChanges();
            }
        }
    }
}

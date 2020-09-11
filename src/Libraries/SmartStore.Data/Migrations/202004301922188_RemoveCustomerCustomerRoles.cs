namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Web.Hosting;
    using SmartStore.Core.Data;
    using SmartStore.Data.Setup;

    public partial class RemoveCustomerCustomerRoles : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            if (DataSettings.Current.IsSqlServer)
            {
                AlterColumn("dbo.Rule", "Value", c => c.String());
            }
            else
            {
                // SQL Server CE cannot alter column of type NTEXT or IMAGE.
            }

            //DropForeignKey("dbo.Customer_CustomerRole_Mapping", "Customer_Id", "dbo.Customer");
            //DropForeignKey("dbo.Customer_CustomerRole_Mapping", "CustomerRole_Id", "dbo.CustomerRole");
            //DropIndex("dbo.Customer_CustomerRole_Mapping", new[] { "Customer_Id" });
            //DropIndex("dbo.Customer_CustomerRole_Mapping", new[] { "CustomerRole_Id" });
            //DropTable("dbo.Customer_CustomerRole_Mapping");
        }

        public override void Down()
        {
            AlterColumn("dbo.Rule", "Value", c => c.String(maxLength: 400));

            //CreateTable(
            //    "dbo.Customer_CustomerRole_Mapping",
            //    c => new
            //        {
            //            Customer_Id = c.Int(nullable: false),
            //            CustomerRole_Id = c.Int(nullable: false),
            //        })
            //    .PrimaryKey(t => new { t.Customer_Id, t.CustomerRole_Id });

            //CreateIndex("dbo.Customer_CustomerRole_Mapping", "CustomerRole_Id");
            //CreateIndex("dbo.Customer_CustomerRole_Mapping", "Customer_Id");
            //AddForeignKey("dbo.Customer_CustomerRole_Mapping", "CustomerRole_Id", "dbo.CustomerRole", "Id", cascadeDelete: true);
            //AddForeignKey("dbo.Customer_CustomerRole_Mapping", "Customer_Id", "dbo.Customer", "Id", cascadeDelete: true);
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            if (!HostingEnvironment.IsHosted || !DataSettings.Current.IsSqlServer)
            {
                return;
            }

            // Remove Customer_CustomerRole_Mapping.
            // Has been migrated through 202003052100521_CustomerRoleMappings.
            context.ExecuteSqlCommandSafe("ALTER TABLE [dbo].[Customer_CustomerRole_Mapping] DROP CONSTRAINT [FK_dbo.Customer_CustomerRole_Mapping_dbo.Customer_Customer_Id]");
            context.ExecuteSqlCommandSafe("ALTER TABLE [dbo].[Customer_CustomerRole_Mapping] DROP CONSTRAINT [FK_dbo.Customer_CustomerRole_Mapping_dbo.CustomerRole_CustomerRole_Id]");
            context.ExecuteSqlCommandSafe("DROP INDEX [IX_Customer_Id] ON [dbo].[Customer_CustomerRole_Mapping]");
            context.ExecuteSqlCommandSafe("DROP INDEX [IX_CustomerRole_Id] ON [dbo].[Customer_CustomerRole_Mapping]");
            context.ExecuteSqlCommandSafe("DROP TABLE [dbo].[Customer_CustomerRole_Mapping]");
        }
    }
}

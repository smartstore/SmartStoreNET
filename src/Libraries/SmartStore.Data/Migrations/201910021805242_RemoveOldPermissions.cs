namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Web.Hosting;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Tasks;
    using SmartStore.Data.Setup;

    public partial class RemoveOldPermissions : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            //if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
            //{
            //    Sql("DELETE FROM [dbo].[PermissionRecord] WHERE [Name] <> '' AND [Name] IS NOT NULL");
            //}

            //DropForeignKey("dbo.PermissionRecord_Role_Mapping", "PermissionRecord_Id", "dbo.PermissionRecord");
            //DropForeignKey("dbo.PermissionRecord_Role_Mapping", "CustomerRole_Id", "dbo.CustomerRole");
            //DropIndex("dbo.PermissionRecord_Role_Mapping", new[] { "PermissionRecord_Id" });
            //DropIndex("dbo.PermissionRecord_Role_Mapping", new[] { "CustomerRole_Id" });
            //DropColumn("dbo.PermissionRecord", "Name");
            //DropColumn("dbo.PermissionRecord", "Category");
            //DropTable("dbo.PermissionRecord_Role_Mapping");
        }

        public override void Down()
        {
            //CreateTable(
            //    "dbo.PermissionRecord_Role_Mapping",
            //    c => new
            //    {
            //        PermissionRecord_Id = c.Int(nullable: false),
            //        CustomerRole_Id = c.Int(nullable: false),
            //    })
            //    .PrimaryKey(t => new { t.PermissionRecord_Id, t.CustomerRole_Id });

            //AddColumn("dbo.PermissionRecord", "Category", c => c.String(nullable: false, maxLength: 255));
            //AddColumn("dbo.PermissionRecord", "Name", c => c.String(nullable: false));
            //CreateIndex("dbo.PermissionRecord_Role_Mapping", "CustomerRole_Id");
            //CreateIndex("dbo.PermissionRecord_Role_Mapping", "PermissionRecord_Id");
            //AddForeignKey("dbo.PermissionRecord_Role_Mapping", "CustomerRole_Id", "dbo.CustomerRole", "Id", cascadeDelete: true);
            //AddForeignKey("dbo.PermissionRecord_Role_Mapping", "PermissionRecord_Id", "dbo.PermissionRecord", "Id", cascadeDelete: true);
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            try
            {
                // Note, core scheduled tasks must always be added to the installation as well!
                context.Set<ScheduleTask>().AddOrUpdate(x => x.Type,
                    new ScheduleTask
                    {
                        Name = "Rebuild XML Sitemap",
                        CronExpression = "45 3 * * *",
                        Type = "SmartStore.Services.Seo.RebuildXmlSitemapTask, SmartStore.Services",
                        Enabled = true,
                        StopOnError = false
                    }
                );
                context.SaveChanges();
            }
            catch { }

            if (!HostingEnvironment.IsHosted || !DataSettings.Current.IsSqlServer)
            {
                return;
            }

            Execute(context, "DELETE FROM [dbo].[PermissionRecord] WHERE [Name] <> '' AND [Name] IS NOT NULL");
            Execute(context, "Alter Table [dbo].[PermissionRecord_Role_Mapping] Drop Constraint [FK_dbo.PermissionRecord_Role_Mapping_dbo.PermissionRecord_PermissionRecord_Id]");
            Execute(context, "Alter Table [dbo].[PermissionRecord_Role_Mapping] Drop Constraint [FK_dbo.PermissionRecord_Role_Mapping_dbo.CustomerRole_CustomerRole_Id]");
            Execute(context, "Drop Index [IX_PermissionRecord_Id] ON [dbo].[PermissionRecord_Role_Mapping]");
            Execute(context, "Drop Index [IX_CustomerRole_Id] ON [dbo].[PermissionRecord_Role_Mapping]");
            Execute(context, "Alter Table [dbo].[PermissionRecord] Drop Column [Name]");
            Execute(context, "Alter Table [dbo].[PermissionRecord] Drop Column [Category]");
            Execute(context, "Drop Table [dbo].[PermissionRecord_Role_Mapping]");
        }

        private void Execute(SmartObjectContext context, string sql)
        {
            try
            {
                context.ExecuteSqlCommand(sql);
            }
            catch { }
        }
    }
}

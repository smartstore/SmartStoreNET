namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Web.Hosting;
    using SmartStore.Core.Data;

    public partial class RemoveOldPermissions : DbMigration
    {
        public override void Up()
        {
            if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
            {
                Sql("DELETE FROM [dbo].[PermissionRecord] WHERE [Name] <> '' AND [Name] IS NOT NULL");
            }

            DropForeignKey("dbo.PermissionRecord_Role_Mapping", "PermissionRecord_Id", "dbo.PermissionRecord");
            DropForeignKey("dbo.PermissionRecord_Role_Mapping", "CustomerRole_Id", "dbo.CustomerRole");
            DropIndex("dbo.PermissionRecord_Role_Mapping", new[] { "PermissionRecord_Id" });
            DropIndex("dbo.PermissionRecord_Role_Mapping", new[] { "CustomerRole_Id" });
            DropColumn("dbo.PermissionRecord", "Name");
            DropColumn("dbo.PermissionRecord", "Category");
            DropTable("dbo.PermissionRecord_Role_Mapping");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.PermissionRecord_Role_Mapping",
                c => new
                    {
                        PermissionRecord_Id = c.Int(nullable: false),
                        CustomerRole_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PermissionRecord_Id, t.CustomerRole_Id });
            
            AddColumn("dbo.PermissionRecord", "Category", c => c.String(nullable: false, maxLength: 255));
            AddColumn("dbo.PermissionRecord", "Name", c => c.String(nullable: false));
            CreateIndex("dbo.PermissionRecord_Role_Mapping", "CustomerRole_Id");
            CreateIndex("dbo.PermissionRecord_Role_Mapping", "PermissionRecord_Id");
            AddForeignKey("dbo.PermissionRecord_Role_Mapping", "CustomerRole_Id", "dbo.CustomerRole", "Id", cascadeDelete: true);
            AddForeignKey("dbo.PermissionRecord_Role_Mapping", "PermissionRecord_Id", "dbo.PermissionRecord", "Id", cascadeDelete: true);
        }
    }
}

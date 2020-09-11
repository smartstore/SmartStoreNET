namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Core.Data;
    using SmartStore.Data.Setup;
    using SmartStore.Data.Utilities;

    public partial class Menus : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MenuRecord",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    SystemName = c.String(nullable: false, maxLength: 400),
                    IsSystemMenu = c.Boolean(nullable: false),
                    Template = c.String(maxLength: 400),
                    WidgetZone = c.String(maxLength: 4000),
                    Title = c.String(maxLength: 400),
                    Published = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    LimitedToStores = c.Boolean(nullable: false),
                    SubjectToAcl = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.SystemName, t.IsSystemMenu }, name: "IX_Menu_SystemName_IsSystemMenu")
                .Index(t => t.Published, name: "IX_Menu_Published")
                .Index(t => t.LimitedToStores, name: "IX_Menu_LimitedToStores")
                .Index(t => t.SubjectToAcl, name: "IX_Menu_SubjectToAcl");

            CreateTable(
                "dbo.MenuItemRecord",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    MenuId = c.Int(nullable: false),
                    ParentItemId = c.Int(nullable: false),
                    ProviderName = c.String(maxLength: 100),
                    Model = c.String(),
                    Title = c.String(maxLength: 400),
                    ShortDescription = c.String(maxLength: 400),
                    PermissionNames = c.String(),
                    Published = c.Boolean(nullable: false),
                    DisplayOrder = c.Int(nullable: false),
                    BeginGroup = c.Boolean(nullable: false),
                    ShowExpanded = c.Boolean(nullable: false),
                    NoFollow = c.Boolean(nullable: false),
                    NewWindow = c.Boolean(nullable: false),
                    Icon = c.String(maxLength: 100),
                    Style = c.String(maxLength: 10),
                    HtmlId = c.String(maxLength: 100),
                    CssClass = c.String(maxLength: 100),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MenuRecord", t => t.MenuId, cascadeDelete: true)
                .Index(t => t.MenuId)
                .Index(t => t.ParentItemId, name: "IX_MenuItem_ParentItemId")
                .Index(t => t.Published, name: "IX_MenuItem_Published")
                .Index(t => t.DisplayOrder, name: "IX_MenuItem_DisplayOrder");

        }

        public override void Down()
        {
            DropForeignKey("dbo.MenuItemRecord", "MenuId", "dbo.MenuRecord");
            DropIndex("dbo.MenuItemRecord", "IX_MenuItem_DisplayOrder");
            DropIndex("dbo.MenuItemRecord", "IX_MenuItem_Published");
            DropIndex("dbo.MenuItemRecord", "IX_MenuItem_ParentItemId");
            DropIndex("dbo.MenuItemRecord", new[] { "MenuId" });
            DropIndex("dbo.MenuRecord", "IX_Menu_SubjectToAcl");
            DropIndex("dbo.MenuRecord", "IX_Menu_LimitedToStores");
            DropIndex("dbo.MenuRecord", "IX_Menu_Published");
            DropIndex("dbo.MenuRecord", "IX_Menu_SystemName_IsSystemMenu");
            DropTable("dbo.MenuItemRecord");
            DropTable("dbo.MenuRecord");
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            if (DataSettings.DatabaseIsInstalled())
            {
                DataMigrator.CreateSystemMenus(context);
            }
        }
    }
}

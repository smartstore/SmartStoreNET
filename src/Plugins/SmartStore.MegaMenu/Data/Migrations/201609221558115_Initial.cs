namespace SmartStore.MegaMenu.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MegaMenu",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CategoryId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        DisplayCategoryPicture = c.Boolean(nullable: false),
                        DisplayBgPicture = c.Boolean(nullable: false),
                        BgPictureId = c.Int(nullable: false),
                        BgLink = c.String(maxLength: 2048),
                        BgAlignX = c.Int(nullable: false),
                        BgAlignY = c.Int(nullable: false),
                        BgOffsetX = c.Int(nullable: false),
                        BgOffsetY = c.Int(nullable: false),
                        MaxItemsPerColumn = c.Int(nullable: false),
                        MaxSubItemsPerCategory = c.Int(nullable: false),
                        Summary = c.String(maxLength: 2048),
                        TeaserHtml = c.String(),
                        HtmlColumnSpan = c.Int(nullable: false),
                        TeaserType = c.Int(nullable: false),
                        TeaserRotatorItemSelectType = c.Int(nullable: false),
                        TeaserRotatorProductIds = c.String(maxLength: 512),
                        DisplaySubItemsInline = c.Boolean(nullable: false),
                        AllowSubItemsColumnWrap = c.Boolean(nullable: false),
                        SubItemsWrapTolerance = c.Int(nullable: false),
                        FavorInMegamenu = c.Boolean(nullable: false),
                        CreatedOnUtc = c.DateTime(),
                        UpdatedOnUtc = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            //DropTable("dbo.MegaMenu");
        }
    }
}

namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VariantAttributeValuePictures : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductVariantAttributeValue", "PictureId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductVariantAttributeValue", "PictureId");
        }
    }
}

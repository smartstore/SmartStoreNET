namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PictureSize : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Picture", "Width", c => c.Int());
            AddColumn("dbo.Picture", "Height", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Picture", "Height");
            DropColumn("dbo.Picture", "Width");
        }
    }
}

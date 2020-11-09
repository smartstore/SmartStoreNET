namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class SpecificationAttributeColorAndPicture : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SpecificationAttributeOption", "PictureId", c => c.Int(nullable: false));
            AddColumn("dbo.SpecificationAttributeOption", "Color", c => c.String(maxLength: 100));
            AlterColumn("dbo.Rule", "Operator", c => c.String(nullable: false, maxLength: 20));
        }

        public override void Down()
        {
            AlterColumn("dbo.Rule", "Operator", c => c.String(nullable: false, maxLength: 10));
            DropColumn("dbo.SpecificationAttributeOption", "Color");
            DropColumn("dbo.SpecificationAttributeOption", "PictureId");
        }
    }
}

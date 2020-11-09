namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ManufacturerSubjectToAcl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Manufacturer", "SubjectToAcl", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Manufacturer", "SubjectToAcl");
        }

        public override void Down()
        {
            DropIndex("dbo.Manufacturer", new[] { "SubjectToAcl" });
            DropColumn("dbo.Manufacturer", "SubjectToAcl");
        }
    }
}

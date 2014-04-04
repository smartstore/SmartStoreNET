namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AclPerf : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AclRecord", "IsIdle", c => c.Boolean(nullable: false, defaultValue: false));

			CreateIndex("AclRecord", "IsIdle", false, "IX_AclRecord_IsIdle", false);
			CreateIndex("Category", "SubjectToAcl", false, "IX_Category_SubjectToAcl", false);
			CreateIndex("Product", "SubjectToAcl", false, "IX_Product_SubjectToAcl", false);
			CreateIndex("UrlRecord", new[] { "EntityId", "EntityName", "LanguageId", "IsActive" }, false, "IX_UrlRecord_Custom_1", false);
        }
        
        public override void Down()
        {
			DropIndex("UrlRecord", "IX_UrlRecord_Custom_1");
			DropIndex("Product", "IX_Product_SubjectToAcl");
			DropIndex("Category", "IX_Category_SubjectToAcl");
			DropIndex("AclRecord", "IX_AclRecord_IsIdle");
			
			DropColumn("dbo.AclRecord", "IsIdle");
        }

	}
}

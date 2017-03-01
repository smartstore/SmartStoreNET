namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CustomerGenericAttributesIndexes : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Customer", "LastIpAddress", name: "IX_Customer_LastIpAddress");
            CreateIndex("dbo.Customer", "CreatedOnUtc", name: "IX_Customer_CreatedOn");
            CreateIndex("dbo.Customer", "LastActivityDateUtc", name: "IX_Customer_LastActivity");
            CreateIndex("dbo.GenericAttribute", "Key", name: "IX_GenericAttribute_Key");
        }
        
        public override void Down()
        {
            DropIndex("dbo.GenericAttribute", "IX_GenericAttribute_Key");
            DropIndex("dbo.Customer", "IX_Customer_LastActivity");
            DropIndex("dbo.Customer", "IX_Customer_CreatedOn");
            DropIndex("dbo.Customer", "IX_Customer_LastIpAddress");
        }
    }
}

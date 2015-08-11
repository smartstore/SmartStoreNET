namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSyncMapping : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SyncMapping",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EntityId = c.Int(nullable: false),
                        SourceKey = c.String(nullable: false, maxLength: 150),
                        EntityName = c.String(nullable: false, maxLength: 100),
                        ContextName = c.String(nullable: false, maxLength: 100),
                        SourceHash = c.String(maxLength: 40),
                        CustomInt = c.Int(),
                        CustomString = c.String(),
                        SyncedOnUtc = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => new { t.EntityId, t.EntityName, t.ContextName }, unique: true, name: "IX_SyncMapping_ByEntity")
                .Index(t => new { t.SourceKey, t.EntityName, t.ContextName }, unique: true, name: "IX_SyncMapping_BySource");
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.SyncMapping", "IX_SyncMapping_BySource");
            DropIndex("dbo.SyncMapping", "IX_SyncMapping_ByEntity");
            DropTable("dbo.SyncMapping");
        }
    }
}

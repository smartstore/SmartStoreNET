namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LocalizedPropertyIndex : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.LocalizedProperty", new[] { "LanguageId" });
            CreateIndex("dbo.LocalizedProperty", new[] { "EntityId", "LocaleKey", "LocaleKeyGroup", "LanguageId" }, name: "IX_LocalizedProperty_Compound");
        }
        
        public override void Down()
        {
            DropIndex("dbo.LocalizedProperty", "IX_LocalizedProperty_Compound");
            CreateIndex("dbo.LocalizedProperty", "LanguageId");
        }
    }
}

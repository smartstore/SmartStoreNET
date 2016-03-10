namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LocalizedPropertyUniqueIndex : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.LocalizedProperty", "IX_LocalizedProperty_Compound");
            CreateIndex("dbo.LocalizedProperty", new[] { "EntityId", "LocaleKey", "LocaleKeyGroup", "LanguageId" }, unique: true, name: "IX_LocalizedProperty_Compound");
        }
        
        public override void Down()
        {
            DropIndex("dbo.LocalizedProperty", "IX_LocalizedProperty_Compound");
            CreateIndex("dbo.LocalizedProperty", new[] { "EntityId", "LocaleKey", "LocaleKeyGroup", "LanguageId" }, name: "IX_LocalizedProperty_Compound");
        }
    }
}

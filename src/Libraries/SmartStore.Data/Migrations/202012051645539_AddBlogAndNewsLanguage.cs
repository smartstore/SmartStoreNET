namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBlogAndNewsLanguage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BlogPost", "LanguageId", c => c.Int());
            AddColumn("dbo.News", "LanguageId", c => c.Int());
            CreateIndex("dbo.BlogPost", "LanguageId");
            CreateIndex("dbo.News", "LanguageId");
            AddForeignKey("dbo.BlogPost", "LanguageId", "dbo.Language", "Id");
            AddForeignKey("dbo.News", "LanguageId", "dbo.Language", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.News", "LanguageId", "dbo.Language");
            DropForeignKey("dbo.BlogPost", "LanguageId", "dbo.Language");
            DropIndex("dbo.News", new[] { "LanguageId" });
            DropIndex("dbo.BlogPost", new[] { "LanguageId" });
            DropColumn("dbo.News", "LanguageId");
            DropColumn("dbo.BlogPost", "LanguageId");
        }
    }
}

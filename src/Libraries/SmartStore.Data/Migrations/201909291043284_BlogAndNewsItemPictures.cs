namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class BlogAndNewsItemPictures : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {   
            AddColumn("dbo.BlogPost", "PictureId", c => c.Int());
            AddColumn("dbo.BlogPost", "PreviewPictureId", c => c.Int());
            AddColumn("dbo.BlogPost", "SectionBg", c => c.String());
            AddColumn("dbo.BlogPost", "HasBgImage", c => c.Boolean(nullable: false));
            AddColumn("dbo.BlogPost", "Intro", c => c.String());
            AddColumn("dbo.News", "PictureId", c => c.Int());
            AddColumn("dbo.News", "PreviewPictureId", c => c.Int());
            AddColumn("dbo.News", "SectionBg", c => c.String());
            AddColumn("dbo.News", "HasBgImage", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.News", "HasBgImage");
            DropColumn("dbo.News", "SectionBg");
            DropColumn("dbo.News", "PreviewPictureId");
            DropColumn("dbo.News", "PictureId");
            DropColumn("dbo.BlogPost", "Intro");
            DropColumn("dbo.BlogPost", "HasBgImage");
            DropColumn("dbo.BlogPost", "SectionBg");
            DropColumn("dbo.BlogPost", "PreviewPictureId");
            DropColumn("dbo.BlogPost", "PictureId");
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.ContentManagement.Blog.BlogPosts.Fields.Intro",
                "Intro",
                "Intro",
                "Specifies the intro of the blog post.",
                "Legt das Intro des Blog-Posts fest.");

            builder.AddOrUpdate("Admin.ContentManagement.Blog.BlogPosts.Fields.Picture",
                "Picture",
                "Bild",
                "Specifies the picture of the blog post.",
                "Legt das Bild des Blog-Posts fest.");

            builder.AddOrUpdate("Admin.ContentManagement.Blog.BlogPosts.Fields.PreviewPicture",
                "Preview picture",
                "Vorschaubild",
                "Specifies the preview picture of the blog post.",
                "Legt das Vorschaubild des Blog-Posts fest.");

            builder.AddOrUpdate("Admin.ContentManagement.Blog.BlogPosts.Fields.SectionBg",
                "Background color",
                "Hintergrundfarbe",
                "Specifies the background color of the blog post.",
                "Legt die Hintergrundfarbe des Blog-Posts fest.");

            builder.AddOrUpdate("Admin.ContentManagement.Blog.BlogPosts.Fields.HasBgImage",
                "Has background image",
                "Hat Hintergrundbild",
                "Specifies whether the image of the blog post are displayed in the background of the blog preview.",
                "Legt das Intro des Blog-Posts fest.");


            builder.AddOrUpdate("Admin.ContentManagement.News.NewsItems.Fields.Picture",
                "Picture",
                "Bild",
                "Specifies the picture of the blog post.",
                "Legt das Bild des Blog-Posts fest.");

            builder.AddOrUpdate("Admin.ContentManagement.News.NewsItems.Fields.PreviewPicture",
                "Preview picture",
                "Vorschaubild",
                "Specifies the preview picture of the blog post.",
                "Legt das Vorschaubild des Blog-Posts fest.");

            builder.AddOrUpdate("Admin.ContentManagement.News.NewsItems.Fields.SectionBg",
                "Background color",
                "Hintergrundfarbe",
                "Specifies the background color of the blog post.",
                "Legt die Hintergrundfarbe des Blog-Posts fest.");

            builder.AddOrUpdate("Admin.ContentManagement.News.NewsItems.Fields.HasBgImage",
                "Has background image",
                "Hat Hintergrundbild",
                "Specifies whether the image of the blog post are displayed in the background of the blog preview.",
                "Legt das Intro des Blog-Posts fest.");

            builder.AddOrUpdate("Common.Cms.EditBlogpost",
                "Edit blog post",
                "Blog-Eintrag bearbeiten");

            builder.AddOrUpdate("Common.Cms.EditNewsItem",
                "Edit news item",
                "News-Eintrag bearbeiten");
        }
    }
}

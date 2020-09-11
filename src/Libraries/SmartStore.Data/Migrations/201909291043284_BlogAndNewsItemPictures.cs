namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class BlogAndNewsItemPictures : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.BlogPost", "PictureId", c => c.Int());
            AddColumn("dbo.BlogPost", "PreviewPictureId", c => c.Int());
            AddColumn("dbo.BlogPost", "SectionBg", c => c.String(maxLength: 100));
            AddColumn("dbo.BlogPost", "Intro", c => c.String());
            AddColumn("dbo.BlogPost", "DisplayTagsInPreview", c => c.Boolean(nullable: false));
            AddColumn("dbo.BlogPost", "IsPublished", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.BlogPost", "PreviewDisplayType", c => c.Int(nullable: false));
            AddColumn("dbo.News", "PictureId", c => c.Int());
            AddColumn("dbo.News", "PreviewPictureId", c => c.Int());
        }

        public override void Down()
        {
            DropColumn("dbo.News", "PreviewPictureId");
            DropColumn("dbo.News", "PictureId");
            DropColumn("dbo.BlogPost", "PreviewDisplayType");
            DropColumn("dbo.BlogPost", "IsPublished");
            DropColumn("dbo.BlogPost", "DisplayTagsInPreview");
            DropColumn("dbo.BlogPost", "Intro");
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

            builder.AddOrUpdate("Admin.ContentManagement.News.NewsItems.Fields.Picture",
                "Picture",
                "Bild",
                "Specifies the picture of the news item.",
                "Legt das Bild des News-Eintrags fest.");

            builder.AddOrUpdate("Admin.ContentManagement.News.NewsItems.Fields.PreviewPicture",
                "Preview picture",
                "Vorschaubild",
                "Specifies the preview picture of the news item.",
                "Legt das Vorschaubild des News-Eintrags fest.");

            builder.AddOrUpdate("Admin.ContentManagement.News.Blog.BlogPosts.Fields.PreviewDisplayType",
                "Preview display type",
                "Vorschau-Darstellung",
                "Specifies display type of the preview for a blog item.",
                "Legt die Darstellung der Vorschau für einen Blog-Eintrag fest.");

            builder.AddOrUpdate("Admin.ContentManagement.News.Blog.BlogPosts.Fields.DisplayTagsInPreview",
                "Display tags on preview",
                "Tags in Vorschau anzeigen",
                "Specifies whether tags are display in the preview for blog item.",
                "Bestimmt, ob Tags in der Vorschau eines Blog-Eintrags angezeigt werden.");

            builder.AddOrUpdate("Admin.ContentManagement.News.Blog.BlogPosts.Fields.IsPublished",
                "Is published",
                "Veröffentlicht",
                "Specifies whether the blog post is published and thus will be displayed in the frontend",
                "Bestimmt, ob der Blog-Eintrag veröffentlicht ist und somit im Frontend dargestellt wird.");

            builder.AddOrUpdate("Common.Cms.EditBlogPost",
                "Edit blog post",
                "Blog-Post bearbeiten");

            builder.AddOrUpdate("Common.Cms.EditNewsItem",
                "Edit news item",
                "News-Eintrag bearbeiten");

            builder.AddOrUpdate("Common.Cms.ReadMore",
                "Read more",
                "Mehr lesen");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Blogs.PreviewDisplayType.Bare", "Minimal (no image, no background)", "Minimal (kein Bild, kein Hintergrund)");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Blogs.PreviewDisplayType.Default", "Picture over text", "Bild über Text");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Blogs.PreviewDisplayType.Preview", "Preview picture over text", "Galleriebild über Text");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Blogs.PreviewDisplayType.DefaultSectionBg", "Picture behind text", "Bild hinter Text");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Blogs.PreviewDisplayType.PreviewSectionBg", "Preview picture behind text", "Galleriebild hinter Text");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Blogs.PreviewDisplayType.SectionBg", "Background color", "Hintergrundfarbe");
        }
    }
}

namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixMaxLength : DbMigration
    {
        public override void Up()
        {
			AlterColumn("dbo.BlogComment", "CommentText", c => c.String());
			AlterColumn("dbo.BlogPost", "Body", c => c.String());
			AlterColumn("dbo.Category", "Description", c => c.String());
			AlterColumn("dbo.Manufacturer", "Description", c => c.String());
			AlterColumn("dbo.ProductReview", "ReviewText", c => c.String());
			AlterColumn("dbo.GenericAttribute", "Value", c => c.String());
			AlterColumn("dbo.Setting", "Value", c => c.String());
			AlterColumn("dbo.Forums_Group", "Description", c => c.String());
			AlterColumn("dbo.Forums_Forum", "Description", c => c.String());
			AlterColumn("dbo.Forums_Post", "Text", c => c.String());
			AlterColumn("dbo.Forums_PrivateMessage", "Text", c => c.String());
			AlterColumn("dbo.LocaleStringResource", "ResourceValue", c => c.String());
			AlterColumn("dbo.LocalizedProperty", "LocaleValue", c => c.String());
			AlterColumn("dbo.ActivityLog", "Comment", c => c.String());
			AlterColumn("dbo.Log", "FullMessage", c => c.String());
			AlterColumn("dbo.Campaign", "Body", c => c.String());
			AlterColumn("dbo.MessageTemplate", "Body", c => c.String());
			AlterColumn("dbo.NewsComment", "CommentText", c => c.String());
			AlterColumn("dbo.News", "Full", c => c.String());
			AlterColumn("dbo.OrderItem", "AttributesXml", c => c.String());
			AlterColumn("dbo.OrderNote", "Note", c => c.String());
			AlterColumn("dbo.ShoppingCartItem", "AttributesXml", c => c.String());
			AlterColumn("dbo.Topic", "Body", c => c.String());
        }
        
        public override void Down()
        {
        }
    }
}

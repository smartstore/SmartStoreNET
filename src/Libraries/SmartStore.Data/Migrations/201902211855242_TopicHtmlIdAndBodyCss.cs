namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class TopicHtmlIdAndBodyCss : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Topic", "HtmlId", c => c.String(maxLength: 128));
            AddColumn("dbo.Topic", "BodyCssClass", c => c.String(maxLength: 512));
        }

        public override void Down()
        {
            DropColumn("dbo.Topic", "BodyCssClass");
            DropColumn("dbo.Topic", "HtmlId");
        }
    }
}

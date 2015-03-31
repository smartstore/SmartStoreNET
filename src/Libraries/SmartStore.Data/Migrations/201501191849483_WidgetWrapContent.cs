namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class WidgetWrapContent : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Topic", "WidgetWrapContent", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Topic", "WidgetWrapContent");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.ContentManagement.Topics.Fields.WidgetWrapContent",
				"Add wrapper around content",
				"Inhalt mit Container umhüllen",
				"Adds an HTML wrapper around widget content",
				"Umhüllt den Widget Inhalt mit einem HTML-Container");
		}
	}
}

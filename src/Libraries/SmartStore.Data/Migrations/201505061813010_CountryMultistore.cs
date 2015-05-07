namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class CountryMultistore : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Country", "LimitedToStores", c => c.Boolean(nullable: false));
            AddColumn("dbo.Forums_Group", "LimitedToStores", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Forums_Group", "LimitedToStores");
            DropColumn("dbo.Country", "LimitedToStores");
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
			builder.AddOrUpdate("Admin.ContentManagement.Forums.ForumGroup.Fields.SeName",
				"URL-Alias",
				"URL-Alias",
				"Set a search engine friendly page name e.g. 'the-best-forum' to make your page URL 'http://www.yourStore.com/the-best-forum'. Leave empty to generate it automatically based on the name of the forum group.",
				"Legt einen Suchmaschinen-freundlichen Seitennamen für die Forengruppe fest. 'Tolles Forum' resultiert bspw. in '~/tolles-forum'. Standard ist der Name der Forengruppe.");
		}
    }
}

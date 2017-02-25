namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using System.Web.Hosting;
	using SmartStore.Core.Data;
	using SmartStore.Data.Setup;

	public partial class SortFilterHomepageProducts : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "HomePageDisplayOrder", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "HomePageDisplayOrder");
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
			builder.AddOrUpdate("Common.Unspecified",
				"Unspecified",
				"Nicht spezifiziert");

			builder.AddOrUpdate("Admin.Catalog.Products.List.SearchIsPublished",
				"Published",
				"Veröffentlicht",
				"Filters for published or unpublished products.",
				"Filtert nach veröffentlichten bzw. unveröffentlichten Produkten.");

			builder.AddOrUpdate("Admin.Catalog.Products.List.SearchHomePageProducts",
				"Showed on home page",
				"Auf Homepage angezeigt",
				"Filters for products displayed or not displayed on homepage.",
				"Filtert nach Produkten, die auf der Homepage angezeigt oder nicht angezeigt werden.");

			builder.AddOrUpdate("Admin.Catalog.Products.Fields.HomePageDisplayOrder",
				"Homepage display order",
				"Homepage Reihenfolge",
				"Specifies the display order for products displayed on homepage. 1 represents the first element in the list.",
				"Legt die Anzeige-Reihenfolge der Produkte auf der Homepage fest (1 steht bspw. für das erste Element in der Liste).");
		}
    }
}

namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using System.Web.Hosting;
	using SmartStore.Core.Data;
	using SmartStore.Data.Setup;

	public partial class NewCategoryProperties : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Category", "FullName", c => c.String(maxLength: 400));
			AddColumn("dbo.Category", "BottomDescription", c => c.String());

			AddColumn("dbo.CheckoutAttribute", "IsActive", c => c.Boolean(nullable: false, defaultValue: true));

			AddColumn("dbo.ReturnRequest", "AdminComment", c => c.String(maxLength: 4000));
			AddColumn("dbo.ReturnRequest", "RequestedActionUpdatedOnUtc", c => c.DateTime());

			if (HostingEnvironment.IsHosted && DataSettings.Current.IsSqlServer)
			{
				this.SqlFile("LatestProductLoadAllPaged.sql");
			}
        }
        
        public override void Down()
        {
			// inverse of LatestProductLoadAllPaged.sql does not make sense to me

			DropColumn("dbo.ReturnRequest", "RequestedActionUpdatedOnUtc");
			DropColumn("dbo.ReturnRequest", "AdminComment");

			DropColumn("dbo.CheckoutAttribute", "IsActive");

			DropColumn("dbo.Category", "BottomDescription");
			DropColumn("dbo.Category", "FullName");
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
			builder.AddOrUpdate("Admin.Catalog.Categories.Fields.FullName",
				"Complete name",
				"Vollständiger Name",
				"Complete name displayed as title on the category page.",
				"Vollständiger Name, der als Titel auf der Warengruppenseite angezeigt wird.");

			builder.AddOrUpdate("Admin.Catalog.Categories.Fields.BottomDescription",
				"Bottom description",
				"Untere Beschreibung",
				"Optional second description displayed below products on the category page.",
				"Optionale zweite Beschreibung, die auf der Warengruppenseite unterhalb der Produkte angezeigt wird.");

			builder.AddOrUpdate("Admin.Catalog.Products.List.SearchWithoutCategories",
				"Without category mapping",
				"Ohne Warengruppenzuordnung",
				"Filters for products without category mapping.",
				"Filtert nach Produkten ohne Warengruppenzuordnung.");

			builder.AddOrUpdate("Admin.Catalog.Products.List.SearchWithoutManufacturers",
				"Without manufacturer mapping",
				"Ohne Herstellerzuordnung",
				"Filters for products without manufacturer mapping.",
				"Filtert nach Produkten ohne Herstellerzuordnung.");

			builder.AddOrUpdate("Admin.Common.AdminComment",
				"Admin comment",
				"Admin-Kommentar",
				"Admin comment for internal use. Won't be published.",
				"Kommentar für internen Gebrauch. Wird nicht veröffentlicht.");

			builder.AddOrUpdate("Admin.ReturnRequests.Fields.RequestedActionUpdatedOnUtc",
				"Last update of requested action",
				"Letzte Aktualisierung der angeforderten Aktion",
				"Date when the requested action was updated the last time.",
				"Datum, an dem die angeforderte Aktion zuletzt geändert wurde.");
		}
    }
}

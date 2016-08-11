namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using Setup;

	public partial class ManufacturerDiscounts : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
		public override void Up()
		{
			CreateTable(
				"dbo.Discount_AppliedToManufacturers",
				c => new
				{
					Discount_Id = c.Int(nullable: false),
					Manufacturer_Id = c.Int(nullable: false),
				})
				.PrimaryKey(t => new { t.Discount_Id, t.Manufacturer_Id })
				.ForeignKey("dbo.Discount", t => t.Discount_Id, cascadeDelete: true)
				.ForeignKey("dbo.Manufacturer", t => t.Manufacturer_Id, cascadeDelete: true)
				.Index(t => t.Discount_Id)
				.Index(t => t.Manufacturer_Id);

			AddColumn("dbo.Manufacturer", "HasDiscountsApplied", c => c.Boolean(nullable: false));
		}

		public override void Down()
		{
			DropForeignKey("dbo.Discount_AppliedToManufacturers", "Manufacturer_Id", "dbo.Manufacturer");
			DropForeignKey("dbo.Discount_AppliedToManufacturers", "Discount_Id", "dbo.Discount");
			DropIndex("dbo.Discount_AppliedToManufacturers", new[] { "Manufacturer_Id" });
			DropIndex("dbo.Discount_AppliedToManufacturers", new[] { "Discount_Id" });
			DropColumn("dbo.Manufacturer", "HasDiscountsApplied");
			DropTable("dbo.Discount_AppliedToManufacturers");
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
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Discounts.DiscountType.AssignedToManufacturers",
				"Assigned to manufacturers",
				"Bezogen auf die Hersteller");

			builder.AddOrUpdate("Admin.Promotions.Discounts.NoObjectsAssigned",
				"No objects assigned",
				"Keinen Objekten zugeordnet");

			builder.AddOrUpdate("Admin.Promotions.Discounts.Fields.AppliedToManufacturers",
				"Assigned to manufacturers",
				"Herstellern zugeordnet");

			builder.AddOrUpdate("Admin.Promotions.Discounts.NoDiscountsAvailable",
				"There are no discounts available. Please create at least one discount before making an assignment.",
				"Es sind keine Rabatte verfügbar. Erstellen Sie bitte zunächst mindestens einen Rabatt, bevor Sie eine Zuordung vornehmen.");

			builder.Delete(
				"Admin.Catalog.Categories.Discounts.NoDiscounts",
				"Admin.Catalog.Products.Discounts.NoDiscounts",
				"Admin.Promotions.Discounts.Fields.AppliedToProducts.NoRecords",
				"Admin.Promotions.Discounts.Fields.AppliedToCategories.NoRecords"
			);
		}
	}
}

namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class LowestAttributeCombinationPrice : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
			AddColumn("dbo.Product", "LowestAttributeCombinationPrice", c => c.Decimal(nullable: true, precision: 18, scale: 4));
        }
        
        public override void Down()
        {
			DropColumn("dbo.Product", "LowestAttributeCombinationPrice");
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
			builder.AddOrUpdate("Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.DeleteAllCombinations",
				"Delete all combinations",
				"Alle Kombinationen löschen");

			builder.AddOrUpdate("Admin.Catalog.Products.ProductVariantAttributes.AttributeCombinations.AskToDeleteAll",
				"Would you like to delete all attribute combinations for this product?",
				"Möchten Sie sämtliche Attribut-Kombinationen für dieses Produkt löschen?");
		}
    }
}

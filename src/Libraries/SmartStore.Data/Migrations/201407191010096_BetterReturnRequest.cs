namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class BetterReturnRequest : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
			AddColumn("dbo.Order", "UpdatedOnUtc", c => c.DateTime(nullable: false));

			try
			{
				Sql("Update [dbo].[Order] Set UpdatedOnUtc = CreatedOnUtc");
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
        }
        
        public override void Down()
        {
			DropColumn("dbo.Order", "UpdatedOnUtc");
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
			builder.Delete("Admin.Orders.Products.ReturnRequests");

			builder.AddOrUpdate("Admin.Orders.Products.ReturnRequest",
				"Return request",
				"Rücksendeauftrag");

			builder.AddOrUpdate("Admin.Orders.Products.ReturnRequest.Create",
				"Create return",
				"Rücksendung erstellen");

			builder.AddOrUpdate("Admin.ReturnRequests.Accept",
				"Accept and update order",
				"Akzeptieren und Auftrag anpassen");

			builder.AddOrUpdate("Admin.ReturnRequests.Accept.Hint",
				"Sets the status to accepted and updates quantities and totals of the order, same with the inventory and reward points.",
				"Setzt den Status auf genehmigt und passt die Mengen und Summen des Auftrags, so wie Warenbestände und Bonuspunkte entsprechend an.");
		}
    }
}

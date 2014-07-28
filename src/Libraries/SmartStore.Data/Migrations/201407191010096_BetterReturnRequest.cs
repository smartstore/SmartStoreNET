namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class BetterReturnRequest : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
        }
        
        public override void Down()
        {
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

			builder.AddOrUpdate("Admin.ReturnRequests.Accept.Confirm",
				"Accept return request? Quantities and totals of the order are updated, same with the inventory and reward points.",
				"Warenrücksendung akzeptieren? Die Mengen und Summen des Auftrags, so wie Warenbestände und Bonuspunkte werden entsprechend angepasst.");
		}
    }
}

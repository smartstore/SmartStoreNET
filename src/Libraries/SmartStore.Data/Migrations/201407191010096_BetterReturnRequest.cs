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
			AddColumn("dbo.Order", "RewardPointsRemaining", c => c.Int());

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
				"Accept",
				"Akzeptieren");

			builder.AddOrUpdate("Admin.ReturnRequests.Accept.Caption",
				"Accept return request",
				"Rücksendung akzeptieren");


			builder.AddOrUpdate("Admin.Orders.OrderItem.Cancel.Info",
				"Stock quantity old {0}, new {1}.<br />Reward points old {2}, new {3}.",
				"Lagerbestand alt {0}, neu {1}.<br />Bonuspunkte alt {2}, neu {3}.");

			builder.AddOrUpdate("Admin.Orders.OrderItem.Cancel.Caption",
				"Remove product",
				"Produkt entfernen");

			builder.AddOrUpdate("Admin.Orders.OrderItem.Cancel.Fields.AdjustInventory",
				"Adjust inventory",
				"Lagerbestand anpassen",
				"Determines whether to increase the stock quantity.",
				"Legt fest, ob der Lagerbestand wieder erhöht werden soll.");

			builder.AddOrUpdate("Admin.Orders.OrderItem.Cancel.Fields.ReduceRewardPoints",
				"Adjust reward points",
				"Bonuspunkte anpassen",
				"Determines whether granted reward points should be deducted again proportionately.",
				"Legt fest, ob gewährte Bonuspunkte wieder anteilig abgezogen werden sollen.");
		}
    }
}

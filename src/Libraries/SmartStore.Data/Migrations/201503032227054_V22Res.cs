namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;
	using System.Linq;

	public partial class V22Res : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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
			// Checkout
			builder.Delete("Checkout.YourOrderHasBeenSuccessfullyProcessed");
			builder.AddOrUpdate("Checkout.OrderHasBeenReceived",
				"Your order has been received",
				"Ihre Bestellung ist angekommen");
			builder.AddOrUpdate("Checkout.ThankYou",
				"Thank you for your purchase!",
				"Vielen Dank für Ihren Einkauf!");
			builder.AddOrUpdate("Checkout.OrderNumber",
				"Your order number",
				"Ihre Bestellnummer");
			builder.AddOrUpdate("Checkout.PlacedOrderDetails",
				"Order details",
				"Bestelldetails");
			builder.AddOrUpdate("Checkout.Continue",
				"Continue shopping",
				"Weiter einkaufen");

			// Move pictures
			builder.Delete("Admin.Configuration.Settings.Media.PicturesStoredIntoDatabase.Hint");
			builder.AddOrUpdate("Common.Shrink",
				"Shrink",
				"Verkleinern");
			builder.AddOrUpdate("Common.ShrinkDatabaseSuccessful",
				"The database has been successfully shrinked",
				"Die Datenbank wurde erfolgreich verkleinert");
			builder.AddOrUpdate("Admin.Configuration.Settings.Media.MovePicturesNote",
				"Do not forget to backup your database before changing this option. Please bear in mind that this operation can take several minutes depending on the amount of images.",
				"Bitte sichern Sie Ihre Datenbank, ehe Sie Mediendateien verschieben. Dieser Vorgang kann je nach Menge der Bilddaten mehrere Minuten in Anspruch nehmen.");
			builder.AddOrUpdate("Admin.Configuration.Settings.Media.MoveToDb",
				"Move to database",
				"In Datenbank verschieben");
			builder.AddOrUpdate("Admin.Configuration.Settings.Media.MoveToFs",
				"Move to file system",
				"Ins Dateisystem verschieben");
		}
	}
}

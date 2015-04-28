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

			// Misc
			builder.AddOrUpdate("Mobile.ViewFullSite",
				"Desktop version",
				"Desktop Version");
			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.FilterEnabled",
				"Display filter sidebar widget",
				"Filter Sidebar Widget anzeigen");
			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.FilterEnabled.Hint",
				"Displays the filter sidebar widget for products within categories",
				"Aktiviert das Filter Sidebar Widget zum Filtern von Produkten in Warengruppen");
			builder.AddOrUpdate("Admin.Catalog.Products.SpecificationAttributes.Fields.ShowOnProductPage.Hint")
				.Value("en", "Check to display the attribute in the public product detail page");
			builder.AddOrUpdate("Admin.Catalog.Products.SpecificationAttributes.AddNew",
				"Assign a specification attribute",
				"Spezifikationsattribut zuordnen");
			builder.AddOrUpdate("Admin.Catalog.Attributes.SpecificationAttributes.Bundled.ShowNotOnProductPage")
				.Value("en", "Don't show on product page");

			builder.AddOrUpdate("Admin.Configuration.Themes.Option.SaveThemeChoiceInCookie",
				"Save theme choice in cookie",
				"Benutzer Theme in Cookie speichern");
			builder.AddOrUpdate("Admin.Configuration.Themes.Option.SaveThemeChoiceInCookie.Hint",
				"If unchecked, user's theme choice is associated to the account, which may be undesirable when, for example, multiple users share a guest account.",
				"Wenn nicht gewählt, wird das Benutzer Theme mit dem Kundenkonto verknüpft, was u.U. unerwünscht sein kann, wenn sich bspw. mehrere User einen Gastzugang teilen.");

			builder.AddOrUpdate("ShoppingCart.SelectAttribute",
				"Please select '{0}'.",
				"Bitte '{0}' auswählen.");

			builder.AddOrUpdate("ShoppingCart.AttributeError",
				"An unknown error has occurred in the examination of the selected product variant.",
				"Ein unbekannter Fehler ist bei der Prüfung der ausgewählten Produktvariante aufgetreten.");

			builder.AddOrUpdate("Common.Execution",
				"Execution",
				"Ausführung");

			builder.AddOrUpdate("Admin.Catalog.Attributes.SpecificationAttributes.OptionsCount",
				"Number options",
				"Anzahl Optionen");

			builder.AddOrUpdate("Admin.Common.DeleteSelected",
				"Delete selected",
				"Ausgewählte löschen");
		}
	}
}

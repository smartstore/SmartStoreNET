namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class PaymentMethodDescription : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.PaymentMethod", "FullDescription", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.PaymentMethod", "FullDescription");
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
			builder.AddOrUpdate("Admin.Configuration.Payment.Methods.ShortDescription",
				"Short description",
				"Kurzbeschreibung",
				"Specifies a short description of the payment method.",
				"Legt eine Kurzbeschreibung der Zahlungsmethode fest.");

			builder.AddOrUpdate("Admin.Configuration.Payment.Methods.FullDescription",
				"Full description",
				"Langtext",
				"Specifies a full description of the payment method. It appears in the payment list in checkout.",
				"Legt eine vollständige Beschreibung der Zahlungsmethode fest. Sie erscheint in der Zahlungsliste im Kassenbereich.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.PriceDisplayType",
				"Price display",
				"Preisanzeige",
				"Specifies whether or what type of price to be displayed in product lists.",
				"Legt fest, ob bzw. welcher Typ von Preis in Produktlisten angezeigt werden soll.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.PriceDisplayType.LowestPrice",
				"Minimum feasible price",
				"Minimal realisierbarer Preis");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.PriceDisplayType.PreSelectedPrice",
				"Price preselected on detail page",
				"Auf der Detailseite vorgewählter Preis");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.PriceDisplayType.PriceWithoutDiscountsAndAttributes",
				"Price without discounts and attributes",
				"Preis ohne Rabatte und Attribute");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.PriceDisplayType.Hide",
				"No price indication",
				"Keine Preisanzeige");
		}
    }
}

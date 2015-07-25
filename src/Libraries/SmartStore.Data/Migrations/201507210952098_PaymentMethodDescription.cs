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
		}
    }
}

namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class CustomerNameFormat : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.CustomerNameFormat.ShowFirstName",
				"Show first name",
				"Vornamen anzeigen");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Customers.CustomerNameFormat.ShowNameAndCity",
				"Show shorted name and city",
				"Gekürzten Namen und Ort anzeigen");

			builder.AddOrUpdate("Common.ComingFrom",
				"from",
				"aus");

			builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.CustomerNameFormatMaxLength",
				"Maximum length of the customer name",
				"Maximale Länge des Benutzernamens",
				"Determines the maximum length of the displayed customer name.",
				"Legt die maximale Länge des angezeigten Benutzernamens fest.");
		}
    }
}
